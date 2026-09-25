using System;
using System.Linq;
using DataLayer.Context;
using DataLayer.Entity;

namespace WebApiSolon.Services;

/// <summary>داده اولیه برای حالت توسعه و دیتابیس درون‌حافظه‌ای</summary>
public static class DevDataSeeder
{
    public static readonly Guid SalonTc = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static void Seed(DatabaseContext db)
    {
        if (db.Tbl_Salon.Any()) return;
        var now = DateTime.Now;

        // تنظیمات سالن
        if (!db.Tbl_SalonSetting.Any())
        {
            db.Tbl_SalonSetting.Add(new Tbl_SalonSetting
            {
                Tc = Guid.NewGuid(),
                IsActive = true,
                RegisterData = now,
                SalonName = "سالن زیبایی حدیث",
                SalonLatinName = "Hadis Beauty",
                Theme = "luxury",
                IsSingleLineMode = false,
                OpeningHour = new TimeSpan(8, 0, 0),
                ClosingHour = new TimeSpan(22, 0, 0)
            });
        }

        db.Tbl_Salon.Add(new Tbl_Salon
        {
            Tc = SalonTc, IsActive = true, RegisterData = now,
            Phone1 = "02112345678", Phone2 = "09120000001", Logo1 = "img/hero.jpg",
            TextSalon = "سالن زیبایی حدیث؛ تجربه‌ای آرام، لوکس و حرفه‌ای از خدمات زیبایی."
        });

        var cats = new[] { "مو", "پوست", "ناخن", "میکاپ و عروس", "مژه و ابرو" }
            .Select(n => new Tbl_Category { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, SalonTC = SalonTc, CategoryName = n })
            .ToList();
        db.Tbl_Category.AddRange(cats);

        var services = new (string Name, int Cat, decimal Price, bool Main)[]
        {
            ("رنگ و لایت", 0, 2_500_000, true), ("کراتین و احیا", 0, 3_800_000, true),
            ("کوپ و براشینگ", 0, 900_000, false), ("فیشال پوست", 1, 1_400_000, true),
            ("کاشت ناخن", 2, 1_900_000, true), ("مانیکور و پدیکور", 2, 700_000, false),
            ("میکاپ عروس", 3, 7_500_000, true), ("شنیون", 3, 2_200_000, false),
            ("اکستنشن مژه", 4, 1_200_000, true), ("لیفت و لمینت مژه", 4, 950_000, false),
        };
        db.Tbl_SalonSerice.AddRange(services.Select(s => new Tbl_SalonSerice
        {
            Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, SalonTC = SalonTc,
            CategoryTC = cats[s.Cat].Tc, ServiceName = s.Name, Price = s.Price, Main = s.Main,
            Description = $"خدمت {s.Name} در سالن زیبایی حدیث"
        }));

        var roles = new[]
        {
            new Tbl_Roles { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, RoleName = "Admin", Description = "مدیر سیستم" },
            new Tbl_Roles { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, RoleName = "Personnel", Description = "پرسنل سالن" },
            new Tbl_Roles { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, RoleName = "Customer", Description = "مشتری عادی" },
        };
        db.Tbl_Roles.AddRange(roles);

        // پرسنل سالن (بند ۱۶ سند)
        var staffList = new[]
        {
            new Tbl_Personal { Tc = Guid.NewGuid(), FirstName = "سارا", LastName = "محمدی", Specialty = "متخصص کاشت و طراحی ناخن", Line = "ناخن", Mobile = "09120000002", IsApproved = true, IsActive = true, RegisterData = now, WorkingHours = "۰۹:۰۰ الی ۱۸:۰۰", WorkingDays = "شنبه تا چهارشنبه", Avatar = "img/gallery-1.jpg", Bio = "دارای مدرک بین‌المللی مانیکور روسی و ۶ سال سابقه کار تخصصی در لاین ناخن." },
            new Tbl_Personal { Tc = Guid.NewGuid(), FirstName = "مریم", LastName = "اسدی", Specialty = "استاد رنگ، بالیاژ و احیا", Line = "مو", Mobile = "09120000004", IsApproved = true, IsActive = true, RegisterData = now, WorkingHours = "۱۰:۰۰ الی ۱۹:۰۰", WorkingDays = "یکشنبه تا پنج‌شنبه", Avatar = "img/work-hair.jpg", Bio = "مدرس رنگ و تکنیک‌های نوین بالیاژ و کالرملتینگ با ۸ سال تجربه درخشان." },
            new Tbl_Personal { Tc = Guid.NewGuid(), FirstName = "حدیث", LastName = "رحیمی", Specialty = "میکاپ آرتیست و شنیون عروس", Line = "عروس و میکاپ", Mobile = "09120000005", IsApproved = true, IsActive = true, RegisterData = now, WorkingHours = "۰۸:۰۰ الی ۱۷:۰۰", WorkingDays = "تمامی روزها با هماهنگی", Avatar = "img/work-bridal.jpg", Bio = "مدیریت لاین عروس، فارغ‌التحصیل از آکادمی‌های برتر گریم استانبول و دبی." },
            new Tbl_Personal { Tc = Guid.NewGuid(), FirstName = "الهام", LastName = "نوری", Specialty = "اسکین کر و فیشالیست", Line = "پوست و فیشال", Mobile = "09120000006", IsApproved = true, IsActive = true, RegisterData = now, WorkingHours = "۰۹:۰۰ الی ۱۷:۰۰", WorkingDays = "شنبه تا چهارشنبه", Avatar = "img/gallery-3.jpg", Bio = "دارای گواهی‌نامه رسمی پاکسازی عمقی، هیدرافیشال و درمان‌های اختصاصی پوست." },
            new Tbl_Personal { Tc = Guid.NewGuid(), FirstName = "نگین", LastName = "کاظمی", Specialty = "اکستنشن مژه و لیفت ابرو", Line = "مژه و ابرو", Mobile = "09120000007", IsApproved = true, IsActive = true, RegisterData = now, WorkingHours = "۱۱:۰۰ الی ۲۰:۰۰", WorkingDays = "شنبه تا پنج‌شنبه", Avatar = "img/gallery-2.jpg", Bio = "مستر اکستنشن مژه تار به تار، والیوم و مگاوالیوم با برترین چسب‌های ضدحساسیت." },
        };
        db.Tbl_Personal.AddRange(staffList);

        // کدهای تخفیف (بند ۸ سند)
        db.Tbl_Discount.AddRange(new[]
        {
            new Tbl_Discount { Tc = Guid.NewGuid(), Code = "HADIS20", Title = "تخفیف ۲۰٪ افتتاحیه آنلاین", Percent = 20, MaxDiscount = 300_000, IsActive = true, RegisterData = now },
            new Tbl_Discount { Tc = Guid.NewGuid(), Code = "BEAUTY10", Title = "تخفیف ۱۰٪ مشتریان وفادار", Percent = 10, MaxDiscount = 150_000, IsActive = true, RegisterData = now },
        });

        // نظرات مشتریان (بند ۲ سند)
        db.Tbl_Comment.AddRange(new[]
        {
            new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "زهرا کمالی", ServiceName = "کاشت ناخن و مانیکور", Rating = 5, CommentText = "محیط بسیار آرام و لوکس، دقت و بهداشت سارا جان واقعاً عالی بود. به همه پیشنهاد می‌کنم.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-2) },
            new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "مهسا تهرانی", ServiceName = "میکاپ و شنیون عروس", Rating = 5, CommentText = "حدیث جون هنر دستتون بی‌نظیره! آرایش من دقیقاً همون چیزی شد که می‌خواستم و تا آخر مراسم تکون نخورد.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-5) },
            new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "پریسا راد", ServiceName = "فیشال و پاکسازی پوست", Rating = 5, CommentText = "پوستم واقعاً شفاف و درخشان شد. ماساژ صورت و محصولات الهام خانم حس سبکی فوق‌العاده‌ای داشت.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-7) },
            new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "ساناز امینی", ServiceName = "بالیاژ مو", Rating = 5, CommentText = "بدون ذره‌ای سوختگی یا خشکی مو، رنگ موهام دقیقاً رنگی شد که از ژورنال نشون داده بودم. مریم جان کارشون بیستِ.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-10) }
        });

        // سوالات متداول (بند ۲ سند)
        db.Tbl_FAQ.AddRange(new[]
        {
            new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "نوبت‌دهی و رزرو", Question = "چگونه می‌توانم نوبت خود را آنلاین ثبت کنم؟", Answer = "کافی است از بخش رزرو نوبت، لاین یا خدمت موردنظر و تاریخ و ساعت خالی پرسنل دلخواهتان را انتخاب کنید و رزرو را قطعی فرمایید.", OrderIndex = 1, IsPublished = true, IsActive = true, RegisterData = now },
            new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "نوبت‌دهی و رزرو", Question = "آیا امکان ثبت در لیست انتظار در صورت پر بودن وقت‌ها وجود دارد؟", Answer = "بله، در صورتی که زمان موردنظر شما تکمیل شده باشد، می‌توانید در لیست انتظار ثبت‌نام کنید تا به محض کنسلی به شما از طریق پیامک اطلاع‌رسانی شود.", OrderIndex = 2, IsPublished = true, IsActive = true, RegisterData = now },
            new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "خدمات و مواد مصرفی", Question = "آیا مواد مصرفی برای مو و پوست برند اصل و باکیفیت هستند؟", Answer = "تمامی مواد مصرفی لاین‌های رنگ، کراتین، مراقبت پوست و ناخن مستقیماً از برندهای معتبر اروپایی و دارای برچسب اصالت تهیه می‌شوند.", OrderIndex = 3, IsPublished = true, IsActive = true, RegisterData = now },
            new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "بهداشت و استانداردهای سالن", Question = "فرآیند استریل کردن ابزارها به چه صورت انجام می‌شود؟", Answer = "تمام ابزارهای فلزی مانیکور، پدیکور و گریم پس از هر مشتری در دستگاه اتوکلاو پزشکی استریل شده و ابزارهای یک‌بارمصرف جلوی دید مشتری باز می‌شوند.", OrderIndex = 4, IsPublished = true, IsActive = true, RegisterData = now }
        });

        // اخبار و مقالات (بند ۲۱ سند)
        db.Tbl_NewsDay.AddRange(new[]
        {
            new Tbl_NewsDay { Tc = Guid.NewGuid(), Title = "راهنمای جامع مراقبت از موهای رنگ و دکلره شده", TextNews = "موهای دکلره شده به دلیل باز شدن کوتیکول‌ها نیازمند رطوبت‌رسانی عمیق و استفاده از ماسک‌های حاوی پروتئین و روغن‌های گیاهی هستند...", Author = "تیم تخصصی حدیث بیوتی", Image = "img/work-hair.jpg", IsPublished = true, IsActive = true, RegisterData = now.AddDays(-3), DateStart = now.AddDays(-3), DateEnd = now.AddYears(1) },
            new Tbl_NewsDay { Tc = Guid.NewGuid(), Title = "ترندهای آرایش و میکاپ عروس در سال ۲۰۲۶", TextNews = "سبک آرایش سافت گلم و پوست شفاف شیشه‌ای با درخشش طبیعی، محبوب‌ترین ترند میکاپ عروس امسال است...", Author = "حدیث رحیمی", Image = "img/work-bridal.jpg", IsPublished = true, IsActive = true, RegisterData = now.AddDays(-6), DateStart = now.AddDays(-6), DateEnd = now.AddYears(1) },
            new Tbl_NewsDay { Tc = Guid.NewGuid(), Title = "مراحل فیشال کلاسیک و تأثیر آن بر جوانسازی", TextNews = "فیشال منظم ماهانه با حذف سلول‌های مرده و پاکسازی منافذ، کلاژن‌سازی طبیعی پوست را تحریک کرده و مانع از چین و چروک زودرس می‌شود...", Author = "الهام نوری", Image = "img/gallery-3.jpg", IsPublished = true, IsActive = true, RegisterData = now.AddDays(-10), DateStart = now.AddDays(-10), DateEnd = now.AddYears(1) }
        });

        // اعلان پیش‌فرض به سیستم (بند ۱۷ سند)
        db.Tbl_Notification.Add(new Tbl_Notification
        {
            Tc = Guid.NewGuid(),
            TargetRole = "Admin",
            Title = "به سامانه مدیریت سالن خوش آمدید",
            Message = "سامانه مدیریت جامع Hadis Beauty با کلیه امکانات رزرو آنلاین، تقویم شمسی و لیست انتظار فعال شد.",
            IsRead = false,
            CreatedAt = now,
            IsActive = true,
            RegisterData = now
        });

        db.SaveChanges();
    }
}
