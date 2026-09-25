using System;
using System.Collections.Generic;

namespace DataCore.Models.Seo;

/// <summary>
/// مدل جامع متادیتای سئو برای صفحات وب
/// شامل تگ‌های متا، Open Graph، توییتر، تگ کنونیکال و داده‌های ساختاریافته Schema.org
/// </summary>
public class SeoMetadata
{
    public string Title { get; set; } = "سالن زیبایی حدیث | Hadis Beauty Salon";

    public string Description { get; set; } = "سالن زیبایی حدیث ارائه دهنده خدمات لوکس و تخصصی زیبایی، میکاپ عروس، سلامت پوست، مو و ناخن در محیطی آرام و مدرن.";

    public string Keywords { get; set; } = "سالن زیبایی, آرایشگاه زنانه, کاشت ناخن, رنگ و لایت, میکاپ عروس, فیشال پوست, سالن زیبایی حدیث";

    public string CanonicalUrl { get; set; } = string.Empty;

    public string Robots { get; set; } = "index, follow, max-image-preview:large, max-snippet:-1";

    public string OgTitle { get; set; } = string.Empty;

    public string OgDescription { get; set; } = string.Empty;

    public string OgUrl { get; set; } = string.Empty;

    public string OgImage { get; set; } = "/img/hero.jpg";

    public string OgType { get; set; } = "website";

    public string SiteName { get; set; } = "سالن زیبایی حدیث";

    public string Locale { get; set; } = "fa_IR";

    public string TwitterCard { get; set; } = "summary_large_image";

    public string? JsonLd { get; set; }

    public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();

    public static SeoMetadata CreateNoIndex(string title)
    {
        return new SeoMetadata
        {
            Title = $"{title} | سالن زیبایی حدیث",
            Robots = "noindex, nofollow",
            Description = string.Empty
        };
    }
}

public class BreadcrumbItem
{
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int Position { get; set; }

    public BreadcrumbItem()
    {
    }

    public BreadcrumbItem(string title, string url, int position)
    {
        Title = title;
        Url = url;
        Position = position;
    }
}
