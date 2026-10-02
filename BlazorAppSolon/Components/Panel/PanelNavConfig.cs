using BlazorAppSolon.Auth;

namespace BlazorAppSolon.Components.Panel;

/// <summary>
/// یک آیتم منوی پنل. تمام ناوبری پنل (Sidebar، Bottom Nav و Drawer موبایل)
/// از همین آرایه تغذیه می‌شود تا آدرس‌ها در چند جا تکرار نشوند.
/// </summary>
public sealed record PanelNavItem
{
    /// <summary>آدرس مقصد نسبت به ریشه سایت</summary>
    public required string Href { get; init; }

    /// <summary>عنوان کامل (در Sidebar باز و Drawer)</summary>
    public required string Label { get; init; }

    /// <summary>عنوان کوتاه برای Bottom Navigation (اختیاری؛ در نبود، Label استفاده می‌شود)</summary>
    public string? ShortLabel { get; init; }

    /// <summary>کلید آیکون در <see cref="PanelNavIcons"/> — دلیل جدا بودن: آیکون خطی SVG جای ایموجی</summary>
    public required string Icon { get; init; }

    /// <summary>
    /// اگر true باشد فقط تطابق دقیق آدرس، آیتم را فعال می‌کند.
    /// برای صفحه‌های خانه‌ی هر نقش لازم است؛ وگرنه تمام زیرصفحه‌ها آن را فعال می‌کنند.
    /// </summary>
    public bool Exact { get; init; }

    /// <summary>کلید شمارش اعلان برای نمایش Badge — فعلاً فقط اعلانات مقدار می‌گیرد</summary>
    public string? BadgeKey { get; init; }

    /// <summary>آیا در Bottom Navigation موبایل نمایش داده شود؟</summary>
    public bool InBottomNav { get; init; }

    public string BottomLabel => ShortLabel ?? Label;
}

/// <summary>یک گروه تاشو (Accordion) از آیتم‌های منو</summary>
public sealed record PanelNavGroup
{
    public required string Title { get; init; }

    /// <summary>کلید پایدار برای شناسه‌ی DOM و ذخیره‌ی وضعیت باز/بسته‌ی گروه</summary>
    public required string Key { get; init; }

    public required IReadOnlyList<PanelNavItem> Items { get; init; }
}

/// <summary>
/// منبع حقیقت واحد منوی پنل: آیتم‌ها، آیکون‌ها، گروه‌ها و دسترسی هر نقش.
/// افزودن یک صفحه‌ی جدید فقط با اضافه‌کردن یک رکورد به همین فایل انجام می‌شود.
/// </summary>
public static class PanelNavConfig
{
    // ---------- آیتم‌های مشترک ----------
    private static readonly PanelNavItem AdminDashboard = new()
    {
        Href = "panel/admin", Label = "داشبورد و شاخص‌ها", ShortLabel = "شاخص‌ها",
        Icon = "dashboard", Exact = true, InBottomNav = true
    };

    private static readonly PanelNavItem PersonnelDashboard = new()
    {
        Href = "panel/personnel", Label = "داشبورد کاری", ShortLabel = "داشبورد",
        Icon = "dashboard", Exact = true, InBottomNav = true
    };

    private static readonly PanelNavItem CustomerDashboard = new()
    {
        Href = "panel/customer", Label = "خلاصه وضعیت", ShortLabel = "داشبورد",
        Icon = "dashboard", Exact = true, InBottomNav = true
    };

