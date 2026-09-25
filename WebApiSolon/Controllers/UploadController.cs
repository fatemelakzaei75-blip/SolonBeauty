using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiSolon.Services;

namespace WebApiSolon.Controllers;

/// <summary>
/// کنترلر RESTful مدیریت بارگذاری و حذف فایل‌ها در ریشه وب‌سرویس
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Personnel")]
public class UploadController : BaseApiController
{
    private readonly FileStorageService _storage;

    public UploadController(FileStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// بارگذاری یک تصویر با حداکثر حجم ۵ مگابایت و جلوگیری از فایل‌های تکراری با هش SHA-256
    /// </summary>
    [HttpPost("image")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult> UploadImage(
        IFormFile file,
        [FromQuery] string section = "portfolio",
        CancellationToken ct = default)
    {
        var (ok, message) = _storage.Validate(file);
        if (!ok)
        {
            return BadRequest(new { Success = false, Message = message });
        }

        var stored = await _storage.SaveAsync(file, section, ct);

        Response.Headers.Append("Link", $"<{stored.RelativeUrl}>; rel=\"file\"");

        return Ok(new
        {
            Success = true,
            Message = stored.AlreadyExisted
                ? "این تصویر قبلاً آپلود شده بود و از نسخه موجود استفاده گردید"
                : "تصویر با موفقیت آپلود شد",
            Data = new
            {
                url = stored.RelativeUrl,
                fileName = stored.FileName,
                hash = stored.Hash,
                size = stored.Size,
                duplicate = stored.AlreadyExisted
            }
        });
    }

    /// <summary>
    /// بارگذاری چند تصویر همزمان
    /// </summary>
    [HttpPost("images")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<ActionResult> UploadImages(
        [FromForm] IFormFileCollection files,
        [FromQuery] string section = "portfolio",
        CancellationToken ct = default)
    {
        var results = new List<object>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            var (ok, message) = _storage.Validate(file);
            if (!ok)
            {
                errors.Add($"{file.FileName}: {message}");
                continue;
            }

            var stored = await _storage.SaveAsync(file, section, ct);
            results.Add(new
            {
                url = stored.RelativeUrl,
                hash = stored.Hash,
                duplicate = stored.AlreadyExisted
            });
        }

        return Ok(new
        {
            Success = errors.Count == 0,
            Message = errors.Count == 0 ? $"{results.Count} تصویر آپلود شد" : string.Join(" | ", errors),
            Data = results
        });
    }

    /// <summary>
    /// فهرست فایل‌های بارگذاری شده در یک بخش
    /// </summary>
    [HttpGet("list")]
    public ActionResult List([FromQuery] string section = "portfolio")
    {
        var files = _storage.List(section);
        return Ok(new { Success = true, Data = files });
    }

    /// <summary>
    /// حذف فایل ذخیره‌شده بر اساس آدرس نسبی (مخصوص ادمین)
    /// </summary>
    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public ActionResult Delete([FromQuery] string url)
    {
        var ok = _storage.Delete(url);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "تصویر یافت نشد" });
        }

        return Ok(new { Success = true, Message = "تصویر با موفقیت حذف گردید" });
    }
}
