using System;
using System.Collections.Generic;
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
/// کنترلر RESTful نظرات و امتیازدهی مشتریان (بند ۲ سند)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommentController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// دریافت نظرات تأیید شده برای نمایش در صفحه اصلی
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<Tbl_Comment>>> GetApproved(
        [FromQuery] Guid? personalTc,
        [FromQuery] Guid? serviceTc,
        CancellationToken ct = default)
    {
        Response.Headers.Append("Link", "</api/comment>; rel=\"self\", </api/comment/paged>; rel=\"paged\"");
        var list = await _commentService.GetApprovedCommentsAsync(personalTc, serviceTc, ct);
        return Ok(list);
    }

    /// <summary>
    /// فهرست کامل نظرات با صفحه‌بندی سمت سرور (مخصوص ادمین)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResult<Tbl_Comment>>>> GetPaged(
        [FromQuery] PaginationParams pagination,
        [FromQuery] bool? approvedOnly,
        CancellationToken ct = default)
    {
        var result = await _commentService.GetPagedCommentsAsync(pagination, approvedOnly, ct);
        return PagedResponse(result, "/api/comment/paged");
    }

    /// <summary>
    /// ثبت نظر جدید
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<Tbl_Comment>> Create(
        [FromBody] CommentCreateDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var comment = await _commentService.AddCommentAsync(dto, ct);
        return Ok(comment);
    }

    /// <summary>
    /// تأیید نظر توسط ادمین
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{tc:guid}/approve")]
    public async Task<ActionResult> Approve(Guid tc, CancellationToken ct = default)
    {
        var ok = await _commentService.ApproveCommentAsync(tc, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "نظر یافت نشد" });
        }

        return Ok(new { Success = true });
    }

    /// <summary>
    /// حذف نظر
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{tc:guid}")]
    public async Task<ActionResult> Delete(Guid tc, CancellationToken ct = default)
    {
        var ok = await _commentService.DeleteCommentAsync(tc, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "نظر یافت نشد" });
        }

        return Ok(new { Success = true });
    }
}
