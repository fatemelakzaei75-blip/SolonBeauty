using System;
using System.ComponentModel.DataAnnotations;
using DataLayer.Entity;

namespace DataCore.Models.ViewModels;

public class ReservationViewModel
{
    public Guid Tc { get; set; }

    public string TrackingCode { get; set; } = string.Empty;

    public Guid CustomerTC { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public Guid PersonalTC { get; set; }

    public string PersonalName { get; set; } = string.Empty;

    public Guid ServiceTC { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public DateTime ReserveDate { get; set; }

    public string PersianDate { get; set; } = string.Empty;

    public string ReserveTime { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public decimal BasePrice { get; set; }

    public string? DiscountCode { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal PayableAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public ReservationStatus Status { get; set; }

    public ReservationType ReservationType { get; set; }

    public bool VariablePriceAgreed { get; set; }

    public string? CustomerNote { get; set; }

    public decimal RefundAmount { get; set; }

    public string? RefundStatus { get; set; }

    public DateTime? RefundDate { get; set; }

    public DateTime RegisterData { get; set; }
}

public class ReservationCreateDto
{
    [Required(ErrorMessage = "نام مشتری الزامی است")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "فرمت شماره موبایل باید ۱۱ رقمی باشد")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب پرسنل الزامی است")]
    public Guid PersonalTC { get; set; }

    [Required(ErrorMessage = "انتخاب خدمت الزامی است")]
    public Guid ServiceTC { get; set; }

    [Required(ErrorMessage = "تاریخ نوبت الزامی است")]
    public DateTime ReserveDate { get; set; }

    [Required(ErrorMessage = "ساعت نوبت الزامی است")]
    public string ReserveTime { get; set; } = string.Empty;

    public string? DiscountCode { get; set; }

    public decimal DepositAmount { get; set; }

    public bool VariablePriceAgreed { get; set; }

    public string? CustomerNote { get; set; }
}

public class InPersonReservationDto
{
    [Required(ErrorMessage = "نام مشتری الزامی است")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    public Guid PersonalTC { get; set; }

    public Guid ServiceTC { get; set; }

    public DateTime ReserveDate { get; set; }

    public string ReserveTime { get; set; } = string.Empty;

    public decimal PaidAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CardReader;

    public string? CustomerNote { get; set; }
}

public class ReservationFilterDto
{
    public Guid? CustomerTC { get; set; }

    public Guid? PersonalTC { get; set; }

    public DateTime? Date { get; set; }

    public ReservationStatus? Status { get; set; }

    public string? Search { get; set; }
}

public class RefundReservationDto
{
    public Guid ReservationTc { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "مبلغ استرداد نمی‌تواند منفی باشد")]
    public decimal RefundAmount { get; set; }

    public string? Reason { get; set; }

    public string? ReferenceCode { get; set; }
}

public class PaymentInitiateResultDto
{
    public string Authority { get; set; } = string.Empty;

    public string PaymentUrl { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string TrackingCode { get; set; } = string.Empty;
}

public class PaymentVerificationDto
{
    public string Authority { get; set; } = string.Empty;

    public string Status { get; set; } = "OK";
}

public class PaymentVerificationResultDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string RefId { get; set; } = string.Empty;

    public string TrackingCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
