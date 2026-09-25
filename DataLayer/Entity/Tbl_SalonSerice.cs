using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
    public class Tbl_SalonSerice:Tbl_BaseEntity
    {
        [Display(Name = "TC سالن")]
        [Required(ErrorMessage = "TC سالن الزامی است")]
        public Guid SalonTC { get; set; }
        [Display(Name = "TC دسته‌بندی")]
        [Required(ErrorMessage = "TC دسته‌بندی الزامی است")]
        public Guid CategoryTC { get; set; }
        [Display(Name = "نام سرویس")]
        [Required(ErrorMessage = "نام سرویس الزامی است")]
        [MaxLength(150, ErrorMessage = "نام سرویس نمی‌تواند بیشتر از 150 کاراکتر باشد")]
        public required string ServiceName { get; set; }
        [Display(Name = "توضیحات")]
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        public string? Description { get; set; }
        [Display(Name = "قیمت")]
        [Required(ErrorMessage = "قیمت الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
        public decimal Price { get; set; }
        [Display(Name = "دسته اصلی")]
        public bool Main { get; set; }
        [Display(Name = "نمونه کار")]
        public string? PortfolioSample { get; set; }
    }
}
