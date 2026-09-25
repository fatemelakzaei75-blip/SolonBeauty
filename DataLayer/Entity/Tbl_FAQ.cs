using DataLayer.Entity.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>پرسش‌های متداول دسته‌بندی‌شده (بند ۲ سند)</summary>
    public class Tbl_FAQ : Tbl_BaseEntity
    {
        [Display(Name = "دسته‌بندی")]
        [Required(ErrorMessage = "دسته‌بندی الزامی است")]
        [MaxLength(100)]
        public string Category { get; set; } = "نوبت‌دهی";

        [Display(Name = "سؤال")]
        [Required(ErrorMessage = "سؤال الزامی است")]
        [MaxLength(300)]
        public string Question { get; set; } = string.Empty;

        [Display(Name = "پاسخ")]
        [Required(ErrorMessage = "پاسخ الزامی است")]
        [MaxLength(1500)]
        public string Answer { get; set; } = string.Empty;

        [Display(Name = "ترتیب نمایش")]
        public int OrderIndex { get; set; }

        [Display(Name = "منتشر شده")]
        public bool IsPublished { get; set; } = true;
    }
}
