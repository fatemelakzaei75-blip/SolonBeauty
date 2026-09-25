using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>اعلان‌های سیستمی و کارتابل پرسنل و ادمین (بند ۱۷ سند)</summary>
    public class Tbl_Notification : Tbl_BaseEntity
    {
        [Display(Name = "نقش هدف")]
        [MaxLength(50)]
        public string TargetRole { get; set; } = "Admin"; // Admin, Personnel, Customer

        [Display(Name = "TC کاربر هدف")]
        public Guid? TargetUserTC { get; set; }

        [Display(Name = "عنوان اعلان")]
        [Required(ErrorMessage = "عنوان اعلان الزامی است")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "متن اعلان")]
        [Required(ErrorMessage = "متن اعلان الزامی است")]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "خوانده شده؟")]
        public bool IsRead { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get => RegisterData; set => RegisterData = value; }

        [Display(Name = "لینک")]
        [MaxLength(200)]
        public string? Link { get; set; }
    }
}
