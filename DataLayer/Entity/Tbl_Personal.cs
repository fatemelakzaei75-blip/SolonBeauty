using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>اطلاعات تفصیلی پرسنل سالن و وضعیت تأیید (بند ۱۶ سند)</summary>
    public class Tbl_Personal : Tbl_BaseEntity
    {
        public Guid salon_Tc { get; set; }

        [Display(Name = "نام")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "نام خانوادگی")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "نام و نام خانوادگی")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Display(Name = "کد پرسنلی سیستمی (TC)")]
        [MaxLength(50)]
        public string PersonalCode { get; set; } = string.Empty;

        [Display(Name = "شماره موبایل")]
        [MaxLength(15)]
        public string Mobile { get; set; } = string.Empty;

        [Display(Name = "تصویر پرسنل")]
        [MaxLength(250)]
        public string? Avatar { get; set; }

        [Display(Name = "تخصص")]
        [MaxLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [Display(Name = "لاین تخصصی")]
        [MaxLength(100)]
        public string Line { get; set; } = string.Empty;

        [Display(Name = "سابقه کار (سال)")]
        public int ExperienceYears { get; set; } = 3;

        [Display(Name = "توضیحات و سوابق")]
        [MaxLength(1000)]
        public string? Bio { get; set; }

        [Display(Name = "روزهای کاری")]
        [MaxLength(100)]
        public string WorkingDays { get; set; } = "شنبه تا چهارشنبه";

        [Display(Name = "ساعات کاری")]
        [MaxLength(100)]
        public string WorkingHours { get; set; } = "۰۹:۰۰ الی ۱۸:۰۰";

        [Display(Name = "تأیید شده توسط مدیر سالن")]
        public bool IsApproved { get; set; } = true;

        [Display(Name = "تاریخ تولد")]
        public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-25);

        [Display(Name = "TC سرویس")]
        public Guid TC_Service { get; set; }
    }
}
