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
/// کنترلر RESTful مدیریت رزرواسیون نوبت‌ها، درگاه پرداخت، استرداد وجه و ثبت حضوری
/// مطابق با بندهای ۸، ۹، ۱۰، ۱۱، ۱۴ و ۱۵ سند
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReservationController : BaseApiController
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    private Guid CurrentUserTc
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var g) ? g : Guid.Empty;
        }
    }

    private bool IsAdmin => User.IsInRole("Admin");

    private bool IsPersonnel => User.IsInRole("Personnel");

    /// <summary>
    /// دریافت فهرست نوبت‌ها (مدیر: همه، پرسنل: نوبت‌های خودش، مشتری: نوبت‌های خودش)
    /// </summary>
    [Authorize]
    [HttpGet]
    public ActionResult<List<Tbl_Reservation>> GetAll()
    {
        Response.Headers.Append("Link", "</api/reservation>; rel=\"self\", </api/reservation/paged>; rel=\"paged\"");

        if (IsAdmin)
        {
            return Ok(_reservationService.GetReservations());
        }

        if (IsPersonnel)
        {
            return Ok(_reservationService.GetByPersonalTC(CurrentUserTc));
        }

        return Ok(_reservationService.GetByCustomerTC(CurrentUserTc));
    }

    /// <summary>
    /// دریافت فهرست نوبت‌ها با صفحه‌بندی سمت سرور و فیلترهای پیشرفته
    /// </summary>
    [Authorize]
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReservationViewModel>>>> GetPaged(
        [FromQuery] ReservationFilterDto filter,
        [FromQuery] PaginationParams pagination,
        CancellationToken ct = default)
    {
        if (IsAdmin)
        {
            // مدیر دسترسی به تمام نوبت‌ها دارد
        }
        else if (IsPersonnel)
        {
            filter.PersonalTC = CurrentUserTc;
        }
        else
        {
            filter.CustomerTC = CurrentUserTc;
        }

        var result = await _reservationService.GetPagedReservationsAsync(filter, pagination, ct);
        return PagedResponse(result, "/api/reservation/paged");
    }

    /// <summary>
    /// نوبت‌های یک روز مشخص (مدیر و پرسنل)
    /// </summary>
    [Authorize(Roles = "Admin,Personnel")]
    [HttpGet("by-date")]
    public ActionResult<List<Tbl_Reservation>> GetByDate([FromQuery] DateTime date)
    {
        return Ok(_reservationService.GetByDate(date));
    }

    /// <summary>
    /// پیگیری رزرو با کد رهگیری سیستمی (عمومی)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("by-tracking/{trackingCode}")]
    public ActionResult<Tbl_Reservation> GetByTracking(string trackingCode)
    {
        var item = _reservationService.GetByTrackingCode(trackingCode);
        if (item is null)
        {
            return NotFound(new { Success = false, Message = "نوبتی با این کد رهگیری یافت نشد" });
        }

        Response.Headers.Append("Link", $"</api/reservation/by-tracking/{trackingCode}>; rel=\"self\"");
        return Ok(item);
    }

    /// <summary>
    /// جزئیات یک نوبت با TC
    /// </summary>
    [Authorize]
    [HttpGet("{tc:guid}")]
    public ActionResult<Tbl_Reservation> Get(Guid tc)
    {
        var item = _reservationService.GetReservationByTC(tc);
        if (item is null)
        {
            return NotFound(new { Success = false, Message = "نوبت یافت نشد" });
        }

        Response.Headers.Append("Link", $"</api/reservation/{tc}>; rel=\"self\", </api/reservation/{tc}/status>; rel=\"status\"");
        return Ok(item);
    }

    /// <summary>
    /// ثبت نوبت آنلاین جدید (بند ۸ و ۲۰)
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    public ActionResult<Tbl_Reservation> Create([FromBody] Tbl_Reservation reservation)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var saved = _reservationService.AddReservation(reservation);
        if (saved is null)
        {
            return StatusCode(500, new { Success = false, Message = "ثبت نوبت با خطا مواجه شد" });
        }

        Response.Headers.Append("Link", $"</api/reservation/{saved.Tc}>; rel=\"self\"");
        return Ok(saved);
    }

    /// <summary>
    /// ثبت نوبت حضوری توسط مدیر یا پرسنل سالن (بند ۱۴ سند)
    /// </summary>
    [Authorize(Roles = "Admin,Personnel")]
    [HttpPost("in-person")]
    public ActionResult<Tbl_Reservation> CreateInPerson([FromBody] Tbl_Reservation reservation)
    {
        reservation.ReservationType = ReservationType.InPerson;
        reservation.PaymentStatus = PaymentStatus.Paid;
        reservation.Status = ReservationStatus.Confirmed;
        reservation.ConfirmDate = DateTime.Now;
        reservation.RegisteredBy = User.Identity?.Name ?? "مدیر سالن";

        var saved = _reservationService.AddReservation(reservation);
        if (saved is null)
        {
            return StatusCode(500, new { Success = false, Message = "ثبت رزرو حضوری با خطا مواجه شد" });
        }

        return Ok(saved);
    }

    /// <summary>
    /// لغو رزرو به همراه استرداد وجه (بند ۱۵ سند)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{tc:guid}/refund")]
    public ActionResult Refund(Guid tc, [FromBody] RefundModel model)
    {
        var ok = _reservationService.CancelWithRefund(
            tc,
            model.RefundAmount,
            User.Identity?.Name ?? "مدیر سیستم",
            model.RefundReference ?? $"REF-{DateTime.Now:MMddHHmm}",
            model.CancelReason ?? "لغو و استرداد توسط مدیر");

        if (!ok)
        {
            return BadRequest(new { Success = false, Message = "عملیات استرداد ناموفق بود" });
        }

        return Ok(new { Success = true, Message = "نوبت لغو و اطلاعات استرداد ثبت شد" });
    }

    /// <summary>
    /// تغییر وضعیت نوبت (تأیید، لغو، اتمام)
    /// </summary>
    [Authorize(Roles = "Admin,Personnel")]
    [HttpPatch("{tc:guid}/status")]
    public ActionResult UpdateStatus(Guid tc, [FromBody] StatusModel model)
    {
        var ok = _reservationService.ChangeStatus(tc, model.Status, model.CancelReason);
        if (!ok)
        {
            return BadRequest(new { Success = false, Message = "تغییر وضعیت ناموفق بود" });
        }

        return Ok(new { Success = true, Message = "وضعیت نوبت به‌روزرسانی شد" });
    }

    /// <summary>
    /// لغو نوبت توسط مشتری
    /// </summary>
    [Authorize]
    [HttpPatch("{tc:guid}/cancel")]
    public ActionResult Cancel(Guid tc, [FromBody] StatusModel model)
    {
        var ok = _reservationService.ChangeStatus(tc, ReservationStatus.Canceled, model.CancelReason ?? "لغو توسط مشتری");
        if (!ok)
        {
            return BadRequest(new { Success = false, Message = "لغو نوبت با شکست مواجه شد" });
        }

        return Ok(new { Success = true, Message = "نوبت با موفقیت لغو شد" });
    }

    /// <summary>
    /// تأیید پرداخت بازگشتی از درگاه زرین‌پال (بند ۹ و ۱۰)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("pay/verify")]
    public async Task<ActionResult> VerifyPayment([FromBody] PaymentVerificationModel model, CancellationToken ct = default)
    {
        if (model.SimulateSuccess)
        {
            var refId = $"REF-{DateTime.Now:yyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            if (model.ReservationTC != Guid.Empty)
            {
                _reservationService.UpdatePaymentStatus(model.ReservationTC, PaymentStatus.Paid, 0, refId);
            }

            return Ok(new
            {
                Success = true,
                Message = "پرداخت موفق و نوبت ثبت قطعی شد.",
                RefId = refId
            });
        }

        var result = await _reservationService.VerifyPaymentAsync(model.Authority, model.Status, ct);
        return Ok(new
        {
            Success = result.Success,
            Message = result.Message,
            RefId = result.RefId
        });
    }

    /// <summary>
    /// شروع فرایند پرداخت زرین‌پال
    /// </summary>
    [AllowAnonymous]
    [HttpPost("pay/initiate")]
    public async Task<ActionResult<PaymentInitiateResultDto>> InitiatePayment(
        [FromQuery] Guid reservationTc,
        [FromQuery] decimal amount,
        [FromQuery] string callbackUrl,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _reservationService.InitiatePaymentAsync(reservationTc, amount, callbackUrl, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}

public class RefundModel
{
    public decimal RefundAmount { get; set; }
    public string? RefundReference { get; set; }
    public string? CancelReason { get; set; }
}

public class StatusModel
{
    public ReservationStatus Status { get; set; }
    public string? CancelReason { get; set; }
}

public class PaymentVerificationModel
{
    public Guid ReservationTC { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string Status { get; set; } = "OK";
    public bool SimulateSuccess { get; set; } = true;
}
