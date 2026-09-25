using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace DataCore.Interfaces;

public interface INotificationService
{
    Task<List<Tbl_Notification>> GetNotificationsByRoleAsync(string role, Guid? userTc = null, CancellationToken ct = default);

    Task<PagedResult<Tbl_Notification>> GetPagedNotificationsAsync(string role, PaginationParams pagination, CancellationToken ct = default);

    Task<bool> MarkAsReadAsync(Guid tc, CancellationToken ct = default);

    Task<bool> MarkAllAsReadAsync(string role, CancellationToken ct = default);

    Task<Tbl_Notification> CreateNotificationAsync(NotificationCreateDto dto, CancellationToken ct = default);
}
