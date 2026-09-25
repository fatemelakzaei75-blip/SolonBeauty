using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using BlazorAppSolon.Auth;
using DataLayer.Entity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorAppSolon.Data;

/// <summary>
/// کلاینت مدیریت نوبت‌ها، درگاه پرداخت، کدهای تخفیف، رزرو حضوری و لیست انتظار (بندهای ۸، ۹، ۱۰، ۱۱، ۱۴ و ۱۵ سند)
/// </summary>
public class ReservationClient
{
    private readonly HttpClient _http;
    private readonly MockDb _mock;
    private readonly JwtAuthenticationStateProvider _auth;
    private readonly AuthenticationStateProvider _state;

    public bool IsOnline { get; private set; } = true;

    public ReservationClient(HttpClient http, MockDb mock, JwtAuthenticationStateProvider auth, AuthenticationStateProvider state)
    {
        _http = http; _mock = mock; _auth = auth; _state = state;
    }

    private async Task AttachTokenAsync()
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    private async Task<(Guid Tc, string Name, string Mobile, string Role)> CurrentUserAsync()
    {
        var s = await _state.GetAuthenticationStateAsync();
        var u = s.User;
        if (u.Identity?.IsAuthenticated != true) return (Guid.Empty, "", "", "");

        Guid.TryParse(u.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var tc);
        return (tc,
                u.Identity.Name ?? "",
                u.FindFirst(ClaimTypes.MobilePhone)?.Value ?? "",
                u.FindFirst(ClaimTypes.Role)?.Value ?? "");
    }

    /// <summary>نوبت‌های کاربر جاری (مشتری: نوبت‌های خودش، پرسنل/مدیر: مرتبط)</summary>
    public async Task<List<Tbl_Reservation>> MineAsync()
    {
        try
        {
            await AttachTokenAsync();
            var list = await _http.GetFromJsonAsync<List<Tbl_Reservation>>($"{AuthService.ApiBase}/api/reservation");
            if (list is not null) { IsOnline = true; return list; }
        }
        catch { IsOnline = false; }

        var (tc, _, _, role) = await CurrentUserAsync();
        var local = _mock.All<Tbl_Reservation>();
        return role switch
        {
            "Admin" => local,
            "Personnel" => local.Where(r => r.PersonalTC == tc || r.PersonalTC == Guid.Empty).ToList(),
            _ => local.Where(r => r.CustomerTC == tc || r.CustomerTC == Guid.Empty).ToList()
        };
    }

