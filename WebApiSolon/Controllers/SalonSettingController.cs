using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>
/// کنترلر RESTful مدیریت تنظیمات عمومی سالن (بندهای ۱، ۲، ۴، ۶، ۲۲ و ۲۳ سند)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalonSettingController : BaseApiController
{
    private readonly ISalonSettingService _settingService;

    public SalonSettingController(ISalonSettingService settingService)
    {
        _settingService = settingService;
    }

    /// <summary>
    /// دریافت تنظیمات عمومی سالن (عمومی)
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<Tbl_SalonSetting>> GetSettings(CancellationToken ct = default)
    {
        Response.Headers.Append("Link", "</api/salonsetting>; rel=\"self\"");
        var settings = await _settingService.GetSettingsAsync(ct);
        return Ok(settings);
    }

    /// <summary>
    /// ذخیره و به‌روزرسانی تنظیمات سالن (مخصوص ادمین)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> UpdateSettings(
        [FromBody] SalonSettingViewModel model,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ok = await _settingService.UpdateSettingsAsync(model, ct);
        if (!ok)
        {
            return BadRequest(new { Success = false, Message = "به‌روزرسانی تنظیمات با شکست مواجه شد" });
        }

        return Ok(new { Success = true, Message = "تنظیمات سالن با موفقیت ذخیره شد" });
    }
}
