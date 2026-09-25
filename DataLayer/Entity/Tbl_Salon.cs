using DataLayer.Entity.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    public class Tbl_Salon : Tbl_BaseEntity
    {
        [Display(Name = "تلفن اول")]
        [Required(ErrorMessage = "شماره تلفن اول الزامی است")]
        [RegularExpression(@"^0\d{10}$", ErrorMessage = "شماره تلفن اول معتبر نیست")]
        public string Phone1 { get; set; } = string.Empty;

        [Display(Name = "تلفن دوم")]
        [RegularExpression(@"09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        public string? Phone2 { get; set; }

        [Display(Name = "لوگوی اول")]
        public string? Logo1 { get; set; }

        [Display(Name = "لوگوی دوم")]
        public string? Logo2 { get; set; }

        [Display(Name = "متن سالن")]
        [Required(ErrorMessage = "متن سالن الزامی است")]
        [MaxLength(500, ErrorMessage = "متن سالن نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string TextSalon { get; set; } = string.Empty;
    }
}
