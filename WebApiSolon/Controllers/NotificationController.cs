using System;
using System.Collections.Generic;
using System.Security.Claims;
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
/// کنترلر RESTful کارتابل اعلانات و پیام‌های سیستمی (بند ۱۷ سند)
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string CurrentRole =>
        User.IsInRole("Admin") ? "Admin" : User.IsInRole("Personnel") ? "Personnel" : "Customer";

    private Guid? CurrentUserTc
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var g) ? g : null;
        }
    }

    /// <summary>
    /// دریافت اعلانات مرتبط با نقش کاربر جاری
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Tbl_Notification>>> GetMyNotifications(CancellationToken ct = default)
    {
        Response.Headers.Append("Link", "</api/notification>; rel=\"self\", </api/notification/paged>; rel=\"paged\"");
        var list = await _notificationService.GetNotificationsByRoleAsync(CurrentRole, CurrentUserTc, ct);
        return Ok(list);
    }

    /// <summary>
    /// دریافت اعلانات با صفحه‌بندی سمت سرور
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResult<Tbl_Notification>>>> GetPaged(
        [FromQuery] PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await _notificationService.GetPagedNotificationsAsync(CurrentRole, pagination, ct);
        return PagedResponse(result, "/api/notification/paged");
    }

    /// <summary>
    /// علامت‌گذاری یک اعلان به عنوان خوانده شده
    /// </summary>
    [HttpPatch("{tc:guid}/read")]
    public async Task<ActionResult> MarkAsRead(Guid tc, CancellationToken ct = default)
    {
        var ok = await _notificationService.MarkAsReadAsync(tc, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "اعلان یافت نشد" });
        }

        return Ok(new { Success = true });
    }

    /// <summary>
    /// علامت‌گذاری تمام اعلانات به عنوان خوانده شده
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<ActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var ok = await _notificationService.MarkAllAsReadAsync(CurrentRole, ct);
        return Ok(new { Success = ok });
    }

    /// <summary>
    /// ایجاد اعلان جدید (مخصوص ادمین)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Tbl_Notification>> Create(
        [FromBody] NotificationCreateDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = await _notificationService.CreateNotificationAsync(dto, ct);
        return Ok(entity);
    }
}
