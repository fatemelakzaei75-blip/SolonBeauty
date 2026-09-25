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
/// کنترلر RESTful اعتبارسنجی و مدیریت کدهای تخفیف
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiscountController : BaseApiController
{
    private readonly IDiscountService _discountService;

    public DiscountController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>
    /// اعتبارسنجی آنلاین کد تخفیف در سمت سرور (بند ۸ و ۹)
    /// </summary>
    [HttpGet("validate/{code}")]
    public async Task<ActionResult<DiscountValidationResultDto>> Validate(
        string code,
        [FromQuery] decimal amount,
        CancellationToken ct = default)
    {
        Response.Headers.Append("Link", $"</api/discount/validate/{code}>; rel=\"self\"");

        var result = await _discountService.ValidateDiscountAsync(code, amount, ct);
        if (!result.Valid)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// فهرست کدهای تخفیف (مخصوص ادمین)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<Tbl_Discount>>> GetList(CancellationToken ct = default)
    {
        Response.Headers.Append("Link", "</api/discount>; rel=\"self\", </api/discount/paged>; rel=\"paged\"");
        var list = await _discountService.GetAllDiscountsAsync(ct);
        return Ok(list);
    }

    /// <summary>
    /// فهرست کدهای تخفیف با صفحه‌بندی سمت سرور (مخصوص ادمین)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResult<Tbl_Discount>>>> GetPaged(
        [FromQuery] PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await _discountService.GetPagedDiscountsAsync(pagination, ct);
        return PagedResponse(result, "/api/discount/paged");
    }

    /// <summary>
    /// ثبت کد تخفیف جدید
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Tbl_Discount>> Create(
        [FromBody] Tbl_Discount disc,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var dto = new DiscountCreateDto
        {
            Code = disc.Code,
            Title = disc.Title,
            Percent = disc.Percent,
            MaxDiscount = disc.MaxDiscount,
            MinPurchase = disc.MinPurchase,
            ExpireDate = disc.ExpireDate
        };

        var created = await _discountService.CreateDiscountAsync(dto, ct);
        return Ok(created ?? disc);
    }

    /// <summary>
    /// حذف منطقی کد تخفیف
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{tc:guid}")]
    public async Task<ActionResult> Delete(Guid tc, CancellationToken ct = default)
    {
        var ok = await _discountService.DeleteDiscountAsync(tc, ct);
        if (!ok)
        {
            return NotFound(new { Success = false, Message = "کد تخفیف یافت نشد" });
        }

        return Ok(new { Success = true, Message = "کد تخفیف حذف شد" });
    }
}
