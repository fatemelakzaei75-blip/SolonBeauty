using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>وضعیت نوبت در چرخه رزرو (بند ۱۰ سند)</summary>
    public enum ReservationStatus
    {
        [Display(Name = "در انتظار پرداخت")] Pending = 0,
        [Display(Name = "در انتظار پرداخت")] PendingPayment = 0,
        [Display(Name = "پرداخت موفق")] PaymentSuccessful = 1,
        [Display(Name = "تأیید قطعی نوبت")] Confirmed = 2,
        [Display(Name = "لغو شده")] Canceled = 3,
        [Display(Name = "منقضی شده")] Expired = 4,
        [Display(Name = "انجام شده")] Done = 5
    }

    /// <summary>نوع رزرو (بند ۱۴ سند)</summary>
    public enum ReservationType
    {
        [Display(Name = "آنلاین")] Online = 0,
        [Display(Name = "حضوری")] InPerson = 1,
        [Display(Name = "ادمین")] Admin = 2
    }

    /// <summary>وضعیت پرداخت (بند ۱۰ سند)</summary>
    public enum PaymentStatus
    {
        [Display(Name = "در انتظار پرداخت")] Pending = 0,
        [Display(Name = "هدایت به درگاه")] RedirectedToGateway = 1,
        [Display(Name = "پرداخت شده")] Paid = 2,
        [Display(Name = "ناموفق")] Failed = 3,
        [Display(Name = "لغو شده")] Canceled = 4,
        [Display(Name = "منقضی شده")] Expired = 5,
        [Display(Name = "نیاز به بررسی و وریفای")] NeedsVerification = 6
    }

    /// <summary>روش پرداخت (بند ۱۴ سند)</summary>
    public enum PaymentMethod
    {
        [Display(Name = "درگاه زرین‌پال")] OnlineGateway = 0,
        [Display(Name = "کارت‌خوان سالن")] CardReader = 1,
        [Display(Name = "نقدی")] Cash = 2,
        [Display(Name = "کارت به کارت")] CardToCard = 3
    }

    /// <summary>نوبت رزرو شده مشتری با کلیه اطلاعات مالی و فرآیندی (بند ۸، ۱۰، ۱۴ و ۱۵)</summary>
    public class Tbl_Reservation : Tbl_BaseEntity
    {
        [Display(Name = "کد پیگیری سیستمی (TC)")]
        [MaxLength(50)]
        public string TrackingCode { get; set; } = string.Empty;

        [Display(Name = "نوع رزرو")]
        public ReservationType ReservationType { get; set; } = ReservationType.Online;

        [Display(Name = "TC سالن")]
        public Guid SalonTC { get; set; }

        [Display(Name = "TC مشتری")]
        public Guid CustomerTC { get; set; }

        [Display(Name = "نام مشتری")]
        [Required(ErrorMessage = "نام مشتری الزامی است")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [Display(Name = "TC خدمت")]
        public Guid ServiceTC { get; set; }

        [Display(Name = "نام خدمت")]
        [Required(ErrorMessage = "نام خدمت الزامی است")]
        [MaxLength(150)]
        public string ServiceName { get; set; } = string.Empty;

        [Display(Name = "TC پرسنل")]
        public Guid PersonalTC { get; set; }

        [Display(Name = "نام پرسنل")]
        [MaxLength(100)]
        public string? PersonalName { get; set; }

        [Display(Name = "TC نمونه کار مرجع")]
        public Guid? PortfolioTC { get; set; }

        [Display(Name = "تاریخ نوبت (میلادی در دیتابیس)")]
        [Required(ErrorMessage = "تاریخ نوبت الزامی است")]
        public DateTime ReserveDate { get; set; }

        [Display(Name = "ساعت نوبت")]
        [Required(ErrorMessage = "ساعت نوبت الزامی است")]
        public TimeSpan ReserveTime { get; set; }

        [Display(Name = "مدت زمان (دقیقه)")]
        [Range(0, 1440, ErrorMessage = "مدت زمان معتبر نیست")]
        public int DurationMinutes { get; set; } = 60;

        [Display(Name = "مبلغ خدمت")]
        public decimal Price { get; set; }

        [Display(Name = "کد تخفیف اعمال شده")]
        [MaxLength(50)]
        public string? DiscountCode { get; set; }

        [Display(Name = "مبلغ تخفیف")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "مبلغ نهایی قابل پرداخت")]
        public decimal PayableAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "بیعانه / پیش‌پرداخت")]
        public decimal Deposit { get => PaidAmount; set => PaidAmount = value; }

        [Display(Name = "روش پرداخت")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.OnlineGateway;

        [Display(Name = "وضعیت پرداخت")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Display(Name = "تأیید شرایط قیمت متغیر")]
        public bool VariablePriceAgreed { get; set; }

        [Display(Name = "وضعیت نوبت")]
        public ReservationStatus Status { get; set; } = ReservationStatus.PendingPayment;

        [Display(Name = "یادداشت مشتری / توضیحات")]
        [MaxLength(500)]
        public string? CustomerNote { get; set; }

        [Display(Name = "ثبت‌کننده رزرو")]
        [MaxLength(100)]
        public string? RegisteredBy { get; set; }

        [Display(Name = "تاریخ تأیید")]
        public DateTime? ConfirmDate { get; set; }

        [Display(Name = "دلیل لغو")]
        [MaxLength(300)]
        public string? CancelReason { get; set; }

        // فیلدهای مالی استرداد (بند ۱۵ سند)
        [Display(Name = "مبلغ استرداد")]
        public decimal RefundAmount { get; set; }

        [Display(Name = "وضعیت بازپرداخت")]
        [MaxLength(50)]
        public string? RefundStatus { get; set; }

        [Display(Name = "تاریخ استرداد")]
        public DateTime? RefundDate { get; set; }

        [Display(Name = "انجام‌دهنده استرداد")]
        [MaxLength(100)]
        public string? RefundBy { get; set; }

        [Display(Name = "شماره پیگیری استرداد")]
        [MaxLength(100)]
        public string? RefundReference { get; set; }
    }
}
