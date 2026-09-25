using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;

namespace BlazorAppSolon.Services;

/// <summary>
/// سرویس کلاینت Blazor جهت دریافت آمار KPI و گزارش‌های مدیریتی
/// </summary>
public class ReportApiService : BaseApiService
{
    public ReportApiService(HttpClient http) : base(http)
    {
    }

    public async Task<ApiResponse<DashboardKpiDto>> GetKpiAsync(CancellationToken ct = default)
    {
        return await GetAsync<DashboardKpiDto>("api/report/kpi", ct);
    }

    public async Task<ApiResponse<ReportSummaryDto>> GetSummaryAsync(string period = "monthly", CancellationToken ct = default)
    {
        return await GetAsync<ReportSummaryDto>($"api/report/summary?period={period}", ct);
    }

    public async Task<ApiResponse<List<StaffPerformanceDto>>> GetStaffPerformanceAsync(CancellationToken ct = default)
    {
        return await GetAsync<List<StaffPerformanceDto>>("api/report/staff-performance", ct);
    }
}
