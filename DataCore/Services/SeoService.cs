using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using DataCore.Interfaces;
using DataCore.Models.Seo;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس سئو جهت تولید استانداردهای سئو تکنیکال، On-Page و داده‌های ساختاریافته Schema.org
/// </summary>
public class SeoService : ISeoService
{
    private const string DefaultSiteName = "سالن زیبایی حدیث";
    private const string DefaultBaseUrl = "https://hadisbeauty.salon";

    public SeoMetadata GetHomeSeo(Tbl_SalonSetting? settings, string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var salonName = settings?.SalonName ?? DefaultSiteName;
        var subtitle = settings?.HeroSubtitle ?? "تجربه‌ای آرام، لوکس و حرفه‌ای از خدمات زیبایی";
        var description = settings?.IntroText ?? "سالن زیبایی حدیث با بهره‌گیری از کادری مجرب، ارائه‌دهنده تخصصی‌ترین خدمات کاشت ناخن، رنگ و لایت، میکاپ عروس، فیشال و کراتین مو است.";

        var homeUrl = $"{baseUrl}/";
        var schemaJson = BuildLocalBusinessSchema(settings, baseUrl);

        return new SeoMetadata
        {
            Title = $"{salonName} | {subtitle}",
            Description = description,
            CanonicalUrl = homeUrl,
            OgTitle = $"{salonName} | خدمات تخصصی زیبایی و آرایشی",
            OgDescription = description,
            OgUrl = homeUrl,
            OgImage = $"{baseUrl}/img/hero.jpg",
            OgType = "website",
            Robots = "index, follow, max-image-preview:large, max-snippet:-1",
            JsonLd = schemaJson,
            Breadcrumbs = new List<BreadcrumbItem>
            {
                new("صفحه اصلی", homeUrl, 1)
            }
        };
    }

    public SeoMetadata GetServiceSeo(string slug, string title, string description, decimal price, string? image, string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var url = $"{baseUrl}/services/{slug}";
        var imgUrl = string.IsNullOrWhiteSpace(image) ? $"{baseUrl}/img/hero.jpg" : (image.StartsWith("http") ? image : $"{baseUrl}/{image.TrimStart('/')}");

        var breadcrumbs = new List<BreadcrumbItem>
        {
            new("صفحه اصلی", $"{baseUrl}/", 1),
            new("خدمات زیبایی", $"{baseUrl}/services", 2),
            new(title, url, 3)
        };

        var serviceSchema = BuildServiceSchema(title, description, price, imgUrl, url, baseUrl);
        var breadcrumbSchema = BuildBreadcrumbSchema(breadcrumbs, baseUrl);

        var combinedSchema = $"[{serviceSchema},{breadcrumbSchema}]";

        return new SeoMetadata
        {
            Title = $"{title} | خدمات تخصصی سالن زیبایی حدیث",
            Description = description,
            CanonicalUrl = url,
            OgTitle = $"{title} | سالن زیبایی حدیث",
            OgDescription = description,
            OgUrl = url,
            OgImage = imgUrl,
            OgType = "service",
            Robots = "index, follow, max-image-preview:large",
            JsonLd = combinedSchema,
            Breadcrumbs = breadcrumbs
        };
    }

    public SeoMetadata GetBlogPostSeo(string slug, string title, string summary, string author, DateTime publishDate, string image, string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var url = $"{baseUrl}/blog/{slug}";
        var imgUrl = string.IsNullOrWhiteSpace(image) ? $"{baseUrl}/img/hero.jpg" : (image.StartsWith("http") ? image : $"{baseUrl}/{image.TrimStart('/')}");

        var breadcrumbs = new List<BreadcrumbItem>
        {
            new("صفحه اصلی", $"{baseUrl}/", 1),
            new("وبلاگ و مقالات", $"{baseUrl}/blog", 2),
            new(title, url, 3)
        };

        var articleSchema = BuildArticleSchema(title, summary, author, publishDate, imgUrl, url, baseUrl);
        var breadcrumbSchema = BuildBreadcrumbSchema(breadcrumbs, baseUrl);
        var combinedSchema = $"[{articleSchema},{breadcrumbSchema}]";

        return new SeoMetadata
        {
            Title = $"{title} | وبلاگ سالن زیبایی حدیث",
            Description = summary,
            CanonicalUrl = url,
            OgTitle = title,
            OgDescription = summary,
            OgUrl = url,
            OgImage = imgUrl,
            OgType = "article",
            Robots = "index, follow, max-image-preview:large",
            JsonLd = combinedSchema,
            Breadcrumbs = breadcrumbs
        };
    }

