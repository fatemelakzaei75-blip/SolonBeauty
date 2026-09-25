using DataLayer.Entity.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entity
{
    /// <summary>تنظیمات جامع سالن، تم، ساعات کاری، ساختار سالن و شبکه‌های اجتماعی (بندهای ۱، ۲، ۴، ۶، ۲۲ و ۲۳)</summary>
    public class Tbl_SalonSetting : Tbl_BaseEntity
    {
        [Display(Name = "نام سالن")]
        [Required(ErrorMessage = "نام سالن الزامی است")]
        [MaxLength(100)]
        public string SalonName { get; set; } = "سالن زیبایی حدیث";

        [Display(Name = "عنوان لاتین")]
        [MaxLength(100)]
        public string SalonLatinName { get; set; } = "Hadis Beauty";

        [Display(Name = "زیرعنوان هیرو")]
        [MaxLength(150)]
        public string HeroSubtitle { get; set; } = "تجربه‌ای آرام، لوکس و حرفه‌ای از خدمات زیبایی";

        [Display(Name = "متن معرفی سالن")]
        [MaxLength(1000)]
        public string IntroText { get; set; } = "سالن زیبایی حدیث با بهره‌گیری از کادری مجرب و به‌روزترین تکنیک‌های بین‌المللی، ارائه‌دهنده تخصصی‌ترین خدمات زیبایی، مراقبت و سلامت پوست و مو در محیطی کاملاً آرام و اختصاصی است.";

        [Display(Name = "متن درباره ما")]
        [MaxLength(2000)]
        public string AboutText { get; set; } = "ما در سالن زیبایی حدیث باور داریم که زیبایی طبیعی هر فرد شایسته درخشش است. با تکیه بر متریال درجه یک اروپایی، رعایت سخت‌گیرانه‌ترین استانداردهای بهداشتی و مشاوره تخصصی قبل از هر خدمت، همراه مطمئن زیبایی شما هستیم.";

        // ---------- ساختار سالن: تک لاینه یا چند لاینه (بند ۲ و ۳) ----------
        [Display(Name = "حالت سالن تک‌لاینه است؟")]
        public bool IsSingleLineMode { get; set; } = false;

        [Display(Name = "اسلاگ لاین فعال در حالت تک‌لاینه")]
        [MaxLength(50)]
        public string ActiveLineSlug { get; set; } = "nail";

        // ---------- ساعت فعالیت نرم‌افزار (بند ۲۲) ----------
        [Display(Name = "ساعت شروع فعالیت")]
        public TimeSpan OpeningHour { get; set; } = new TimeSpan(8, 0, 0);

        [Display(Name = "ساعت پایان فعالیت")]
        public TimeSpan ClosingHour { get; set; } = new TimeSpan(22, 0, 0);

        [Display(Name = "متن خارج از ساعت کاری")]
        [MaxLength(200)]
        public string OffHoursMessage { get; set; } = "سالن در حال حاضر خارج از ساعات کاری است. رزروهای قبلی برقرار هستند و می‌توانید برای روزهای آینده نوبت خود را ثبت کنید.";

        // ---------- تم نرم‌افزار (بند ۲۳: کلاسیک، مدرن، لوکس، مینیمال، تیره) ----------
        [Display(Name = "تم فعال نرم‌افزار")]
        [MaxLength(50)]
        public string Theme { get; set; } = "luxury"; // luxury, classic, modern, minimal, dark

        // ---------- اطلاعات تماس و آدرس (بند ۴ و ۵ و ۶) ----------
        [Display(Name = "تلفن ثابت")]
        [MaxLength(20)]
        public string PhoneLandline { get; set; } = "021-12345678";

        [Display(Name = "تلفن همراه")]
        [MaxLength(20)]
        public string PhoneMobile { get; set; } = "09120000001";

        [Display(Name = "ایمیل")]
        [MaxLength(100)]
        public string Email { get; set; } = "info@hadisbeauty.salon";

        [Display(Name = "آدرس کامل")]
        [MaxLength(300)]
        public string Address { get; set; } = "تهران، زعفرانیه، خیابان مقدس اردبیلی، پلاک ۲۴، ساختمان تجاری حدیث، طبقه ۳";

        [Display(Name = "ساعات کاری نمایشی")]
        [MaxLength(150)]
        public string WorkingHoursDisplay { get; set; } = "شنبه تا پنج‌شنبه: ۹:۰۰ الی ۲۱:۰۰ (جمعه‌ها با هماهنگی قبلی)";

        // ---------- مدیریت شبکه‌های اجتماعی (بند ۱) ----------
        [Display(Name = "اینستاگرام")]
        [MaxLength(150)]
        public string InstagramUrl { get; set; } = "https://instagram.com/hadisbeauty.salon";

        [Display(Name = "تلگرام")]
        [MaxLength(150)]
        public string TelegramUrl { get; set; } = "https://t.me/hadisbeauty";

        [Display(Name = "واتس‌اپ")]
        [MaxLength(150)]
        public string WhatsAppUrl { get; set; } = "https://wa.me/989120000001";

        [Display(Name = "ایتا / بله")]
        [MaxLength(150)]
        public string DomesticSocialUrl { get; set; } = "https://eitaa.com/hadisbeauty";

        // تصاویر اسلایدر هیرو
        [Display(Name = "تصاویر اسلایدر هیرو (با کاما جدا شود)")]
        [MaxLength(1000)]
        public string HeroSliderImages { get; set; } = "img/hero.jpg,img/gallery-1.jpg,img/gallery-2.jpg,img/gallery-3.jpg";
    }
}
