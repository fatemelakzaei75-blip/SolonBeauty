using System.Collections.Generic;
using DataCore.Models.Seo;
using DataLayer.Entity;

namespace DataCore.Interfaces;

/// <summary>
/// قرارداد خدمات مدیریت سئو، متادیتا و اسکیماهای ساختاریافته Schema.org
/// </summary>
public interface ISeoService
{
    SeoMetadata GetHomeSeo(Tbl_SalonSetting? settings, string baseUrl);

    SeoMetadata GetServiceSeo(string slug, string title, string description, decimal price, string? image, string baseUrl);

    SeoMetadata GetBlogPostSeo(string slug, string title, string summary, string author, System.DateTime publishDate, string image, string baseUrl);

    SeoMetadata GetGeneralPageSeo(string title, string description, string relativePath, string baseUrl, IEnumerable<BreadcrumbItem>? breadcrumbs = null);

    SeoMetadata GetNoIndexSeo(string title);

    string BuildLocalBusinessSchema(Tbl_SalonSetting? settings, string baseUrl);

    string BuildServiceSchema(string title, string description, decimal price, string? image, string url, string baseUrl);

    string BuildArticleSchema(string title, string summary, string author, System.DateTime publishDate, string image, string url, string baseUrl);

    string BuildFaqSchema(IEnumerable<(string Question, string Answer)> faqs);

    string BuildBreadcrumbSchema(IEnumerable<BreadcrumbItem> breadcrumbs, string baseUrl);

    string GenerateSitemapXml(string baseUrl, IEnumerable<string> serviceSlugs, IEnumerable<string> blogSlugs);

    string GenerateRobotsTxt(string baseUrl);
}
