using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
    public class Tbl_RolePermission:Tbl_BaseEntity
    {
        [Display(Name = "دسترسی")]
        [Required(ErrorMessage = "انتخاب دسترسی الزامی است")]
        public Guid PermissionTC { get; set; }
    }
}