    public SeoMetadata GetGeneralPageSeo(string title, string description, string relativePath, string baseUrl, IEnumerable<BreadcrumbItem>? breadcrumbs = null)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var cleanPath = relativePath.TrimStart('/');
        var url = string.IsNullOrEmpty(cleanPath) ? $"{baseUrl}/" : $"{baseUrl}/{cleanPath}";

        var breadcrumbList = breadcrumbs?.ToList() ?? new List<BreadcrumbItem>
        {
            new("صفحه اصلی", $"{baseUrl}/", 1),
            new(title, url, 2)
        };

        var breadcrumbSchema = BuildBreadcrumbSchema(breadcrumbList, baseUrl);

        return new SeoMetadata
        {
            Title = $"{title} | سالن زیبایی حدیث",
            Description = description,
            CanonicalUrl = url,
            OgTitle = $"{title} | سالن زیبایی حدیث",
            OgDescription = description,
            OgUrl = url,
            OgImage = $"{baseUrl}/img/hero.jpg",
            OgType = "website",
            Robots = "index, follow",
            JsonLd = breadcrumbSchema,
            Breadcrumbs = breadcrumbList
        };
    }

    public SeoMetadata GetNoIndexSeo(string title)
    {
        return SeoMetadata.CreateNoIndex(title);
    }

    public string BuildLocalBusinessSchema(Tbl_SalonSetting? settings, string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var salonName = settings?.SalonName ?? DefaultSiteName;
        var phone = settings?.PhoneLandline ?? "021-12345678";
        var mobile = settings?.PhoneMobile ?? "09120000001";
        var address = settings?.Address ?? "تهران، زعفرانیه، خیابان مقدس اردبیلی، پلاک ۲۴";

        var schemaObj = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BeautySalon",
            ["@id"] = $"{baseUrl}/#salon",
            ["name"] = salonName,
            ["alternateName"] = settings?.SalonLatinName ?? "Hadis Beauty Lounge",
            ["url"] = $"{baseUrl}/",
            ["logo"] = $"{baseUrl}/icon-512.png",
            ["image"] = $"{baseUrl}/img/hero.jpg",
            ["telephone"] = phone,
            ["priceRange"] = "$$",
            ["address"] = new Dictionary<string, string>
            {
                ["@type"] = "PostalAddress",
                ["streetAddress"] = address,
                ["addressLocality"] = "تهران",
                ["addressRegion"] = "تهران",
                ["addressCountry"] = "IR"
            },
            ["geo"] = new Dictionary<string, object>
            {
                ["@type"] = "GeoCoordinates",
                ["latitude"] = 35.8041,
                ["longitude"] = 51.4172
            },
            ["openingHoursSpecification"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["@type"] = "OpeningHoursSpecification",
                    ["dayOfWeek"] = new[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" },
                    ["opens"] = "09:00",
                    ["closes"] = "21:00"
                }
            },
            ["sameAs"] = new[]
            {
                settings?.InstagramUrl ?? "https://instagram.com/hadisbeauty.salon",
                settings?.TelegramUrl ?? "https://t.me/hadisbeauty"
            }
        };

        return JsonSerializer.Serialize(schemaObj);
    }

    public string BuildServiceSchema(string title, string description, decimal price, string? image, string url, string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var schemaObj = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Service",
            ["serviceType"] = "خدمات زیبایی و آرایشی",
            ["name"] = title,
            ["description"] = description,
            ["url"] = url,
            ["provider"] = new Dictionary<string, string>
            {
                ["@type"] = "BeautySalon",
                ["name"] = DefaultSiteName,
                ["url"] = $"{baseUrl}/"
            },
            ["offers"] = new Dictionary<string, object>
            {
                ["@type"] = "Offer",
                ["price"] = price,
                ["priceCurrency"] = "IRR",
                ["availability"] = "https://schema.org/InStock"
            }
        };

        if (!string.IsNullOrWhiteSpace(image))
        {
            schemaObj["image"] = image;
        }

        return JsonSerializer.Serialize(schemaObj);
    }

    public string BuildArticleSchema(string title, string summary, string author, DateTime publishDate, string image, string url, string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var schemaObj = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Article",
            ["headline"] = title,
            ["description"] = summary,
            ["image"] = image,
            ["datePublished"] = publishDate.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            ["dateModified"] = publishDate.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            ["mainEntityOfPage"] = url,
            ["author"] = new Dictionary<string, string>
            {
                ["@type"] = "Person",
                ["name"] = string.IsNullOrWhiteSpace(author) ? DefaultSiteName : author
            },
            ["publisher"] = new Dictionary<string, object>
            {
                ["@type"] = "Organization",
                ["name"] = DefaultSiteName,
                ["logo"] = new Dictionary<string, string>
                {
                    ["@type"] = "ImageObject",
                    ["url"] = $"{baseUrl}/icon-512.png"
                }
            }
        };

        return JsonSerializer.Serialize(schemaObj);
    }

    public string BuildFaqSchema(IEnumerable<(string Question, string Answer)> faqs)
    {
        var faqList = faqs.Select(f => new Dictionary<string, object>
        {
            ["@type"] = "Question",
            ["name"] = f.Question,
            ["acceptedAnswer"] = new Dictionary<string, string>
            {
                ["@type"] = "Answer",
                ["text"] = f.Answer
            }
        }).ToArray();

        var schemaObj = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "FAQPage",
            ["mainEntity"] = faqList
        };

        return JsonSerializer.Serialize(schemaObj);
    }

    public string BuildBreadcrumbSchema(IEnumerable<BreadcrumbItem> breadcrumbs, string baseUrl)
    {
        var items = breadcrumbs.Select(b => new Dictionary<string, object>
        {
            ["@type"] = "ListItem",
            ["position"] = b.Position,
            ["name"] = b.Title,
            ["item"] = b.Url
        }).ToArray();

        var schemaObj = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = items
        };

        return JsonSerializer.Serialize(schemaObj);
    }

    public string GenerateSitemapXml(string baseUrl, IEnumerable<string> serviceSlugs, IEnumerable<string> blogSlugs)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // صفحات ثابت عمومی
        AddUrl(sb, $"{baseUrl}/", "daily", "1.0");
        AddUrl(sb, $"{baseUrl}/services", "weekly", "0.9");
        AddUrl(sb, $"{baseUrl}/gallery", "weekly", "0.8");
        AddUrl(sb, $"{baseUrl}/reserve", "daily", "0.8");
        AddUrl(sb, $"{baseUrl}/blog", "daily", "0.8");
        AddUrl(sb, $"{baseUrl}/about", "monthly", "0.6");
        AddUrl(sb, $"{baseUrl}/contact", "monthly", "0.6");
        AddUrl(sb, $"{baseUrl}/terms", "yearly", "0.3");

        // صفحات خدمات
        foreach (var slug in serviceSlugs)
        {
            AddUrl(sb, $"{baseUrl}/services/{slug}", "weekly", "0.8");
        }

        // صفحات وبلاگ
        foreach (var slug in blogSlugs)
        {
            AddUrl(sb, $"{baseUrl}/blog/{slug}", "monthly", "0.7");
        }

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }

    public string GenerateRobotsTxt(string baseUrl)
    {
        baseUrl = NormalizeBaseUrl(baseUrl);
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /panel/");
        sb.AppendLine("Disallow: /login");
        sb.AppendLine("Disallow: /cart");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        return sb.ToString();
    }

    private static void AddUrl(StringBuilder sb, string loc, string changefreq, string priority)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{loc}</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        sb.AppendLine($"    <priority>{priority}</priority>");
        sb.AppendLine("  </url>");
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return DefaultBaseUrl;
        return baseUrl.TrimEnd('/');
    }
}