    // ---------- منوی ادمین: ۱۲ آیتم در ۴ گروه ----------
    private static readonly IReadOnlyList<PanelNavGroup> AdminGroups = new[]
    {
        new PanelNavGroup
        {
            Key = "overview",
            Title = "مدیریت کلان سالن",
            Items = new[]
            {
                AdminDashboard,
                new PanelNavItem
                {
                    Href = "panel/admin/base-data", Label = "اطلاعات پایه", ShortLabel = "پایه‌ها",
                    Icon = "layers", InBottomNav = true
                }
            }
        },
        new PanelNavGroup
        {
            Key = "bookings",
            Title = "نوبت‌دهی و مراجعین",
            Items = new[]
            {
                new PanelNavItem
                {
                    Href = "panel/admin/data/reservation", Label = "مدیریت نوبت‌ها", ShortLabel = "نوبت‌ها",
                    Icon = "calendar", InBottomNav = true
                },
                new PanelNavItem
                {
                    Href = "panel/admin/data/waiting-list", Label = "لیست انتظار", Icon = "hourglass"
                },
                new PanelNavItem
                {
                    Href = "panel/admin/data/customer", Label = "پرونده مشتریان", Icon = "users"
                }
            }
        },
        new PanelNavGroup
        {
            Key = "team",
            Title = "تیم و خدمات",
            Items = new[]
            {
                new PanelNavItem
                {
                    Href = "panel/admin/data/personal", Label = "پرسنل سالن", Icon = "userCheck"
                },
                new PanelNavItem
                {
                    Href = "panel/admin/data/salon-service", Label = "خدمات و تعرفه‌ها", Icon = "sparkles"
                },
                new PanelNavItem
                {
                    Href = "panel/admin/data/portfolio", Label = "گالری نمونه‌کارها", Icon = "image"
                }
            }
        },
        new PanelNavGroup
        {
            Key = "settings",
            Title = "تنظیمات و تعاملات",
            Items = new[]
            {
                new PanelNavItem
                {
                    Href = "panel/admin/data/salon-setting", Label = "تنظیمات سالن و تم", Icon = "settings"
                },
                new PanelNavItem
                {
                    Href = "panel/admin/data/discount", Label = "کدهای تخفیف", Icon = "tag"
                },
                new PanelNavItem
                {
                    Href = "panel/admin/data/news-day", Label = "اخبار و وبلاگ", Icon = "megaphone"
                },
                new PanelNavItem
                {
                    Href = "panel/admin/data/notification", Label = "اعلانات سیستم", ShortLabel = "اعلانات",
                    Icon = "bell", BadgeKey = "notifications", InBottomNav = true
                }
            }
        }
    };

    // ---------- منوی پرسنل: ۴ آیتم ----------
    private static readonly IReadOnlyList<PanelNavGroup> PersonnelGroups = new[]
    {
        new PanelNavGroup
        {
            Key = "work",
            Title = "کارتابل پرسنل",
            Items = new[]
            {
                PersonnelDashboard,
                new PanelNavItem
                {
                    Href = "panel/personnel/schedule", Label = "کارتابل و نوبت‌ها", ShortLabel = "نوبت‌ها",
                    Icon = "calendar", InBottomNav = true
                },
                new PanelNavItem
                {
                    Href = "panel/personnel/portfolio", Label = "نمونه‌کارهای من", ShortLabel = "نمونه‌کار",
                    Icon = "image", InBottomNav = true
                },
                new PanelNavItem
                {
                    Href = "panel/personnel/profile", Label = "مشخصات و ساعت کاری", ShortLabel = "پروفایل",
                    Icon = "user", InBottomNav = true
                }
            }
        }
    };

    // ---------- منوی مشتری: ۴ آیتم ----------
    private static readonly IReadOnlyList<PanelNavGroup> CustomerGroups = new[]
    {
        new PanelNavGroup
        {
            Key = "account",
            Title = "حساب کاربری من",
            Items = new[]
            {
                CustomerDashboard,
                new PanelNavItem
                {
                    Href = "panel/customer/appointments", Label = "نوبت‌های رزرو شده", ShortLabel = "نوبت‌های من",
                    Icon = "calendar", InBottomNav = true
                },
                new PanelNavItem
                {
                    Href = "panel/customer/favorites", Label = "علاقه‌مندی‌ها", ShortLabel = "علاقه‌ها",
                    Icon = "heart", BadgeKey = "favorites", InBottomNav = true
                },
                new PanelNavItem
                {
                    Href = "panel/customer/profile", Label = "اطلاعات حساب", ShortLabel = "حساب",
                    Icon = "user", InBottomNav = true
                }
            }
        }
    };

    /// <summary>گروه‌های منو بر اساس نقش کاربر — آیتمی خارج از این آرایه در UI ساخته نمی‌شود</summary>
    public static IReadOnlyList<PanelNavGroup> GroupsFor(string? role) => role switch
    {
        AppRoles.Admin => AdminGroups,
        AppRoles.Personnel => PersonnelGroups,
        AppRoles.Customer => CustomerGroups,
        _ => Array.Empty<PanelNavGroup>()
    };

    /// <summary>آیتم‌های Bottom Navigation (حداکثر ۴ مورد + دکمه منو)</summary>
    public static IReadOnlyList<PanelNavItem> BottomNavFor(string? role) =>
        GroupsFor(role)
            .SelectMany(g => g.Items)
            .Where(i => i.InBottomNav)
            .ToList();

    /// <summary>جست‌وجوی یک گروه بر اساس کلید پایدار آن</summary>
    public static PanelNavGroup? FindGroup(string? role, string key) =>
        GroupsFor(role).FirstOrDefault(g => g.Key == key);
}
