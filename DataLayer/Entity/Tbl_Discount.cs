using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>کدهای تخفیف با اعتبارسنجی سمت سرور (بند ۸ و ۹)</summary>
    public class Tbl_Discount : Tbl_BaseEntity
    {
        [Display(Name = "کد تخفیف")]
        [Required(ErrorMessage = "کد تخفیف الزامی است")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "عنوان تخفیف")]
        [Required(ErrorMessage = "عنوان تخفیف الزامی است")]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "درصد تخفیف")]
        [Range(1, 100, ErrorMessage = "درصد باید بین ۱ تا ۱۰۰ باشد")]
        public int Percent { get; set; }

        [Display(Name = "سقف تخفیف (تومان)")]
        public decimal MaxDiscount { get; set; }

        [Display(Name = "حداقل مبلغ خرید (تومان)")]
        public decimal MinPurchase { get; set; }

        [Display(Name = "تاریخ انقضا")]
        public DateTime? ExpireDate { get; set; }

        [Display(Name = "تعداد دفعات استفاده شده")]
        public int UsageCount { get; set; }
    }
}
