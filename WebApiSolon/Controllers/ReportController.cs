using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>
/// کنترلر RESTful گزارش‌های آماری، شاخص‌های کلیدی عملکرد (KPI) و درآمد سالن (بند ۱۸ سند)
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ReportController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// دریافت شاخص‌های کلیدی عملکرد (KPI) داشبورد مدیر
    /// </summary>
    [HttpGet("kpi")]
    public async Task<ActionResult<DashboardKpiDto>> GetKpi(CancellationToken ct = default)
    {
        Response.Headers.Append("Link", "</api/report/kpi>; rel=\"self\", </api/report/summary>; rel=\"summary\"");
        var kpi = await _reportService.GetKpiStatisticsAsync(ct);
        return Ok(kpi);
    }

    /// <summary>
    /// گزارش‌های دوره‌ای عملکرد و درآمد سالن (روزانه، هفتگی، ماهانه، سالانه)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary(
        [FromQuery] string period = "monthly",
        CancellationToken ct = default)
    {
        Response.Headers.Append("Link", $"</api/report/summary?period={period}>; rel=\"self\"");
        var summary = await _reportService.GetPeriodicSummaryAsync(period, ct);
        return Ok(summary);
    }

    /// <summary>
    /// گزارش عملکرد و درآمد تفکیکی پرسنل سالن
    /// </summary>
    [HttpGet("staff-performance")]
    public async Task<ActionResult<List<StaffPerformanceDto>>> GetStaffPerformance(CancellationToken ct = default)
    {
        var list = await _reportService.GetStaffPerformanceReportAsync(ct);
        return Ok(list);
    }
}
