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
/// کنترلر RESTful پرسش‌ها و پاسخ‌های متداول سالن (بند ۲ سند)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FAQController : BaseApiController
{
    private readonly IFAQService _faqService;

    public FAQController(IFAQService faqService)
    {
        _faqService = faqService;
    }

    /// <summary>
    /// دریافت پرسش و پاسخ‌های متداول
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<Tbl_FAQ>>> GetList(
        [FromQuery] string? category,
        CancellationToken ct = default)
    {
        Response.Headers.Append("Link", "</api/faq>; rel=\"self\", </api/faq/paged>; rel=\"paged\"");
        var list = await _faqService.GetActiveFAQsAsync(category, ct);
        return Ok(list);
    }

    /// <summary>
    /// دریافت پرسش و پاسخ‌ها با صفحه‌بندی سمت سرور
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResult<Tbl_FAQ>>>> GetPaged(
        [FromQuery] PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await _faqService.GetPagedFAQsAsync(pagination, ct);
        return PagedResponse(result, "/api/faq/paged");
    }

    /// <summary>
    /// ثبت پرسش و پاسخ جدید
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Tbl_FAQ>> Create(
        [FromBody] FAQCreateDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = await _faqService.CreateFAQAsync(dto, ct);
        return Ok(entity);
    }

    /// <summary>
    /// ویرایش پرسش و پاسخ
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{tc:guid}")]
    public async Task<ActionResult> Update(
        Guid tc,
        [FromBody] FAQCreateDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ok = await _faqService.UpdateFAQAsync(tc, dto, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "مورد یافت نشد" });
        }

        return Ok(new { Success = true });
    }

    /// <summary>
    /// حذف پرسش و پاسخ
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{tc:guid}")]
    public async Task<ActionResult> Delete(Guid tc, CancellationToken ct = default)
    {
        var ok = await _faqService.DeleteFAQAsync(tc, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "مورد یافت نشد" });
        }

        return Ok(new { Success = true });
    }
}
