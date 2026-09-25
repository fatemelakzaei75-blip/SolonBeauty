namespace BlazorAppSolon;

/// <summary>یک آیتم خدمت سالن (آیکون خطی مینیمال + عنوان + لینک)</summary>
public record ServiceItem(string Title, string Icon, string Url);

/// <summary>فهرست خدمات سالن حدیث — بند ۶ بریف طراحی</summary>
public static class ServiceCatalog
{
    public static readonly ServiceItem[] All =
    {
        new("عروس و میکاپ", "bride", "services/bride"),
        new("میکاپ", "makeup", "services/bride"),
        new("شنیون", "shinion", "services/bride"),
        new("رنگ و لایت", "color", "services/hair"),
        new("کراتین و احیا", "keratin", "services/hair"),
        new("کوپ", "cut", "services/hair"),
        new("اکستنشن مژه", "lash", "services/lash"),
        new("لیفت", "lift", "services/lash"),
        new("لمینت", "laminate", "services/lash"),
        new("ماساژ", "massage", "services/skin"),
        new("بافت مو", "hairtexture", "services/hair"),
        new("اکستنشن مو", "hairext", "services/hair"),
        new("براشینگ", "brushing", "services/hair"),
        new("فیشال پوست", "facial", "services/skin"),
        new("آرایش دائم", "permanent", "services/skin"),
        new("ناخن", "nail", "services/nail"),
    };
}
