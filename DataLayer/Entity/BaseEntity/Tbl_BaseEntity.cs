using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity.BaseEntity
{
    public class Tbl_BaseEntity
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public Guid Tc { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime RegisterData { get; set; }
    }
}
