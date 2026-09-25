using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    public class Tbl_CategoryPortfolio : Tbl_BaseEntity
    {
        [Display(Name = "TC سالن")]
        [Required(ErrorMessage = "TC سالن الزامی است")]
        public Guid SalonTC { get; set; }

        [Display(Name = "نام دسته‌بندی نمونه")]
        [Required(ErrorMessage = "نام دسته‌بندی نمونه الزامی است")]
        [MaxLength(100, ErrorMessage = "نام دسته‌بندی نمونه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string CategoryNameSample { get; set; } = string.Empty;

        [Display(Name = "تصویر نمونه")]
        [Required(ErrorMessage = "تصویر نمونه الزامی است")]
        public string SamplePic { get; set; } = string.Empty;
    }
}
