using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.Common;
using DataLayer.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>
/// کنترلر RESTful مدیریت لیست انتظار نوبت‌های پر شده (بند ۱۱ سند)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WaitingListController : BaseApiController
{
    private readonly IWaitingListService _waitingListService;

    public WaitingListController(IWaitingListService waitingListService)
    {
        _waitingListService = waitingListService;
    }

    /// <summary>
    /// فهرست متقاضیان فعال لیست انتظار (مخصوص ادمین و پرسنل)
    /// </summary>
    [Authorize(Roles = "Admin,Personnel")]
    [HttpGet]
    public async Task<ActionResult<List<Tbl_WaitingList>>> GetAll(CancellationToken ct = default)
    {
        Response.Headers.Append("Link", "</api/waitinglist>; rel=\"self\", </api/waitinglist/paged>; rel=\"paged\"");
        var list = await _waitingListService.GetActiveWaitingListAsync(ct);
        return Ok(list);
    }

    /// <summary>
    /// فهرست متقاضیان با صفحه‌بندی سمت سرور
    /// </summary>
    [Authorize(Roles = "Admin,Personnel")]
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResult<Tbl_WaitingList>>>> GetPaged(
        [FromQuery] PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await _waitingListService.GetPagedWaitingListAsync(pagination, ct);
        return PagedResponse(result, "/api/waitinglist/paged");
    }

    /// <summary>
    /// ثبت‌نام در لیست انتظار نوبت پر شده
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<Tbl_WaitingList>> Create(
        [FromBody] Tbl_WaitingList entry,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        entry.Tc = entry.Tc == Guid.Empty ? Guid.NewGuid() : entry.Tc;
        entry.IsActive = true;
        entry.RegisterData = DateTime.Now;
        entry.Status = WaitingStatus.Waiting;

        await _waitingListService.AddToWaitingListAsync(new DataCore.Models.ViewModels.WaitingListCreateDto
        {
            CustomerName = entry.CustomerName,
            MobileNumber = entry.MobileNumber,
            ServiceName = entry.ServiceName,
            PersonalName = entry.PersonalName,
            PreferredDate = entry.DesiredDate,
            PreferredTimeShift = entry.DesiredTime.ToString(@"hh\:mm")
        }, ct);

        Response.Headers.Append("Link", $"</api/waitinglist/{entry.Tc}>; rel=\"self\"");
        return Ok(entry);
    }

    /// <summary>
    /// علامت‌گذاری به عنوان اطلاع‌رسانی شده
    /// </summary>
    [Authorize(Roles = "Admin,Personnel")]
    [HttpPatch("{tc:guid}/notified")]
    public async Task<ActionResult> MarkNotified(Guid tc, CancellationToken ct = default)
    {
        var ok = await _waitingListService.MarkNotifiedAsync(tc, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "مورد یافت نشد" });
        }

        return Ok(new { Success = true, Message = "وضعیت اطلاع‌رسانی ثبت شد" });
    }

    /// <summary>
    /// حذف متقاضی از لیست انتظار
    /// </summary>
    [Authorize(Roles = "Admin,Personnel")]
    [HttpDelete("{tc:guid}")]
    public async Task<ActionResult> Delete(Guid tc, CancellationToken ct = default)
    {
        var ok = await _waitingListService.DeleteFromWaitingListAsync(tc, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "مورد یافت نشد" });
        }

        return Ok(new { Success = true, Message = "مورد از لیست انتظار حذف شد" });
    }
}
