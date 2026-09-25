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
/// پیاده‌سازی سرویس کارتابل و اعلانات بلادرنگ کاربران، پرسنل و مدیران سالن
/// </summary>
public class NotificationService : BaseRepository<Tbl_Notification>, INotificationService
{
    public NotificationService(DatabaseContext db) : base(db)
    {
    }

    public async Task<List<Tbl_Notification>> GetNotificationsByRoleAsync(
        string role,
        Guid? userTc = null,
        CancellationToken ct = default)
    {
        return await QueryNoTracking
            .Where(n => n.TargetRole == role || n.TargetRole == "All" || (userTc.HasValue && n.TargetUserTC == userTc.Value))
            .OrderByDescending(n => n.RegisterData)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Tbl_Notification>> GetPagedNotificationsAsync(
        string role,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = QueryNoTracking
            .Where(n => n.TargetRole == role || n.TargetRole == "All");

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.RegisterData)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Tbl_Notification>(items, total, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<bool> MarkAsReadAsync(Guid tc, CancellationToken ct = default)
    {
        var item = await Set.FirstOrDefaultAsync(x => x.Tc == tc && !x.IsDelete, ct);
        if (item is null)
        {
            return false;
        }

        item.IsRead = true;
        return await Db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(string role, CancellationToken ct = default)
    {
        var items = await Set
            .Where(n => !n.IsDelete && (n.TargetRole == role || n.TargetRole == "All") && !n.IsRead)
            .ToListAsync(ct);

        if (!items.Any())
        {
            return true;
        }

        foreach (var item in items)
        {
            item.IsRead = true;
        }

        return await Db.SaveChangesAsync(ct) > 0;
    }

    public async Task<Tbl_Notification> CreateNotificationAsync(
        NotificationCreateDto dto,
        CancellationToken ct = default)
    {
        var entity = new Tbl_Notification
        {
            Tc = Guid.NewGuid(),
            Title = dto.Title,
            Message = dto.Message,
            TargetRole = dto.TargetRole,
            TargetUserTC = dto.TargetUserTc,
            Link = dto.ActionUrl,
            IsRead = false,
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        await InsertAsync(entity, ct);
        return entity;
    }
}