    /// <summary>ثبت نوبت جدید آنلاین با قیمت متغیر و کد رهگیری خودکار (بند ۸ و ۲۰)</summary>
    public async Task<(bool Ok, string Message, Tbl_Reservation? Item)> CreateAsync(Tbl_Reservation reservation)
    {
        var (tc, name, mobile, role) = await CurrentUserAsync();
        if (role == "Customer" && tc != Guid.Empty)
        {
            reservation.CustomerTC = tc;
            if (string.IsNullOrWhiteSpace(reservation.CustomerName)) reservation.CustomerName = name;
            if (string.IsNullOrWhiteSpace(reservation.MobileNumber)) reservation.MobileNumber = mobile;
        }

        reservation.MobileNumber = DigitHelper.Normalize(reservation.MobileNumber);
        if (string.IsNullOrWhiteSpace(reservation.TrackingCode))
        {
            reservation.TrackingCode = $"TC-{DateTime.Now:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        }

        if (reservation.PayableAmount <= 0)
        {
            reservation.PayableAmount = Math.Max(0, reservation.Price - reservation.DiscountAmount);
        }

        reservation.Status = ReservationStatus.PendingPayment;
        reservation.IsActive = true;
        reservation.RegisterData = DateTime.Now;
        if (reservation.Tc == Guid.Empty) reservation.Tc = Guid.NewGuid();

        try
        {
            await AttachTokenAsync();
            var res = await _http.PostAsJsonAsync($"{AuthService.ApiBase}/api/reservation", reservation);
            if (res.IsSuccessStatusCode)
            {
                IsOnline = true;
                var saved = await res.Content.ReadFromJsonAsync<Tbl_Reservation>();
                return (true, "درخواست نوبت شما ثبت شد.", saved ?? reservation);
            }
        }
        catch { IsOnline = false; }

        _mock.Add(reservation);
        return (true, "درخواست نوبت ثبت شد.", reservation);
    }

    /// <summary>ثبت نوبت حضوری توسط مدیر یا پرسنل (بند ۱۴ سند)</summary>
    public async Task<(bool Ok, string Message, Tbl_Reservation? Item)> CreateInPersonAsync(Tbl_Reservation reservation)
    {
        reservation.ReservationType = ReservationType.InPerson;
        reservation.PaymentStatus = PaymentStatus.Paid;
        reservation.Status = ReservationStatus.Confirmed;
        reservation.ConfirmDate = DateTime.Now;
        reservation.VariablePriceAgreed = true;
        if (string.IsNullOrWhiteSpace(reservation.TrackingCode))
            reservation.TrackingCode = $"TC-INP-{DateTime.Now:yyMMdd}-{Random.Shared.Next(100, 999)}";

        try
        {
            await AttachTokenAsync();
            var res = await _http.PostAsJsonAsync($"{AuthService.ApiBase}/api/reservation/in-person", reservation);
            if (res.IsSuccessStatusCode)
            {
                var saved = await res.Content.ReadFromJsonAsync<Tbl_Reservation>();
                return (true, "رزرو حضوری با موفقیت ثبت شد.", saved ?? reservation);
            }
        }
        catch { }

        _mock.Add(reservation);
        return (true, "رزرو حضوری ثبت شد (آفلاین).", reservation);
    }

    /// <summary>اعتبارسنجی کد تخفیف در سمت سرور (بند ۸ و ۹)</summary>
    public async Task<(bool Valid, decimal Discount, decimal Final, string Message)> ValidateDiscountAsync(string code, decimal amount)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<DiscountCheckDto>($"{AuthService.ApiBase}/api/discount/validate/{code}?amount={amount}");
            if (res is not null) return (res.Valid, res.DiscountAmount, res.FinalAmount, res.Message);
        }
        catch { }

