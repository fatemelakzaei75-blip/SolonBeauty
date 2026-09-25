using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Helpers;
using DataCore.Interfaces;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس جامع مدیریت رزرواسیون، تراکنش‌ها، اعتبارسنجی‌ها و پرداخت نوبت‌ها
/// </summary>
public class ReservationService : BaseRepository<Tbl_Reservation>, IReservationService
{
    public ReservationService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Reservation> GetReservations()
    {
        return QueryNoTracking
            .OrderByDescending(r => r.ReserveDate)
            .ThenBy(r => r.ReserveTime)
            .ToList();
    }

    public async Task<PagedResult<ReservationViewModel>> GetPagedReservationsAsync(
        ReservationFilterDto filter,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = QueryNoTracking;

        if (filter.CustomerTC.HasValue && filter.CustomerTC.Value != Guid.Empty)
        {
            query = query.Where(r => r.CustomerTC == filter.CustomerTC.Value);
        }

        if (filter.PersonalTC.HasValue && filter.PersonalTC.Value != Guid.Empty)
        {
            query = query.Where(r => r.PersonalTC == filter.PersonalTC.Value);
        }

        if (filter.Date.HasValue)
        {
            var targetDate = filter.Date.Value.Date;
            query = query.Where(r => r.ReserveDate.Date == targetDate);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(r => r.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(r => r.CustomerName.Contains(search) ||
                                     r.MobileNumber.Contains(search) ||
                                     r.TrackingCode.Contains(search) ||
                                     r.ServiceName.Contains(search));
        }

        var totalCount = await query.CountAsync(ct);

        var entities = await query
            .OrderByDescending(r => r.ReserveDate)
            .ThenByDescending(r => r.ReserveTime)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        var viewModels = entities.Select(MapToViewModel).ToList();

        return new PagedResult<ReservationViewModel>(
            viewModels,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }

    public Tbl_Reservation? GetReservationByTC(Guid tc)
    {
        return GetByTc(tc);
    }

    public async Task<ReservationViewModel?> GetReservationViewModelByTcAsync(Guid tc, CancellationToken ct = default)
    {
        var entity = await GetByTcAsync(tc, ct);
        return entity is null ? null : MapToViewModel(entity);
    }

    public Tbl_Reservation? GetByTrackingCode(string trackingCode)
    {
        return QueryNoTracking.FirstOrDefault(r => r.TrackingCode == trackingCode);
    }

    public async Task<ReservationViewModel?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        var entity = await QueryNoTracking
            .FirstOrDefaultAsync(r => r.TrackingCode == trackingCode, ct);

        return entity is null ? null : MapToViewModel(entity);
    }

    public List<Tbl_Reservation> GetByCustomerTC(Guid customerTC)
    {
        return QueryNoTracking
            .Where(r => r.CustomerTC == customerTC)
            .OrderByDescending(r => r.ReserveDate)
            .ThenBy(r => r.ReserveTime)
            .ToList();
    }

    public List<Tbl_Reservation> GetByPersonalTC(Guid personalTC)
    {
        return QueryNoTracking
            .Where(r => r.PersonalTC == personalTC)
            .OrderBy(r => r.ReserveDate)
            .ThenBy(r => r.ReserveTime)
            .ToList();
    }

    public List<Tbl_Reservation> GetByDate(DateTime date)
    {
        var targetDate = date.Date;
        return QueryNoTracking
            .Where(r => r.ReserveDate.Date == targetDate)
            .OrderBy(r => r.ReserveTime)
            .ToList();
    }

    public Tbl_Reservation? AddReservation(Tbl_Reservation reservation)
    {
        try
        {
            reservation.MobileNumber = NormalizeDigits(reservation.MobileNumber);

            if (reservation.CustomerTC == Guid.Empty)
            {
                reservation.CustomerTC = EnsureCustomer(reservation.CustomerName, reservation.MobileNumber);
            }

            if (reservation.Tc == Guid.Empty)
            {
                reservation.Tc = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(reservation.TrackingCode))
            {
                var count = Set.Count() + 1;
                reservation.TrackingCode = $"TC-{DateTime.Now:yyMMdd}-{count:D4}";
            }

            if (reservation.PayableAmount <= 0)
            {
                reservation.PayableAmount = Math.Max(0, reservation.Price - reservation.DiscountAmount);
            }

            if (reservation.ReservationType == ReservationType.InPerson || reservation.ReservationType == ReservationType.Admin)
            {
                reservation.Status = ReservationStatus.Confirmed;
                reservation.PaymentStatus = PaymentStatus.Paid;
                reservation.PaidAmount = reservation.PayableAmount;
            }
            else
            {
                reservation.Status = ReservationStatus.PendingPayment;
                if (reservation.PaymentStatus == PaymentStatus.Paid)
                {
                    reservation.Status = ReservationStatus.PaymentSuccessful;
                }
            }

            reservation.IsActive = true;
            reservation.RegisterData = DateTime.Now;

            var ok = Insert(reservation);
            if (!ok) return null;

            CreateReservationNotifications(reservation);
            return reservation;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ReservationViewModel?> CreateReservationFromDtoAsync(ReservationCreateDto dto, CancellationToken ct = default)
    {
        var normalizedMobile = NormalizeDigits(dto.MobileNumber);
        var customerTc = EnsureCustomer(dto.CustomerName, normalizedMobile);

        var service = await Db.Tbl_SalonSerice
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Tc == dto.ServiceTC && !s.IsDelete, ct);

        var personal = await Db.Tbl_Personal
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Tc == dto.PersonalTC && !p.IsDelete, ct);

        var serviceName = service?.ServiceName ?? "خدمت سالن";
        var staffName = personal?.FullName ?? "پرسنل سالن";
        var price = service?.Price ?? 500_000m;

        decimal discountAmount = 0;
        if (!string.IsNullOrWhiteSpace(dto.DiscountCode))
        {
            var disc = await Db.Tbl_Discount
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == dto.DiscountCode.Trim().ToUpper() && d.IsActive && !d.IsDelete, ct);

            if (disc is not null)
            {
                discountAmount = price * disc.Percent / 100m;
                if (disc.MaxDiscount > 0 && discountAmount > disc.MaxDiscount)
                {
                    discountAmount = disc.MaxDiscount;
                }
            }
        }

        var payableAmount = Math.Max(0, price - discountAmount);
        var count = await Set.CountAsync(ct) + 1;
        var trackingCode = $"TC-{DateTime.Now:yyMMdd}-{count:D4}";

        var timeSpan = TimeSpan.TryParse(dto.ReserveTime, out var ts)
            ? ts
            : new TimeSpan(10, 0, 0);

        var reservation = new Tbl_Reservation
        {
            Tc = Guid.NewGuid(),
            CustomerTC = customerTc,
            CustomerName = dto.CustomerName,
            MobileNumber = normalizedMobile,
            PersonalTC = dto.PersonalTC,
            PersonalName = staffName,
            ServiceTC = dto.ServiceTC,
            ServiceName = serviceName,
            ReserveDate = dto.ReserveDate,
            ReserveTime = timeSpan,
            TrackingCode = trackingCode,
            Price = price,
            DiscountCode = dto.DiscountCode,
            DiscountAmount = discountAmount,
            PayableAmount = payableAmount,
            PaidAmount = dto.DepositAmount,
            PaymentStatus = dto.DepositAmount > 0 ? PaymentStatus.Paid : PaymentStatus.Pending,
            Status = dto.DepositAmount > 0 ? ReservationStatus.Confirmed : ReservationStatus.PendingPayment,
            ReservationType = ReservationType.Online,
            VariablePriceAgreed = dto.VariablePriceAgreed,
            CustomerNote = dto.CustomerNote,
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        var inserted = await InsertAsync(reservation, ct);
        if (!inserted) return null;

        CreateReservationNotifications(reservation);
        return MapToViewModel(reservation);
    }

    public async Task<ReservationViewModel?> CreateInPersonReservationAsync(InPersonReservationDto dto, string registeredBy, CancellationToken ct = default)
    {
        var normalizedMobile = NormalizeDigits(dto.MobileNumber);
        var customerTc = EnsureCustomer(dto.CustomerName, normalizedMobile);

        var service = await Db.Tbl_SalonSerice
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Tc == dto.ServiceTC && !s.IsDelete, ct);

        var personal = await Db.Tbl_Personal
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Tc == dto.PersonalTC && !p.IsDelete, ct);

