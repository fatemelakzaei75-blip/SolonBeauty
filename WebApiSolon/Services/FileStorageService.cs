using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebApiSolon.Services;

/// <summary>
/// پیاده‌سازی سرویس ذخیره‌سازی فایل‌ها با نام‌گذاری هش یکتا و محافظت در برابر Path Traversal
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileStorageService> _logger;

    public const string RootFolder = "uploads";
    private const long MaxBytes = 5 * 1024 * 1024;   // ۵ مگابایت

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    private string WebRoot => string.IsNullOrWhiteSpace(_env.WebRootPath)
        ? Path.Combine(_env.ContentRootPath, "wwwroot")
        : _env.WebRootPath;

    public (bool Ok, string Message) Validate(IFormFile file)
    {
        if (file is null || file.Length == 0) return (false, "فایلی ارسال نشده است");
        return ValidateFile(file.FileName, file.ContentType, file.Length);
    }

    public (bool Ok, string Message) ValidateFile(string fileName, string contentType, long length)
    {
        if (length <= 0) return (false, "فایلی ارسال نشده است");
        if (length > MaxBytes) return (false, "حجم تصویر نباید بیشتر از ۵ مگابایت باشد");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext)) return (false, "فرمت مجاز: jpg, jpeg, png, webp, gif");
        if (!AllowedContentTypes.Contains(contentType.ToLowerInvariant())) return (false, "نوع فایل تصویری معتبر نیست");

        return (true, string.Empty);
    }

    public async Task<FileStorageResult> SaveAsync(IFormFile file, string section = "portfolio", CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream();
        return await SaveFileAsync(stream, file.FileName, section, ct);
    }

    public async Task<FileStorageResult> SaveFileAsync(
        Stream stream,
        string originalFileName,
        string section = "portfolio",
        CancellationToken ct = default)
    {
        section = SanitizeSection(section);

        using var sha = SHA256.Create();
        var hashBytes = await sha.ComputeHashAsync(stream, ct);
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        var now = DateTime.UtcNow;
        var relativeDir = Path.Combine(RootFolder, section, now.ToString("yyyy"), now.ToString("MM"));
        var absoluteDir = Path.Combine(WebRoot, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{hash}{ext}";
        var absolutePath = Path.Combine(absoluteDir, fileName);
        var relativeUrl = "/" + Path.Combine(relativeDir, fileName).Replace('\\', '/');

        if (File.Exists(absolutePath))
        {
            _logger.LogInformation("تصویر تکراری بود و از نسخه موجود استفاده شد: {File}", relativeUrl);
            return new FileStorageResult(fileName, relativeUrl, hash, new FileInfo(absolutePath).Length, true);
        }

        stream.Position = 0;
        await using (var target = File.Create(absolutePath))
        {
            await stream.CopyToAsync(target, ct);
        }

        return new FileStorageResult(fileName, relativeUrl, hash, stream.Length, false);
    }

    public bool DeleteFile(string relativeUrl)
    {
        return Delete(relativeUrl);
    }

    public bool Delete(string relativeUrl)
    {
        try
        {
            var clean = relativeUrl.TrimStart('/', '\\');
            if (!clean.StartsWith(RootFolder, StringComparison.OrdinalIgnoreCase)) return false;

            var path = Path.Combine(WebRoot, clean.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) return false;

            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "حذف فایل ناموفق بود: {Url}", relativeUrl);
            return false;
        }
    }

    public List<string> ListFiles(string section = "portfolio")
    {
        return List(section);
    }

    public List<string> List(string section = "portfolio")
    {
        section = SanitizeSection(section);
        var dir = Path.Combine(WebRoot, RootFolder, section);
        if (!Directory.Exists(dir)) return new List<string>();

        return Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
            .Select(f => "/" + Path.GetRelativePath(WebRoot, f).Replace('\\', '/'))
            .OrderByDescending(f => f)
            .ToList();
    }

    private static string SanitizeSection(string section)
    {
        if (string.IsNullOrWhiteSpace(section)) return "portfolio";
        var clean = new string(section.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
        return string.IsNullOrEmpty(clean) ? "portfolio" : clean.ToLowerInvariant();
    }
}
