using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.ViewModels;
using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس گزارش‌های تحلیلی، KPI، و آمار عملکرد سالن
/// </summary>
public class ReportService : IReportService
{
    private readonly DatabaseContext _db;

    public ReportService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<DashboardKpiDto> GetKpiStatisticsAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var rsvList = await _db.Tbl_Reservation
            .AsNoTracking()
            .Where(r => !r.IsDelete)
            .ToListAsync(ct);

        var totalCustomers = await _db.Tbl_Customer
            .AsNoTracking()
            .CountAsync(c => !c.IsDelete, ct);

        var todayReservations = rsvList.Count(r => r.ReserveDate.Date == today);
        var pendingReservations = rsvList.Count(r => r.Status == ReservationStatus.PendingPayment);
        var completedReservations = rsvList.Count(r => r.Status == ReservationStatus.Confirmed ||
                                                       r.Status == ReservationStatus.PaymentSuccessful ||
                                                       r.Status == ReservationStatus.Done);
        var canceledReservations = rsvList.Count(r => r.Status == ReservationStatus.Canceled);
        var totalIncome = rsvList
            .Where(r => r.PaymentStatus == PaymentStatus.Paid)
            .Sum(r => r.PaidAmount > 0 ? r.PaidAmount : r.PayableAmount);

        var totalRefunds = rsvList.Sum(r => r.RefundAmount);
        var netIncome = Math.Max(0, totalIncome - totalRefunds);

        var activePersonnel = await _db.Tbl_Personal
            .AsNoTracking()
            .CountAsync(p => !p.IsDelete && p.IsActive, ct);

        var activeServices = await _db.Tbl_SalonSerice
            .AsNoTracking()
            .CountAsync(s => !s.IsDelete && s.IsActive, ct);

        return new DashboardKpiDto
        {
            TotalCustomers = totalCustomers,
            TodayReservations = todayReservations,
            PendingReservations = pendingReservations,
            CompletedReservations = completedReservations,
            CanceledReservations = canceledReservations,
            TotalIncome = totalIncome,
            TotalRefunds = totalRefunds,
            NetIncome = netIncome,
            ActivePersonnel = activePersonnel,
            ActiveServices = activeServices
        };
    }

    public async Task<ReportSummaryDto> GetPeriodicSummaryAsync(string period = "monthly", CancellationToken ct = default)
    {
        var rsvList = await _db.Tbl_Reservation
            .AsNoTracking()
            .Where(r => !r.IsDelete)
            .ToListAsync(ct);

        var serviceBreakdown = rsvList
            .GroupBy(r => r.ServiceName)
            .Select(g => new TopServiceStatDto
            {
                ServiceName = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(x => x.PaidAmount > 0 ? x.PaidAmount : x.PayableAmount)
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        return new ReportSummaryDto
        {
            Period = period,
            TotalCount = rsvList.Count,
            TotalRevenue = rsvList.Sum(x => x.PaidAmount),
            TopServices = serviceBreakdown
        };
    }

    public async Task<List<StaffPerformanceDto>> GetStaffPerformanceReportAsync(CancellationToken ct = default)
    {
        var staffList = await _db.Tbl_Personal
            .AsNoTracking()
            .Where(p => !p.IsDelete && p.IsActive)
            .ToListAsync(ct);

        var reservations = await _db.Tbl_Reservation
            .AsNoTracking()
            .Where(r => !r.IsDelete)
            .ToListAsync(ct);

        var result = new List<StaffPerformanceDto>();

        foreach (var staff in staffList)
        {
            var staffReservations = reservations.Where(r => r.PersonalTC == staff.Tc).ToList();
            var totalRevenue = staffReservations.Sum(r => r.PaidAmount > 0 ? r.PaidAmount : r.PayableAmount);

            result.Add(new StaffPerformanceDto
            {
                PersonalTc = staff.Tc,
                StaffName = staff.FullName,
                Specialty = staff.Specialty,
                TotalReservations = staffReservations.Count,
                TotalRevenue = totalRevenue,
                SatisfactionRating = 4.8
            });
        }

        return result.OrderByDescending(x => x.TotalReservations).ToList();
    }
}