        var serviceName = service?.ServiceName ?? "خدمت حضوری";
        var staffName = personal?.FullName ?? "پرسنل سالن";
        var price = service?.Price ?? dto.PaidAmount;

        var count = await Set.CountAsync(ct) + 1;
        var trackingCode = $"TC-{DateTime.Now:yyMMdd}-{count:D4}";

        var timeSpan = TimeSpan.TryParse(dto.ReserveTime, out var ts)
            ? ts
            : new TimeSpan(10, 0, 0);

        var reservation = new Tbl_Reservation
        {
            Tc = Guid.NewGuid(),
            CustomerTC = customerTc,
            CustomerName = dto.CustomerName,
            MobileNumber = normalizedMobile,
            PersonalTC = dto.PersonalTC,
            PersonalName = staffName,
            ServiceTC = dto.ServiceTC,
            ServiceName = serviceName,
            ReserveDate = dto.ReserveDate,
            ReserveTime = timeSpan,
            TrackingCode = trackingCode,
            Price = price,
            PayableAmount = price,
            PaidAmount = dto.PaidAmount,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = PaymentStatus.Paid,
            Status = ReservationStatus.Confirmed,
            ReservationType = ReservationType.InPerson,
            RegisteredBy = registeredBy,
            ConfirmDate = DateTime.Now,
            CustomerNote = dto.CustomerNote,
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        var inserted = await InsertAsync(reservation, ct);
        if (!inserted) return null;

        CreateReservationNotifications(reservation);
        return MapToViewModel(reservation);
    }

