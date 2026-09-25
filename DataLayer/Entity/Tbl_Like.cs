using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
    public class Tbl_Like:Tbl_BaseEntity
    {
        [Display(Name = "مشتری")]
        [Required(ErrorMessage = "انتخاب مشتری الزامی است")]
        public Guid CustomerTC { get; set; }
        [Display(Name = "نمونه کار")]
        [Required(ErrorMessage = "انتخاب نمونه کار الزامی است")]
        public Guid ProfileTC { get; set; }
    }
}
