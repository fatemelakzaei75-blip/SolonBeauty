using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
    public class Tbl_Roles:Tbl_BaseEntity
    {
        [Display(Name = "نام نقش")]
        [Required(ErrorMessage = "نام نقش الزامی است")]
        [MaxLength(100, ErrorMessage = "نام نقش نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public required string RoleName { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string? Description { get; set; }
    }
}
