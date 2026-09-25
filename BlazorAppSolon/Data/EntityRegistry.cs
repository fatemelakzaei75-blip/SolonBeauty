using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.Entity;

namespace BlazorAppSolon.Data;

/// <summary>توصیف یک موجودیت برای منوی پنل ادمین و مسیر API آن</summary>
public record EntityDescriptor(string Key, string Title, string Group, Type EntityType, string Icon, string ApiRoute);

/// <summary>فهرست تفکیک‌شده موجودیت‌های سامانه (بند ۱۹ سند — تفکیک اطلاعات پایه از داشبورد اصلی)</summary>
public static class EntityRegistry
{
    public static readonly EntityDescriptor[] All =
    {
        // اطلاعات پایه (بند ۱۹)
        new("salon",              "مشخصات پایه سالن",  "اطلاعات پایه", typeof(Tbl_Salon),             "🏠", "Salon"),
        new("category",           "دسته‌بندی خدمات",   "اطلاعات پایه", typeof(Tbl_Category),          "🗂", "Category"),
        new("salon-service",      "فهرست خدمات سالن",  "اطلاعات پایه", typeof(Tbl_SalonSerice),       "💅", "SalonService"),
        new("social",             "شبکه‌های اجتماعی",  "اطلاعات پایه", typeof(Tbl_Social),            "🔗", "Social"),
        new("role",               "نقش‌های کاربری",    "اطلاعات پایه", typeof(Tbl_Roles),             "🎫", "Role"),
        new("permission",         "سطوح دسترسی",       "اطلاعات پایه", typeof(Tbl_Permission),        "🔑", "Permission"),
        new("role-permission",    "تخصیص دسترسی",     "اطلاعات پایه", typeof(Tbl_RolePermission),    "🧩", "RolePermission"),
        new("user-role",          "نقش کاربران",       "اطلاعات پایه", typeof(Tbl_UserRole),          "🧑‍💼", "UserRole"),
        new("mobile",             "شماره‌های موبایل",  "اطلاعات پایه", typeof(Tbl_Mobile),            "📱", "Mobile"),
        new("confirm",            "کدهای تأیید OTP",   "اطلاعات پایه", typeof(Tbl_Confirm),           "🔐", "Confirm"),

        // مدیریت نوبت‌ها و مراجعین
        new("reservation",        "مدیریت نوبت‌ها",    "نوبت‌ها",      typeof(Tbl_Reservation),       "🗓", "Reservation"),
        new("waiting-list",       "لیست انتظار",       "نوبت‌ها",      typeof(Tbl_WaitingList),       "⏳", "WaitingList"),
        new("customer",           "پرونده مشتریان",   "مشتریان",      typeof(Tbl_Customer),          "👥", "Customer"),

        // پرسنل و نمونه‌کارها
        new("personal",           "اطلاعات پرسنل",     "پرسنل",        typeof(Tbl_Personal),          "👩‍🎨", "Personal"),
        new("category-portfolio", "دسته‌بندی نمونه‌کار","نمونه کار",   typeof(Tbl_CategoryPortfolio), "📁", "CategoryPortfolio"),
        new("portfolio",          "گالری نمونه‌کارها", "نمونه کار",   typeof(Tbl_Portfoilo),         "🖼", "Portfolio"),
        new("like",               "پسندها",           "نمونه کار",   typeof(Tbl_Like),              "❤️", "Like"),

        // بازاریابی، محتوا و تنظیمات
        new("discount",           "کدهای تخفیف",      "مالی و تخفیف",typeof(Tbl_Discount),         "🏷️", "Discount"),
        new("news-day",           "اخبار و مقالات",    "محتوا",        typeof(Tbl_NewsDay),           "📣", "NewsDay"),
        new("comment",            "نظرات مشتریان",    "محتوا",        typeof(Tbl_Comment),           "💬", "Comment"),
        new("faq",                "پرسش‌های متداول",  "محتوا",        typeof(Tbl_FAQ),               "❓", "FAQ"),
        new("notification",       "اعلانات سیستم",    "ارتباطات",     typeof(Tbl_Notification),      "🔔", "Notification"),
        new("salon-setting",      "تنظیمات سالن و تم", "تنظیمات",      typeof(Tbl_SalonSetting),      "⚙️", "SalonSetting"),
    };

    public static EntityDescriptor? ByKey(string key)
        => All.FirstOrDefault(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

    public static string ApiRouteFor(Type entityType)
        => All.FirstOrDefault(e => e.EntityType == entityType)?.ApiRoute ?? entityType.Name;

    public static IEnumerable<IGrouping<string, EntityDescriptor>> Grouped() => All.GroupBy(e => e.Group);
}