        // بررسی در داده محلی
        var d = _mock.All<Tbl_Discount>().FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && x.IsActive);
        if (d is not null)
        {
            var discountVal = Math.Min(d.MaxDiscount > 0 ? d.MaxDiscount : decimal.MaxValue, amount * d.Percent / 100m);
            return (true, discountVal, Math.Max(0, amount - discountVal), $"کد تخفیف {d.Percent}٪ اعمال شد.");
        }

        if (code.Equals("HADIS20", StringComparison.OrdinalIgnoreCase))
        {
            var discountVal = Math.Min(300_000m, amount * 0.20m);
            return (true, discountVal, Math.Max(0, amount - discountVal), "کد تخفیف ۲۰٪ اعمال شد.");
        }

        return (false, 0, amount, "کد تخفیف وارد شده نامعتبر است.");
    }

    /// <summary>تأیید پرداخت بازگشتی از درگاه زرین‌پال (بند ۹ و ۱۰)</summary>
    public async Task<(bool Success, string Message, string? RefId)> VerifyPaymentAsync(Guid rsvTc, string authority, bool simulateSuccess)
    {
        try
        {
            var res = await _http.PostAsJsonAsync($"{AuthService.ApiBase}/api/reservation/pay/verify", new
            {
                ReservationTC = rsvTc,
                Authority = authority,
                Status = simulateSuccess ? "OK" : "NOK",
                SimulateSuccess = simulateSuccess
            });
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadFromJsonAsync<VerifyResponseDto>();
                return (data?.Success ?? false, data?.Message ?? "", data?.RefId);
            }
        }
        catch { }

        // شبیه‌سازی در MockDb
        var rsv = _mock.Find<Tbl_Reservation>(rsvTc);
        if (rsv is not null)
        {
            if (simulateSuccess)
            {
                var refId = $"REF-{DateTime.Now:yyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
                rsv.Status = ReservationStatus.PaymentSuccessful;
                rsv.PaymentStatus = PaymentStatus.Paid;
                rsv.PaidAmount = rsv.PayableAmount > 0 ? rsv.PayableAmount : rsv.Price;
                rsv.ConfirmDate = DateTime.Now;
                return (true, "پرداخت موفق و نوبت ثبت قطعی شد.", refId);
            }
            else
            {
                rsv.Status = ReservationStatus.Canceled;
                rsv.PaymentStatus = PaymentStatus.Failed;
                return (false, "پرداخت ناموفق بود.", null);
            }
        }

        return (false, "نوبت یافت نشد.", null);
    }

    /// <summary>لغو رزرو توسط ادمین به همراه ثبت استرداد وجه (بند ۱۵ سند)</summary>
    public async Task<bool> CancelWithRefundAsync(Guid tc, decimal refundAmount, string cancelReason, string? refundRef = null)
    {
        try
        {
            await AttachTokenAsync();
            var res = await _http.PostAsJsonAsync($"{AuthService.ApiBase}/api/reservation/{tc}/refund", new
            {
                RefundAmount = refundAmount,
                RefundReference = refundRef ?? $"REFUND-{DateTime.Now:MMddHHmm}",
                CancelReason = cancelReason
            });
            if (res.IsSuccessStatusCode) return true;
        }
        catch { }

        var item = _mock.Find<Tbl_Reservation>(tc);
        if (item is null) return false;
        item.Status = ReservationStatus.Canceled;
        item.CancelReason = cancelReason;
        item.RefundAmount = refundAmount;
        item.RefundStatus = refundAmount > 0 ? "استرداد شده" : "بدون استرداد";
        item.RefundDate = DateTime.Now;
        item.RefundReference = refundRef ?? $"REFUND-{DateTime.Now:MMddHHmm}";
        return true;
    }

    /// <summary>ثبت درخواست در لیست انتظار (بند ۱۱ سند)</summary>
    public async Task<bool> AddToWaitingListAsync(Tbl_WaitingList entry)
    {
        try
        {
            var res = await _http.PostAsJsonAsync($"{AuthService.ApiBase}/api/waitinglist", entry);
            if (res.IsSuccessStatusCode) return true;
        }
        catch { }

        _mock.Add(entry);
        return true;
    }

    /// <summary>تغییر وضعیت نوبت</summary>
    public async Task<bool> ChangeStatusAsync(Guid tc, ReservationStatus status, string? cancelReason = null)
    {
        try
        {
            await AttachTokenAsync();
            var res = await _http.PatchAsJsonAsync($"{AuthService.ApiBase}/api/reservation/{tc}/status",
                new { Status = status, CancelReason = cancelReason });
            if (res.IsSuccessStatusCode) { IsOnline = true; return true; }
        }
        catch { IsOnline = false; }

        var item = _mock.Find<Tbl_Reservation>(tc);
        if (item is null) return false;
        item.Status = status;
        if (status == ReservationStatus.Confirmed) item.ConfirmDate = DateTime.Now;
        if (status == ReservationStatus.Canceled) item.CancelReason = cancelReason;
        return true;
    }

    /// <summary>لغو نوبت توسط مشتری</summary>
    public async Task<bool> CancelAsync(Guid tc, string? reason = null)
    {
        try
        {
            await AttachTokenAsync();
            var res = await _http.PatchAsJsonAsync($"{AuthService.ApiBase}/api/reservation/{tc}/cancel",
                new { Status = ReservationStatus.Canceled, CancelReason = reason ?? "لغو توسط مشتری" });
            if (res.IsSuccessStatusCode) { IsOnline = true; return true; }
        }
        catch { IsOnline = false; }

        return await ChangeStatusAsync(tc, ReservationStatus.Canceled, reason);
    }

    public static string StatusLabel(ReservationStatus s) => s switch
    {
        ReservationStatus.PendingPayment => "در انتظار پرداخت",
        ReservationStatus.PaymentSuccessful => "پرداخت موفق",
        ReservationStatus.Confirmed => "تأیید قطعی نوبت",
        ReservationStatus.Canceled => "لغو شده",
        ReservationStatus.Expired => "منقضی شده",
        ReservationStatus.Done => "انجام شده",
        _ => "-"
    };

    public static string StatusCss(ReservationStatus s) => s switch
    {
        ReservationStatus.Confirmed or ReservationStatus.PaymentSuccessful or ReservationStatus.Done => "ok",
        ReservationStatus.Canceled or ReservationStatus.Expired => "off",
        _ => "wait"
    };

    private record DiscountCheckDto(bool Valid, decimal DiscountAmount, decimal FinalAmount, string Message);
    private record VerifyResponseDto(bool Success, string Message, string? RefId);
}
