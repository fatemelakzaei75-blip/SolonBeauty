using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
    public class Tbl_Social:Tbl_BaseEntity
    {
        [Display(Name = "تصویر")]
        public string? Pics { get; set; }


        [Display(Name = "لینک شبکه اجتماعی")]
        [Url(ErrorMessage = "لینک شبکه اجتماعی معتبر نیست")]
        public string? SocialLink { get; set; }


        [Display(Name = "سالن زیبایی")]
        [Required(ErrorMessage = "انتخاب سالن الزامی است")]
        public Guid TC_Salon { get; set; }
    }
}