    public bool EditReservation(Tbl_Reservation reservation)
    {
        return Modify(reservation);
    }

    public bool ChangeStatus(Guid tc, ReservationStatus status, string? cancelReason = null)
    {
        try
        {
            var current = Set.FirstOrDefault(r => r.Tc == tc && !r.IsDelete);
            if (current is null) return false;

            current.Status = status;
            if (status == ReservationStatus.Confirmed) current.ConfirmDate = DateTime.Now;
            if (status == ReservationStatus.Canceled) current.CancelReason = cancelReason;

            return Db.SaveChanges() > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool CancelWithRefund(Guid tc, decimal refundAmount, string refundBy, string refundRef, string cancelReason)
    {
        try
        {
            var current = Set.FirstOrDefault(r => r.Tc == tc && !r.IsDelete);
            if (current is null) return false;

            current.Status = ReservationStatus.Canceled;
            current.CancelReason = cancelReason;
            current.RefundAmount = refundAmount;
            current.RefundStatus = refundAmount > 0 ? "استرداد شده" : "بدون استرداد";
            current.RefundDate = DateTime.Now;
            current.RefundBy = refundBy;
            current.RefundReference = refundRef;

            return Db.SaveChanges() > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool UpdatePaymentStatus(Guid tc, PaymentStatus status, decimal paidAmount, string? refId = null)
    {
        try
        {
            var current = Set.FirstOrDefault(r => r.Tc == tc && !r.IsDelete);
            if (current is null) return false;

            current.PaymentStatus = status;
            if (status == PaymentStatus.Paid)
            {
                current.PaidAmount = paidAmount;
                current.Status = ReservationStatus.PaymentSuccessful;
                current.ConfirmDate = DateTime.Now;
            }
            else if (status == PaymentStatus.Failed || status == PaymentStatus.Canceled)
            {
                current.Status = ReservationStatus.Canceled;
            }

            return Db.SaveChanges() > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool DeleteReservation(Guid tc)
    {
        return SoftDelete(tc);
    }

    public async Task<PaymentInitiateResultDto> InitiatePaymentAsync(Guid reservationTc, decimal amount, string callbackUrl, CancellationToken ct = default)
    {
        var reservation = await GetByTcAsync(reservationTc, ct);
        if (reservation is null)
        {
            throw new InvalidOperationException("رزرو مورد نظر یافت نشد");
        }

        var authority = $"A0000000000000000000000000{DateTime.Now:MMddHHmmss}";
        var transaction = new Tbl_PaymentTransaction
        {
            Tc = Guid.NewGuid(),
            ReservationTC = reservationTc,
            TrackingCode = reservation.TrackingCode,
            Amount = amount > 0 ? amount : reservation.PayableAmount,
            GatewayName = "ZarinPal",
            Authority = authority,
            Status = PaymentStatus.Pending,
            IdempotencyKey = Guid.NewGuid().ToString(),
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        await Db.Tbl_PaymentTransaction.AddAsync(transaction, ct);
        reservation.PaymentStatus = PaymentStatus.RedirectedToGateway;
        await Db.SaveChangesAsync(ct);

        return new PaymentInitiateResultDto
        {
            Authority = authority,
            PaymentUrl = $"https://www.zarinpal.com/pg/StartPay/{authority}",
            Amount = transaction.Amount,
            TrackingCode = reservation.TrackingCode
        };
    }

    public async Task<PaymentVerificationResultDto> VerifyPaymentAsync(string authority, string status, CancellationToken ct = default)
    {
        var tx = await Db.Tbl_PaymentTransaction
            .FirstOrDefaultAsync(t => t.Authority == authority && !t.IsDelete, ct);

        if (tx is null)
        {
            return new PaymentVerificationResultDto
            {
                Success = false,
                Message = "تراکنشی با این شناسه درگاه یافت نشد."
            };
        }

        if (status?.ToUpper() != "OK")
        {
            tx.Status = PaymentStatus.Failed;
            await Db.SaveChangesAsync(ct);
            return new PaymentVerificationResultDto
            {
                Success = false,
                Message = "پرداخت توسط کاربر یا بانک لغو گردید.",
                TrackingCode = tx.TrackingCode,
                Amount = tx.Amount
            };
        }

        var refId = $"{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
        tx.Status = PaymentStatus.Paid;
        tx.RefId = refId;
        tx.VerifiedAt = DateTime.Now;

        var reservation = await Db.Tbl_Reservation.FirstOrDefaultAsync(r => r.Tc == tx.ReservationTC && !r.IsDelete, ct);
        if (reservation is not null)
        {
            reservation.PaymentStatus = PaymentStatus.Paid;
            reservation.PaidAmount = tx.Amount;
            reservation.Status = ReservationStatus.Confirmed;
            reservation.ConfirmDate = DateTime.Now;
        }

        await Db.SaveChangesAsync(ct);

        return new PaymentVerificationResultDto
        {
            Success = true,
            Message = "پرداخت با موفقیت انجام و نوبت شما قطعی شد.",
            RefId = refId,
            TrackingCode = tx.TrackingCode,
            Amount = tx.Amount
        };
    }

    private void CreateReservationNotifications(Tbl_Reservation reservation)
    {
        try
        {
            Db.Tbl_Notification.Add(new Tbl_Notification
            {
                Tc = Guid.NewGuid(),
                TargetRole = "Admin",
                Title = "رزرو جدید ثبت شد",
                Message = $"رزرو جدید برای خدمت «{reservation.ServiceName}» توسط {reservation.CustomerName} ({reservation.TrackingCode})",
                CreatedAt = DateTime.Now,
                IsActive = true,
                RegisterData = DateTime.Now,
                Link = $"/panel/admin/entity/reservation?tc={reservation.Tc}"
            });

            if (reservation.PersonalTC != Guid.Empty || !string.IsNullOrWhiteSpace(reservation.PersonalName))
            {
                Db.Tbl_Notification.Add(new Tbl_Notification
                {
                    Tc = Guid.NewGuid(),
                    TargetRole = "Personnel",
                    TargetUserTC = reservation.PersonalTC != Guid.Empty ? reservation.PersonalTC : null,
                    Title = "نوبت جدید برای شما",
                    Message = $"برای شما یک نوبت جدید برای خدمت «{reservation.ServiceName}» در تاریخ {PersianDateHelper.ToShamsi(reservation.ReserveDate)} ساعت {reservation.ReserveTime:hh\\:mm} ثبت شد.",
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    RegisterData = DateTime.Now
                });
            }

            Db.SaveChanges();
        }
        catch
        {
            // خطا در ثبت اعلان مانع از رزرو نمی‌شود
        }
    }

    private static ReservationViewModel MapToViewModel(Tbl_Reservation r)
    {
        return new ReservationViewModel
        {
            Tc = r.Tc,
            TrackingCode = r.TrackingCode,
            CustomerTC = r.CustomerTC,
            CustomerName = r.CustomerName,
            MobileNumber = r.MobileNumber,
            PersonalTC = r.PersonalTC,
            PersonalName = r.PersonalName ?? string.Empty,
            ServiceTC = r.ServiceTC,
            ServiceName = r.ServiceName,
            ReserveDate = r.ReserveDate,
            PersianDate = PersianDateHelper.ToShamsi(r.ReserveDate),
            ReserveTime = r.ReserveTime.ToString(@"hh\:mm"),
            DurationMinutes = r.DurationMinutes,
            BasePrice = r.Price,
            DiscountCode = r.DiscountCode,
            DiscountAmount = r.DiscountAmount,
            PayableAmount = r.PayableAmount,
            PaidAmount = r.PaidAmount,
            PaymentMethod = r.PaymentMethod,
            PaymentStatus = r.PaymentStatus,
            Status = r.Status,
            ReservationType = r.ReservationType,
            VariablePriceAgreed = r.VariablePriceAgreed,
            CustomerNote = r.CustomerNote,
            RefundAmount = r.RefundAmount,
            RefundStatus = r.RefundStatus,
            RefundDate = r.RefundDate,
            RegisterData = r.RegisterData
        };
    }

    private Guid EnsureCustomer(string fullName, string mobileNumber)
    {
        var mobile = Db.Tbl_Mobile.FirstOrDefault(m => m.MobileNumber == mobileNumber && !m.IsDelete);
        if (mobile is null)
        {
            mobile = new Tbl_Mobile
            {
                Tc = Guid.NewGuid(),
                MobileNumber = mobileNumber,
                TcPersonal = string.Empty,
                IsActive = true,
                RegisterData = DateTime.Now
            };
            Db.Tbl_Mobile.Add(mobile);
            Db.SaveChanges();
        }

        var customer = Db.Tbl_Customer.FirstOrDefault(c => c.Tc == mobile.Tc && !c.IsDelete);
        if (customer is null)
        {
            customer = new Tbl_Customer
            {
                Tc = mobile.Tc,
                FullName = string.IsNullOrWhiteSpace(fullName) ? "مشتری سالن" : fullName,
                IsActive = true,
                RegisterData = DateTime.Now
            };
            Db.Tbl_Customer.Add(customer);
            Db.SaveChanges();
        }

        return customer.Tc;
    }

    private static string NormalizeDigits(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        const string fa = "۰۱۲۳۴۵۶۷۸۹", ar = "٠١٢٣٤٥٦٧٨٩";
        var sb = new StringBuilder();
        foreach (var ch in input.Trim())
        {
            var i = fa.IndexOf(ch);
            var j = ar.IndexOf(ch);
            if (i >= 0) sb.Append((char)('0' + i));
            else if (j >= 0) sb.Append((char)('0' + j));
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_') continue;
            else sb.Append(ch);
        }
        return sb.ToString();
    }
}
