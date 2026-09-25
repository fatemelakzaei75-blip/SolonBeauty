using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace DataCore.Interfaces;

/// <summary>
/// قرارداد مدیریت جامع رزرواسیون، تراکنش‌ها، استرداد وجه و ثبت حضوری
/// </summary>
public interface IReservationService
{
    List<Tbl_Reservation> GetReservations();

    Task<PagedResult<ReservationViewModel>> GetPagedReservationsAsync(
        ReservationFilterDto filter,
        PaginationParams pagination,
        CancellationToken ct = default);

    Tbl_Reservation? GetReservationByTC(Guid tc);

    Task<Tbl_Reservation?> GetByTcAsync(Guid tc, CancellationToken ct = default);

    Task<ReservationViewModel?> GetReservationViewModelByTcAsync(Guid tc, CancellationToken ct = default);

    Tbl_Reservation? GetByTrackingCode(string trackingCode);

    Task<ReservationViewModel?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);

    List<Tbl_Reservation> GetByCustomerTC(Guid customerTC);

    List<Tbl_Reservation> GetByPersonalTC(Guid personalTC);

    List<Tbl_Reservation> GetByDate(DateTime date);

    Tbl_Reservation? AddReservation(Tbl_Reservation reservation);

    Task<ReservationViewModel?> CreateReservationFromDtoAsync(ReservationCreateDto dto, CancellationToken ct = default);

    Task<ReservationViewModel?> CreateInPersonReservationAsync(InPersonReservationDto dto, string registeredBy, CancellationToken ct = default);

    bool EditReservation(Tbl_Reservation reservation);

    bool ChangeStatus(Guid tc, ReservationStatus status, string? cancelReason = null);

    bool CancelWithRefund(Guid tc, decimal refundAmount, string refundBy, string refundRef, string cancelReason);

    bool UpdatePaymentStatus(Guid tc, PaymentStatus status, decimal paidAmount, string? refId = null);

    bool DeleteReservation(Guid tc);

    Task<PaymentInitiateResultDto> InitiatePaymentAsync(Guid reservationTc, decimal amount, string callbackUrl, CancellationToken ct = default);

    Task<PaymentVerificationResultDto> VerifyPaymentAsync(string authority, string status, CancellationToken ct = default);
}
