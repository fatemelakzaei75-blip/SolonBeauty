using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.Entity;
using DataLayer.Entity.BaseEntity;

namespace BlazorAppSolon.Data;

/// <summary>
/// منبع داده محلی (نمونه) برای پنل‌ها تا بدون SQL Server هم کاملاً قابل استفاده باشند.
/// ساختار متدها عمداً شبیه WebApi است تا جایگزینی با HttpClient ساده باشد.
/// </summary>
public class MockDb
{
    private readonly Dictionary<Type, List<Tbl_BaseEntity>> _store = new();

    public static readonly Guid SalonTc = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public MockDb() => Seed();

    private List<Tbl_BaseEntity> Bucket(Type t)
    {
        if (!_store.TryGetValue(t, out var list)) _store[t] = list = new List<Tbl_BaseEntity>();
        return list;
    }

    public List<T> All<T>() where T : Tbl_BaseEntity
        => Bucket(typeof(T)).Where(x => !x.IsDelete).Cast<T>().ToList();

    public T? Find<T>(Guid tc) where T : Tbl_BaseEntity
        => All<T>().FirstOrDefault(x => x.Tc == tc);

    public void Add<T>(T entity) where T : Tbl_BaseEntity
    {
        var list = Bucket(typeof(T));
        entity.ID = list.Count == 0 ? 1 : list.Max(x => x.ID) + 1;
        if (entity.Tc == Guid.Empty) entity.Tc = Guid.NewGuid();
        if (entity.RegisterData == default) entity.RegisterData = DateTime.Now;
        list.Add(entity);
    }

    public void Update<T>(T entity) where T : Tbl_BaseEntity
    {
        var list = Bucket(typeof(T));
        var idx = list.FindIndex(x => x.Tc == entity.Tc);
        if (idx >= 0) list[idx] = entity;
    }

    /// <summary>حذف منطقی</summary>
    public void Delete<T>(Guid tc) where T : Tbl_BaseEntity
    {
        var item = Bucket(typeof(T)).FirstOrDefault(x => x.Tc == tc);
        if (item is not null) { item.IsDelete = true; item.IsActive = false; }
    }

    public int Count<T>() where T : Tbl_BaseEntity => All<T>().Count;

