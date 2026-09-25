using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.ViewModels;

namespace DataCore.Interfaces;

public interface IReportService
{
    Task<DashboardKpiDto> GetKpiStatisticsAsync(CancellationToken ct = default);

    Task<ReportSummaryDto> GetPeriodicSummaryAsync(string period = "monthly", CancellationToken ct = default);

    Task<List<StaffPerformanceDto>> GetStaffPerformanceReportAsync(CancellationToken ct = default);
}
