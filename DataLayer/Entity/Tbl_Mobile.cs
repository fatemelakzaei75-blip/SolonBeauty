using DataLayer.Entity.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    public class Tbl_Mobile : Tbl_BaseEntity
    {
        [Display(Name = "شماره همراه کاربر")]
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [RegularExpression(@"09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        public string MobileNumber { get; set; } = string.Empty;

        public string TcPersonal { get; set; } = string.Empty;
    }
}
