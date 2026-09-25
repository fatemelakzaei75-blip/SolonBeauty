using DataLayer.Entity.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Entity
{
   
        public class Tbl_Confirm : Tbl_BaseEntity
        {
            [Display(Name = "TC موبایل")]
            [Required(ErrorMessage = "TC موبایل الزامی است")]
            public Guid Tc_Mobile { get; set; }
            [Display(Name = "IP کاربر")]
            [Required(ErrorMessage = "IP کاربر الزامی است")]
            public required string IpUser { get; set; }
            [Display(Name = "کد تأیید")]
            [Required(ErrorMessage = "کد تأیید الزامی است")]
            public required string Code { get; set; }
            [Display(Name = "تأیید شده")]
            public bool IsConfirmed { get; set; }
            [Display(Name = "تعداد ارسال")]
            public int SentCount { get; set; }
            /// <summary>
            /// تعداد تلاش برای وارد کردن کد
            /// </summary>
            [Display(Name = "تعداد تلاش")]
            public int TryCount { get; set; }
            [Display(Name = "تاریخ ارسال")]
            public DateTime SentDate { get; set; }
            [Display(Name = "تاریخ تأیید")]
            public DateTime ConfirmDate { get; set; }
        }
}
