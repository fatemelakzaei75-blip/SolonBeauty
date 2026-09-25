using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>اخبار و مقالات وبلاگ با تاریخ استاندارد و نمایش شمسی (بند ۲۱ سند)</summary>
    public class Tbl_NewsDay : Tbl_BaseEntity
    {
        [Display(Name = "عنوان خبر / مقاله")]
        [Required(ErrorMessage = "عنوان خبر الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "تصویر شاخص")]
        [MaxLength(250)]
        public string? Image { get; set; }

        [Display(Name = "متن کامل")]
        [Required(ErrorMessage = "متن خبر الزامی است")]
        public string TextNews { get; set; } = string.Empty;

        [Display(Name = "نویسنده")]
        [MaxLength(100)]
        public string Author { get; set; } = "مدیریت سالن حدیث";

        [Display(Name = "وضعیت انتشار")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "تاریخ شروع / انتشار")]
        public DateTime DateStart { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ پایان")]
        public DateTime DateEnd { get; set; } = DateTime.Now.AddYears(1);

        [Display(Name = "زمان شروع")]
        public TimeSpan TimeStart { get; set; } = TimeSpan.Zero;

        [Display(Name = "زمان پایان")]
        public TimeSpan TimeEnd { get; set; } = new TimeSpan(23, 59, 59);

        [Display(Name = "TC پرسنل")]
        public Guid TC_Personal { get; set; }
    }
}
