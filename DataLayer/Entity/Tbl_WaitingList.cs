using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    public enum WaitingStatus
    {
        [Display(Name = "در انتظار نوبت")] Waiting = 0,
        [Display(Name = "اطلاع‌رسانی شده")] Notified = 1,
        [Display(Name = "تأیید و تبدیل به نوبت")] Confirmed = 2,
        [Display(Name = "منقضی شده")] Expired = 3,
        [Display(Name = "لغو شده")] Canceled = 4
    }

    /// <summary>لیست انتظار در زمان پر بودن نوبت‌ها (بند ۱۱ سند)</summary>
    public class Tbl_WaitingList : Tbl_BaseEntity
    {
        [Display(Name = "نام متقاضی")]
        [Required(ErrorMessage = "نام متقاضی الزامی است")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [Display(Name = "نام خدمت")]
        [Required(ErrorMessage = "نام خدمت الزامی است")]
        [MaxLength(150)]
        public string ServiceName { get; set; } = string.Empty;

        [Display(Name = "نام پرسنل انتخابی")]
        [MaxLength(100)]
        public string? PersonalName { get; set; }

        [Display(Name = "تاریخ مدنظر")]
        public DateTime DesiredDate { get; set; }

        [Display(Name = "بازه زمانی مدنظر")]
        public TimeSpan DesiredTime { get; set; }

        [Display(Name = "وضعیت")]
        public WaitingStatus Status { get; set; } = WaitingStatus.Waiting;

        [Display(Name = "مهلت تأیید در صورت خالی شدن نوبت")]
        public DateTime? NotifyDeadline { get; set; }

        [Display(Name = "یادداشت متقاضی")]
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
