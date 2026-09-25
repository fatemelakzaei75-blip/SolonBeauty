using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace BlazorAppSolon.Services;

/// <summary>
/// سرویس کلاینت Blazor جهت مدیریت ارتباط با API نوبت‌ها، درگاه بانکی و استرداد
/// </summary>
public class ReservationApiService : BaseApiService
{
    public ReservationApiService(HttpClient http) : base(http)
    {
    }

    public async Task<ApiResponse<PagedResult<ReservationViewModel>>> GetPagedReservationsAsync(
        ReservationFilterDto filter,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var url = "api/reservation";
        if (filter.PersonalTC.HasValue && filter.PersonalTC.Value != Guid.Empty)
        {
            url += $"?personalTC={filter.PersonalTC.Value}";
        }

        return await GetPagedAsync<ReservationViewModel>(url, pagination, ct);
    }

    public async Task<ApiResponse<ReservationViewModel>> GetByTcAsync(Guid tc, CancellationToken ct = default)
    {
        return await GetAsync<ReservationViewModel>($"api/reservation/{tc}", ct);
    }

    public async Task<ApiResponse<ReservationViewModel>> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        return await GetAsync<ReservationViewModel>($"api/reservation/by-tracking/{trackingCode}", ct);
    }

    public async Task<ApiResponse<ReservationViewModel>> CreateOnlineReservationAsync(
        ReservationCreateDto dto,
        CancellationToken ct = default)
    {
        return await PostAsync<ReservationCreateDto, ReservationViewModel>("api/reservation", dto, ct);
    }

    public async Task<ApiResponse<ReservationViewModel>> CreateInPersonReservationAsync(
        InPersonReservationDto dto,
        CancellationToken ct = default)
    {
        return await PostAsync<InPersonReservationDto, ReservationViewModel>("api/reservation/in-person", dto, ct);
    }

    public async Task<ApiResponse<bool>> UpdateStatusAsync(
        Guid tc,
        ReservationStatus status,
        string? reason = null,
        CancellationToken ct = default)
    {
        var url = $"api/reservation/{tc}/status?status={status}";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            url += $"&reason={Uri.EscapeDataString(reason)}";
        }

        return await PostAsync<object, bool>(url, new { }, ct);
    }

    public async Task<ApiResponse<bool>> CancelReservationAsync(
        Guid tc,
        RefundReservationDto refundDto,
        CancellationToken ct = default)
    {
        return await PostAsync<RefundReservationDto, bool>($"api/reservation/{tc}/cancel", refundDto, ct);
    }

    public async Task<ApiResponse<PaymentInitiateResultDto>> InitiatePaymentAsync(
        Guid reservationTc,
        decimal amount,
        string callbackUrl,
        CancellationToken ct = default)
    {
        var url = $"api/reservation/pay/initiate?reservationTc={reservationTc}&amount={amount}&callbackUrl={Uri.EscapeDataString(callbackUrl)}";
        return await PostAsync<object, PaymentInitiateResultDto>(url, new { }, ct);
    }

    public async Task<ApiResponse<PaymentVerificationResultDto>> VerifyPaymentAsync(
        PaymentVerificationDto dto,
        CancellationToken ct = default)
    {
        return await PostAsync<PaymentVerificationDto, PaymentVerificationResultDto>("api/reservation/pay/verify", dto, ct);
    }
}
