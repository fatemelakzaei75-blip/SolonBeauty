using DataLayer.Entity;

namespace BlazorAppSolon.Data;

/// <summary>اطلاعات نمایشی یک نمونه کار برای صفحات خدمات</summary>
public class PortfolioItem
{
    public Guid Tc { get; set; }
    public string Title { get; set; } = "";
    public string ServiceSlug { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Image { get; set; } = "";
    public string? BeforeImage { get; set; }
    public string Description { get; set; } = "";
    public string Personal { get; set; } = "";
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public int Likes { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}

/// <summary>خدمتی که صفحه اختصاصی دارد</summary>
public record ServicePage(string Slug, string Title, string Intro, decimal FromPrice, string HeroImage);

/// <summary>
/// کاتالوگ نمونه کارها و صفحات خدمات.
/// برای اتصال به دیتابیس، همین داده ها از Tbl_Portfoilo و Tbl_SalonSerice خوانده می شوند.
/// </summary>
public static class PortfolioCatalog
{
    public static readonly ServicePage[] Services =
    {
        new("nail", "ناخن", "کاشت، ترمیم، مانیکور و طراحی ناخن با متریال درجه یک و رعایت کامل استانداردهای بهداشتی.", 700_000, "img/gallery-1.jpg"),
        new("hair", "مو (رنگ، لایت و کراتین)", "رنگ، هایلایت، بالیاژ، کراتین و احیای مو با متدهای روز و مواد حرفه ای.", 900_000, "img/work-hair.jpg"),
        new("bride", "عروس و میکاپ", "میکاپ عروس، شنیون و گریم مجلسی متناسب با فرم چهره و سبک دلخواه شما.", 2_200_000, "img/work-bridal.jpg"),
        new("skin", "پوست و فیشال", "پاکسازی، فیشال، میکرودرم و ماساژ صورت برای پوستی شفاف و سالم.", 800_000, "img/gallery-3.jpg"),
        new("lash", "مژه و ابرو", "اکستنشن مژه، لیفت و لمینت، اصلاح و طراحی ابرو.", 950_000, "img/gallery-2.jpg"),
    };

    public static ServicePage? ServiceBySlug(string slug)
        => Services.FirstOrDefault(s => s.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    private static readonly List<PortfolioItem> _items = Build();

    public static List<PortfolioItem> All() => _items;

    public static List<PortfolioItem> ByService(string slug)
        => _items.Where(i => i.ServiceSlug.Equals(slug, StringComparison.OrdinalIgnoreCase)).ToList();

    public static PortfolioItem? ByTc(Guid tc) => _items.FirstOrDefault(i => i.Tc == tc);

    private static List<PortfolioItem> Build()
    {
        var data = new (string Slug, string Service, string Title, string Img, string? Before, string Desc, string Personal, int Min, decimal Price, string[] Tags)[]
        {
            ("nail", "ناخن", "کاشت ناخن فرنچ کلاسیک", "img/gallery-1.jpg", "img/gallery-2.jpg",
             "کاشت ژل با فرم بادامی و طراحی فرنچ ساده؛ مناسب محیط کار و استفاده روزمره.", "سارا محمدی", 150, 1_900_000, new[] { "کاشت ژل", "فرم بادامی", "فرنچ" }),

            ("nail", "ناخن", "مانیکور نود و براق", "img/gallery-2.jpg", null,
             "مانیکور کامل به همراه لاک ژل در طیف نود؛ ماندگاری حدود سه هفته.", "سارا محمدی", 90, 700_000, new[] { "مانیکور", "لاک ژل" }),

            ("nail", "ناخن", "طراحی ناخن عروس", "img/gallery-3.jpg", null,
             "کاشت بلند با طراحی دستی و نگین کاری ظریف، هماهنگ با لباس عروس.", "نگین کاظمی", 210, 2_600_000, new[] { "عروس", "نگین کاری" }),

            ("hair", "مو", "بالیاژ عسلی روی موی تیره", "img/work-hair.jpg", "img/gallery-2.jpg",
             "روشن سازی تدریجی با تکنیک بالیاژ و تونر عسلی؛ همراه با ماسک ترمیم کننده.", "مریم اسدی", 240, 3_200_000, new[] { "بالیاژ", "روشن سازی", "تونر" }),

            ("hair", "مو", "کراتین و احیای مو", "img/gallery-2.jpg", null,
             "کراتین تراپی برای موهای آسیب دیده؛ کاهش وز و افزایش درخشندگی تا چند ماه.", "مریم اسدی", 180, 3_800_000, new[] { "کراتین", "احیا" }),

            ("bride", "عروس و میکاپ", "میکاپ عروس سافت گلم", "img/work-bridal.jpg", null,
             "گریم ملایم با پوششی طبیعی، سایه های نود و شنیون باز؛ مناسب مراسم روز.", "حدیث رحیمی", 240, 7_500_000, new[] { "سافت گلم", "شنیون" }),

            ("bride", "عروس و میکاپ", "شنیون مجلسی", "img/gallery-3.jpg", null,
             "شنیون بافت دار همراه با آرایش مجلسی؛ مناسب مهمانی و نامزدی.", "حدیث رحیمی", 120, 2_200_000, new[] { "شنیون", "مجلسی" }),

            ("skin", "پوست", "فیشال روشن کننده", "img/gallery-3.jpg", null,
             "پاکسازی عمیق، میکرودرم و ماسک ویتامین ث برای شفافیت پوست.", "الهام نوری", 75, 1_400_000, new[] { "فیشال", "ویتامین ث" }),

            ("skin", "پوست", "ماساژ و آبرسانی صورت", "img/cta.jpg", null,
             "ماساژ لنفاوی صورت به همراه ماسک آبرسان؛ مناسب پوست های خشک.", "الهام نوری", 60, 800_000, new[] { "ماساژ", "آبرسانی" }),

            ("lash", "مژه و ابرو", "اکستنشن مژه ولوم", "img/gallery-1.jpg", null,
             "اکستنشن مو به مو با حالت ولوم طبیعی؛ ترمیم توصیه شده هر سه هفته.", "نگین کاظمی", 120, 1_200_000, new[] { "اکستنشن", "ولوم" }),

            ("lash", "مژه و ابرو", "لیفت و لمینت مژه", "img/hero.jpg", null,
             "فر و تثبیت مژه های طبیعی به همراه تغذیه با کراتین؛ بدون نیاز به ترمیم.", "نگین کاظمی", 75, 950_000, new[] { "لیفت", "لمینت" }),
        };

        var rnd = new Random(7);
        return data.Select((d, i) => new PortfolioItem
        {
            Tc = Guid.Parse($"22222222-0000-0000-0000-{i + 1:D12}"),
            ServiceSlug = d.Slug,
            ServiceName = d.Service,
            Title = d.Title,
            Image = d.Img,
            BeforeImage = d.Before,
            Description = d.Desc,
            Personal = d.Personal,
            DurationMinutes = d.Min,
            Price = d.Price,
            Likes = rnd.Next(12, 140),
            Tags = d.Tags
        }).ToList();
    }
}
