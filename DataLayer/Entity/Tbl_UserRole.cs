using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
    public class Tbl_UserRole:Tbl_BaseEntity
    {
        [Display(Name = "نقش کاربری")]
        [Required(ErrorMessage = "انتخاب نقش الزامی است")]
        public Guid RoleTC { get; set; }


        [Display(Name = "پرسنل")]
        [Required(ErrorMessage = "انتخاب پرسنل الزامی است")]
        public Guid PersonalTC { get; set; }
    }
}
