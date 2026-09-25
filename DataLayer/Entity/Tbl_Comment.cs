using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>نظرات مشتریان برای نمایش در صفحه اصلی (بند ۲ سند)</summary>
    public class Tbl_Comment : Tbl_BaseEntity
    {
        [Display(Name = "نام مشتری")]
        [Required(ErrorMessage = "نام مشتری الزامی است")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "نام خدمت")]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [Display(Name = "امتیاز (۱ تا ۵)")]
        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد")]
        public int Rating { get; set; } = 5;

        [Display(Name = "متن نظر")]
        [Required(ErrorMessage = "متن نظر الزامی است")]
        [MaxLength(1000)]
        public string CommentText { get; set; } = string.Empty;

        [Display(Name = "تأیید شده برای نمایش عمومی")]
        public bool IsApproved { get; set; } = true;
    }
}
