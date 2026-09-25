using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
    public class Tbl_Customer:Tbl_BaseEntity
    {
        [Display(Name = "نام و نام خانوادگی")]
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
        [MaxLength(100, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public required string FullName { get; set; }
        [Display(Name = "کد معرف")]
        [MaxLength(50, ErrorMessage = "کد معرف نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string? CodeMoaref { get; set; }
        [Display(Name = "مشتری اول")]
        public int? BirthdayCustomer { get; set; }
        [Display(Name = "مشتری اصلی")]
        public int? PicCustomer { get; set; }
    }
}

