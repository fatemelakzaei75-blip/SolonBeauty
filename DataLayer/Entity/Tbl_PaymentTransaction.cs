using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>تراکنش درگاه پرداخت زرین‌پال با رعایت Idempotency (بند ۹ و ۱۰)</summary>
    public class Tbl_PaymentTransaction : Tbl_BaseEntity
    {
        [Display(Name = "شماره رزرو")]
        public Guid ReservationTC { get; set; }

        [Display(Name = "کد پیگیری سیستمی")]
        [MaxLength(50)]
        public string TrackingCode { get; set; } = string.Empty;

        [Display(Name = "مبلغ تراکنش (تومان)")]
        public decimal Amount { get; set; }

        [Display(Name = "نام درگاه")]
        [MaxLength(50)]
        public string GatewayName { get; set; } = "ZarinPal";

        [Display(Name = "کد اتوریتی (Authority)")]
        [MaxLength(100)]
        public string? Authority { get; set; }

        [Display(Name = "شماره مرجع / پیگیری بانکی (RefID)")]
        [MaxLength(100)]
        public string? RefId { get; set; }

        [Display(Name = "وضعیت پرداخت")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Display(Name = "شرح وضعیت")]
        [MaxLength(250)]
        public string? StatusDescription { get; set; }

        [Display(Name = "کلید یکتایی (Idempotency Key)")]
        [MaxLength(100)]
        public string IdempotencyKey { get; set; } = string.Empty;

        [Display(Name = "تاریخ اعتبارسنجی")]
        public DateTime? VerifiedAt { get; set; }
    }
}