    // --------------------------------------------------------------
    private void Seed()
    {
        var now = DateTime.Now;

        // تنظیمات سالن (بندهای ۱، ۲، ۴، ۶، ۲۲ و ۲۳)
        var setting = new Tbl_SalonSetting
        {
            Tc = Guid.NewGuid(),
            IsActive = true,
            RegisterData = now,
            SalonName = "سالن زیبایی حدیث",
            SalonLatinName = "Hadis Beauty",
            Theme = "luxury",
            IsSingleLineMode = false,
            ActiveLineSlug = "nail",
            OpeningHour = new TimeSpan(8, 0, 0),
            ClosingHour = new TimeSpan(22, 0, 0),
            PhoneLandline = "021-12345678",
            PhoneMobile = "09120000001",
            Email = "info@hadisbeauty.salon",
            Address = "تهران، زعفرانیه، خیابان مقدس اردبیلی، پلاک ۲۴، ساختمان تجاری حدیث، طبقه ۳",
            WorkingHoursDisplay = "شنبه تا پنج‌شنبه: ۹:۰۰ الی ۲۱:۰۰ (جمعه‌ها با هماهنگی)",
            InstagramUrl = "https://instagram.com/hadisbeauty.salon",
            TelegramUrl = "https://t.me/hadisbeauty",
            WhatsAppUrl = "https://wa.me/989120000001",
            HeroSliderImages = "img/hero.jpg,img/gallery-1.jpg,img/gallery-2.jpg,img/gallery-3.jpg"
        };
        Add(setting);

        Add(new Tbl_Salon
        {
            Tc = SalonTc, IsActive = true, RegisterData = now,
            Phone1 = "02112345678", Phone2 = "09120000001",
            Logo1 = "img/hero.jpg",
            TextSalon = "سالن زیبایی حدیث؛ تجربه‌ای آرام، لوکس و حرفه‌ای از خدمات زیبایی."
        });

        var catNames = new[] { "مو", "پوست", "ناخن", "میکاپ و عروس", "مژه و ابرو" };
        var cats = new List<Tbl_Category>();
        foreach (var name in catNames)
        {
            var c = new Tbl_Category { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, SalonTC = SalonTc, CategoryName = name };
            cats.Add(c); Add(c);
        }

        var services = new (string Name, int Cat, decimal Price, bool Main)[]
        {
            ("رنگ و لایت", 0, 2_500_000, true),
            ("کراتین و احیا", 0, 3_800_000, true),
            ("کوپ و براشینگ", 0, 900_000, false),
            ("اکستنشن مو", 0, 6_500_000, false),
            ("فیشال پوست", 1, 1_400_000, true),
            ("ماساژ صورت", 1, 800_000, false),
            ("کاشت ناخن", 2, 1_900_000, true),
            ("مانیکور و پدیکور", 2, 700_000, false),
            ("میکاپ عروس", 3, 7_500_000, true),
            ("شنیون", 3, 2_200_000, false),
            ("اکستنشن مژه", 4, 1_200_000, true),
            ("لیفت و لمینت مژه", 4, 950_000, false),
        };
        foreach (var s in services)
            Add(new Tbl_SalonSerice
            {
                Tc = Guid.NewGuid(), IsActive = true, RegisterData = now,
                SalonTC = SalonTc, CategoryTC = cats[s.Cat].Tc,
                ServiceName = s.Name, Price = s.Price, Main = s.Main,
                Description = $"خدمت {s.Name} در سالن زیبایی حدیث"
            });

        foreach (var role in new[] { ("Admin", "مدیر سیستم"), ("Personnel", "پرسنل سالن"), ("Customer", "مشتری") })
            Add(new Tbl_Roles { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, RoleName = role.Item1, Description = role.Item2 });

        foreach (var p in new[] { "مدیریت اطلاعات پایه", "مدیریت پرسنل", "مدیریت مشتریان", "مدیریت نمونه کار", "مشاهده گزارش‌ها" })
            Add(new Tbl_Permission { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, PermissionName = p });

        var roles = All<Tbl_Roles>();
        foreach (var perm in All<Tbl_Permission>())
            Add(new Tbl_RolePermission { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, PermissionTC = perm.Tc });

        // پرسنل سالن با اطلاعات کامل (بند ۱۶ و ۲۰ سند)
        var staffData = new (string FName, string LName, string Specialty, string Line, string Mobile, string Avatar, string Bio, int Exp)[]
        {
            ("سارا", "محمدی", "متخصص کاشت و طراحی ناخن", "ناخن", "09120000002", "img/gallery-1.jpg", "دارای مدرک بین‌المللی مانیکور روسی و ۶ سال سابقه کار تخصصی.", 6),
            ("مریم", "اسدی", "استاد رنگ و بالیاژ", "مو", "09121110001", "img/work-hair.jpg", "مدرس و متخصص رنگ، لایت و بالیاژ با ۸ سال تجربه تخصصی.", 8),
            ("حدیث", "رحیمی", "میکاپ آرتیست عروس", "عروس و میکاپ", "09121110003", "img/work-bridal.jpg", "مدیریت و میکاپ‌آرتیست لاین عروس با سابقه اجرای صدها پروژه موفق.", 10),
            ("الهام", "نوری", "اسکین کر و فیشال", "پوست و فیشال", "09121110004", "img/gallery-3.jpg", "متخصص پاکسازی عمقی و جوانسازی پوست با محصولات ارگانیک.", 5),
            ("نگین", "کاظمی", "اکستنشن مژه و لیفت", "مژه و ابرو", "09121110005", "img/gallery-2.jpg", "مستر مژه و ابرو، ارائه انواع تکنیک‌های کلاسیک، والیوم و مگاوالیوم.", 4),
        };

        var allServices = All<Tbl_SalonSerice>();
        for (int i = 0; i < staffData.Length; i++)
        {
            var sd = staffData[i];
            var personal = new Tbl_Personal
            {
                Tc = Guid.NewGuid(),
                FirstName = sd.FName,
                LastName = sd.LName,
                Specialty = sd.Specialty,
                Line = sd.Line,
                PersonalCode = $"EMP-{(i + 1):D3}",
                Mobile = sd.Mobile,
                Avatar = sd.Avatar,
                Bio = sd.Bio,
                ExperienceYears = sd.Exp,
                IsApproved = true,
                IsActive = true,
                RegisterData = now,
                salon_Tc = SalonTc,
                BirthDate = new DateTime(1993 + i, 5, 12),
                WorkingDays = "شنبه تا پنج‌شنبه",
                WorkingHours = "۰۹:۰۰ الی ۱۸:۰۰"
            };
            Add(personal);
            Add(new Tbl_Mobile { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, MobileNumber = sd.Mobile, TcPersonal = personal.Tc.ToString() });
            Add(new Tbl_UserRole { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, PersonalTC = personal.Tc, RoleTC = roles.First(r => r.RoleName == "Personnel").Tc });
        }

        foreach (var c in new[] { "نگار رضایی", "مریم احمدی", "سحر کریمی", "مهدیه صادقی" })
            Add(new Tbl_Customer { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, FullName = c, CodeMoaref = "HB" + Random.Shared.Next(1000, 9999) });

        var portfolioCats = new[] { "نمونه کار ناخن", "نمونه کار مو", "نمونه کار میکاپ" };
        var pcs = new List<Tbl_CategoryPortfolio>();
        for (int i = 0; i < portfolioCats.Length; i++)
        {
            var pc = new Tbl_CategoryPortfolio
            {
                Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, SalonTC = SalonTc,
                CategoryNameSample = portfolioCats[i], SamplePic = $"img/gallery-{i + 1}.jpg"
            };
            pcs.Add(pc); Add(pc);
        }

        for (int i = 0; i < 6; i++)
            Add(new Tbl_Portfoilo
            {
                Tc = Guid.NewGuid(), IsActive = true, RegisterData = now,
                SampleTC = pcs[i % pcs.Count].Tc,
                Title = $"نمونه کار شماره {i + 1}",
                Description = "کار انجام شده در سالن زیبایی حدیث",
                ProtfiloPic = $"img/gallery-{(i % 3) + 1}.jpg"
            });

        Add(new Tbl_Social { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, TC_Salon = SalonTc, SocialLink = "https://instagram.com/hadisbeauty.salon", Pics = "instagram" });
        Add(new Tbl_Social { Tc = Guid.NewGuid(), IsActive = true, RegisterData = now, TC_Salon = SalonTc, SocialLink = "https://t.me/hadisbeauty", Pics = "telegram" });

        var firstPersonal = All<Tbl_Personal>().First();
        var firstCustomer = All<Tbl_Customer>().First();
        var nailService = allServices.First(s => s.ServiceName == "کاشت ناخن");

        Add(new Tbl_Reservation
        {
            Tc = Guid.NewGuid(),
            TrackingCode = "TC-260924-0001",
            ReservationType = ReservationType.Online,
            IsActive = true, RegisterData = now.AddDays(-2), SalonTC = SalonTc,
            CustomerTC = firstCustomer.Tc, CustomerName = firstCustomer.FullName, MobileNumber = "09120000003",
            ServiceTC = nailService.Tc, ServiceName = nailService.ServiceName,
            PersonalTC = firstPersonal.Tc, PersonalName = "سارا محمدی",
            ReserveDate = now.Date.AddDays(2), ReserveTime = new TimeSpan(11, 0, 0),
            DurationMinutes = 150, Price = nailService.Price, PayableAmount = nailService.Price,
            PaidAmount = nailService.Price, PaymentStatus = PaymentStatus.Paid,
            VariablePriceAgreed = true,
            Status = ReservationStatus.Confirmed, ConfirmDate = now
        });

        Add(new Tbl_Reservation
        {
            Tc = Guid.NewGuid(),
            TrackingCode = "TC-260925-0002",
            ReservationType = ReservationType.InPerson,
            IsActive = true, RegisterData = now.AddDays(-1), SalonTC = SalonTc,
            CustomerTC = firstCustomer.Tc, CustomerName = "سحر کریمی", MobileNumber = "09123334455",
            ServiceTC = allServices.First(s => s.ServiceName == "میکاپ عروس").Tc, ServiceName = "میکاپ عروس",
            PersonalTC = firstPersonal.Tc, PersonalName = "حدیث رحیمی",
            ReserveDate = now.Date.AddDays(5), ReserveTime = new TimeSpan(10, 0, 0),
            DurationMinutes = 240, Price = 7_500_000, PayableAmount = 7_500_000,
            PaidAmount = 2_000_000, PaymentStatus = PaymentStatus.Paid, PaymentMethod = PaymentMethod.CardReader,
            RegisteredBy = "ادمین سالن", VariablePriceAgreed = true,
            Status = ReservationStatus.Confirmed, ConfirmDate = now
        });

        Add(new Tbl_Reservation
        {
            Tc = Guid.NewGuid(),
            TrackingCode = "TC-260925-0003",
            ReservationType = ReservationType.Online,
            IsActive = true, RegisterData = now, SalonTC = SalonTc,
            CustomerTC = firstCustomer.Tc, CustomerName = "مریم احمدی", MobileNumber = "09124445566",
            ServiceTC = allServices.First(s => s.ServiceName == "فیشال پوست").Tc, ServiceName = "فیشال پوست",
            PersonalTC = firstPersonal.Tc, PersonalName = "الهام نوری",
            ReserveDate = now.Date.AddDays(1), ReserveTime = new TimeSpan(17, 30, 0),
            DurationMinutes = 75, Price = 1_400_000, PayableAmount = 1_400_000,
            CustomerNote = "پوست حساس", Status = ReservationStatus.PendingPayment,
            VariablePriceAgreed = true
        });

        // کدهای تخفیف (بند ۸ سند)
        Add(new Tbl_Discount { Tc = Guid.NewGuid(), Code = "HADIS20", Title = "تخفیف ۲۰٪ افتتاحیه آنلاین", Percent = 20, MaxDiscount = 300_000, IsActive = true, RegisterData = now });
        Add(new Tbl_Discount { Tc = Guid.NewGuid(), Code = "BEAUTY10", Title = "تخفیف ۱۰٪ مشتریان وفادار", Percent = 10, MaxDiscount = 150_000, IsActive = true, RegisterData = now });

        // نظرات مشتریان (بند ۲ سند)
        Add(new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "زهرا کمالی", ServiceName = "کاشت ناخن و مانیکور", Rating = 5, CommentText = "محیط بسیار آرام و لوکس، دقت و بهداشت سارا جان واقعاً عالی بود. به همه پیشنهاد می‌کنم.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-2) });
        Add(new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "مهسا تهرانی", ServiceName = "میکاپ و شنیون عروس", Rating = 5, CommentText = "حدیث جون هنر دستتون بی‌نظیره! آرایش من دقیقاً همون چیزی شد که می‌خواستم و تا آخر مراسم تکون نخورد.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-5) });
        Add(new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "پریسا راد", ServiceName = "فیشال و پاکسازی پوست", Rating = 5, CommentText = "پوستم واقعاً شفاف و درخشان شد. ماساژ صورت و محصولات الهام خانم حس سبکی فوق‌العاده‌ای داشت.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-7) });
        Add(new Tbl_Comment { Tc = Guid.NewGuid(), CustomerName = "ساناز امینی", ServiceName = "بالیاژ مو", Rating = 5, CommentText = "بدون ذره‌ای سوختگی یا آسیب، رنگ موهام دقیقاً رنگی شد که می‌خواستم. مریم جان کارشون بیستِ.", IsApproved = true, IsActive = true, RegisterData = now.AddDays(-10) });

        // سوالات متداول (بند ۲ سند)
        Add(new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "نوبت‌دهی و رزرو", Question = "چگونه می‌توانم نوبت خود را آنلاین ثبت کنم؟", Answer = "کافی است از بخش رزرو نوبت، خدمت و پرسنل موردنظر را انتخاب کرده، تاریخ شمسی و ساعت دلخواه را مشخص کنید و رزرو را نهایی فرمایید.", OrderIndex = 1, IsPublished = true, IsActive = true, RegisterData = now });
        Add(new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "نوبت‌دهی و رزرو", Question = "آیا در صورت پر بودن وقت‌ها امکان ثبت در لیست انتظار هست؟", Answer = "بله، در صورت تکمیل بودن ظرفیت روز، گزینه «مرا در لیست انتظار قرار بده» فعال می‌شود تا در صورت لغو سایر مراجعین، فوراً پیامک دریافت کنید.", OrderIndex = 2, IsPublished = true, IsActive = true, RegisterData = now });
        Add(new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "خدمات و مواد مصرفی", Question = "آیا مواد مصرفی سالن اصل و اورجینال هستند؟", Answer = "تمامی مواد رنگ، کراتین، مواد فیشال و کاشت ناخن مستقیماً از برندهای معتبر بین‌المللی با گواهی اصالت تهیه می‌شوند.", OrderIndex = 3, IsPublished = true, IsActive = true, RegisterData = now });
        Add(new Tbl_FAQ { Tc = Guid.NewGuid(), Category = "بهداشت و استانداردهای سالن", Question = "نحوه استریل ابزارهای کار به چه صورت است؟", Answer = "تمام ابزارهای فلزی در دستگاه اتوکلاو پزشکی استریل شده و سوهان‌ها و اقلام مصرفی به صورت کاملاً یک‌بارمصرف استفاده می‌شوند.", OrderIndex = 4, IsPublished = true, IsActive = true, RegisterData = now });

        // اخبار و مقالات (بند ۲۱ سند)
        Add(new Tbl_NewsDay { Tc = Guid.NewGuid(), Title = "راهنمای جامع مراقبت از موهای رنگ و دکلره شده", TextNews = "موهای دکلره شده به دلیل باز شدن کوتیکول‌ها نیازمند رطوبت‌رسانی عمیق و استفاده از ماسک‌های حاوی پروتئین و روغن‌های گیاهی هستند...", Author = "تیم تخصصی حدیث بیوتی", Image = "img/work-hair.jpg", IsPublished = true, IsActive = true, RegisterData = now.AddDays(-3), DateStart = now.AddDays(-3), DateEnd = now.AddYears(1) });
        Add(new Tbl_NewsDay { Tc = Guid.NewGuid(), Title = "ترندهای آرایش و میکاپ عروس در سال ۲۰۲۶", TextNews = "سبک آرایش سافت گلم و پوست شفاف شیشه‌ای با درخشش طبیعی، محبوب‌ترین ترند میکاپ عروس امسال است...", Author = "حدیث رحیمی", Image = "img/work-bridal.jpg", IsPublished = true, IsActive = true, RegisterData = now.AddDays(-6), DateStart = now.AddDays(-6), DateEnd = now.AddYears(1) });
        Add(new Tbl_NewsDay { Tc = Guid.NewGuid(), Title = "مراحل فیشال کلاسیک و تأثیر آن بر جوانسازی", TextNews = "فیشال منظم ماهانه با حذف سلول‌های مرده و پاکسازی منافذ، کلاژن‌سازی طبیعی پوست را تحریک کرده و مانع از چین و چروک زودرس می‌شود...", Author = "الهام نوری", Image = "img/gallery-3.jpg", IsPublished = true, IsActive = true, RegisterData = now.AddDays(-10), DateStart = now.AddDays(-10), DateEnd = now.AddYears(1) });

        // اعلانات (بند ۱۷ سند)
        Add(new Tbl_Notification { Tc = Guid.NewGuid(), TargetRole = "Admin", Title = "رزرو جدید ثبت شد", Message = "رزرو جدید برای خدمت «کاشت ناخن» توسط نگار رضایی (TC-260924-0001)", IsRead = false, CreatedAt = now.AddMinutes(-30), IsActive = true, RegisterData = now.AddMinutes(-30) });
    }
}
