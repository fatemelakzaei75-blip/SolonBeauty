using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس لیست انتظار نوبت‌های پر شده
/// </summary>
public class WaitingListService : BaseRepository<Tbl_WaitingList>, IWaitingListService
{
    public WaitingListService(DatabaseContext db) : base(db)
    {
    }

    public async Task<List<Tbl_WaitingList>> GetActiveWaitingListAsync(CancellationToken ct = default)
    {
        return await QueryNoTracking
            .Where(w => w.Status == WaitingStatus.Waiting)
            .OrderBy(w => w.DesiredDate)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Tbl_WaitingList>> GetPagedWaitingListAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        return await GetPagedAsync(pagination, ct);
    }

    public async Task<Tbl_WaitingList> AddToWaitingListAsync(
        WaitingListCreateDto dto,
        CancellationToken ct = default)
    {
        var timeSpan = TimeSpan.TryParse(dto.PreferredTimeShift, out var ts)
            ? ts
            : new TimeSpan(10, 0, 0);

        var entity = new Tbl_WaitingList
        {
            Tc = Guid.NewGuid(),
            CustomerName = dto.CustomerName,
            MobileNumber = dto.MobileNumber,
            ServiceName = dto.ServiceName ?? "خدمت عمومی",
            PersonalName = dto.PersonalName,
            DesiredDate = dto.PreferredDate,
            DesiredTime = timeSpan,
            Status = WaitingStatus.Waiting,
            Note = dto.PreferredTimeShift,
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        await InsertAsync(entity, ct);
        return entity;
    }

    public async Task<bool> MarkNotifiedAsync(Guid tc, CancellationToken ct = default)
    {
        var item = await Set.FirstOrDefaultAsync(w => w.Tc == tc && !w.IsDelete, ct);
        if (item is null)
        {
            return false;
        }

        item.Status = WaitingStatus.Notified;
        return await Db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteFromWaitingListAsync(Guid tc, CancellationToken ct = default)
    {
        return await SoftDeleteAsync(tc, ct);
    }
}
