using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataLayer.Entity;

namespace BlazorAppSolon.Services;

/// <summary>
/// سرویس کلاینت Blazor جهت دریافت و مدیریت اعلانات کارتابل
/// </summary>
public class NotificationApiService : BaseApiService
{
    public NotificationApiService(HttpClient http) : base(http)
    {
    }

    public async Task<ApiResponse<List<Tbl_Notification>>> GetMyNotificationsAsync(CancellationToken ct = default)
    {
        return await GetAsync<List<Tbl_Notification>>("api/notification", ct);
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid tc, CancellationToken ct = default)
    {
        return await PostAsync<object, bool>($"api/notification/{tc}/read", new { }, ct);
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(CancellationToken ct = default)
    {
        return await PostAsync<object, bool>("api/notification/mark-all-read", new { }, ct);
    }
}
