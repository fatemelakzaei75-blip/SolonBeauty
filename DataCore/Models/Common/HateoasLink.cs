namespace DataCore.Models.Common;

/// <summary>
/// مدل پیوند استاندارد برای پیاده‌سازی اصول HATEOAS در پاسخ‌های API
/// </summary>
public class HateoasLink
{
    public string Href { get; set; } = string.Empty;

    public string Rel { get; set; } = string.Empty;

    public string Method { get; set; } = "GET";

    public HateoasLink()
    {
    }

    public HateoasLink(string href, string rel, string method = "GET")
    {
        Href = href;
        Rel = rel;
        Method = method;
    }
}
