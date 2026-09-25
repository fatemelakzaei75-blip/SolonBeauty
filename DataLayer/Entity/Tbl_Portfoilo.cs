using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    public class Tbl_Portfoilo : Tbl_BaseEntity
    {
        [Display(Name = "دسته‌بندی نمونه‌کار")]
        [Required(ErrorMessage = "انتخاب دسته‌بندی نمونه‌کار الزامی است")]
        public Guid SampleTC { get; set; }

        [Display(Name = "عنوان")]
        [Required(ErrorMessage = "عنوان الزامی است")]
        [StringLength(150, ErrorMessage = "عنوان نمی‌تواند بیشتر از 150 کاراکتر باشد")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        public string? Description { get; set; }

        [Display(Name = "تصویر پروفایل")]
        public string? ProtfiloPic { get; set; }

        [Display(Name = "ویدیو پروفایل")]
        public string? ProtfiloVideo { get; set; }

        [Display(Name = "تصویر قبل")]
        public string? BeforePic { get; set; }

        [Display(Name = "تصویر بعد")]
        public string? AfterPic { get; set; }

        public bool Like { get; set; }
    }
}
