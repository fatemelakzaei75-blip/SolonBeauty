using System.Text;
using DataCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>
/// کنترلر تولید داینامیک Sitemap.xml و Robots.txt بر اساس وضعیت پایگاه داده و تنظیمات سالن
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeoController : ControllerBase
{
    private readonly ISeoService _seoService;
    private readonly ISalonSettingService _settingService;

    public SeoController(ISeoService seoService, ISalonSettingService settingService)
    {
        _seoService = seoService;
        _settingService = settingService;
    }

    /// <summary>
    /// تولید فایل داینامیک Sitemap.xml
    /// </summary>
    [HttpGet("sitemap.xml")]
    [Produces("application/xml")]
    public IActionResult GetSitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
        var serviceSlugs = new[] { "nail", "hair", "bride", "skin", "lash" };
        var blogSlugs = new[] { "nail-care-after-extension", "skin-care-routine", "hair-color-trends" };

        var xml = _seoService.GenerateSitemapXml(baseUrl, serviceSlugs, blogSlugs);
        return Content(xml, "application/xml", Encoding.UTF8);
    }

    /// <summary>
    /// تولید فایل داینامیک Robots.txt
    /// </summary>
    [HttpGet("robots.txt")]
    [Produces("text/plain")]
    public IActionResult GetRobots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
        var txt = _seoService.GenerateRobotsTxt(baseUrl);
        return Content(txt, "text/plain", Encoding.UTF8);
    }
}
