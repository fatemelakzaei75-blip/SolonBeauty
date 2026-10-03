using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BlazorAppSolon.Auth;
using DataLayer.Entity;
using Microsoft.JSInterop;

namespace BlazorAppSolon.Services;

/// <summary>
/// مراحل واقعی بارگذاری نرم‌افزار برای صفحه آغاز.
///
/// چرا این کلاس وجود دارد: نوار پیشرفت صفحه آغاز نباید با تایمر تقلبی پر شود.
/// فاز اول (۰ تا ۴۵٪) پیشرفت واقعی دانلود خود Blazor است و در splash.js خوانده
/// می‌شود. فاز دوم — همین‌جا — پنج کار واقعی را انجام می‌دهد و ۴۵٪ را به ۱۰۰٪
/// می‌رساند. تا وقتی این پنج مرحله تمام نشده‌اند هیچ‌کس setProgress(1) را صدا
/// نمی‌زند؛ یعنی نوار هرگز بی‌دلیل به صد نمی‌رسد.
///
/// سه قاعده:
/// ۱) هر مرحله سقف زمانی دارد. یک قدم کند نباید نوار را برای همیشه بخواباند.
/// ۲) کل فاز بودجه زمانی دارد؛ عبور از بودجه یعنی خطا با پیام روشن.
/// ۳) شکست یک مرحله به‌تنهایی کشنده نیست. تنظیمات سالن مقدار پیش‌فرض دارد،
///    لوگو می‌تواند نباشد و فونت ممکن است کش شده باشد. اما اگر هر سه منبع
///    واقعی شبکه شکست بخورند (یعنی عملاً آفلاینیم) صادقانه خطا می‌دهیم، نه
///    اینکه با ۱۰۰٪ و یک صفحه ناقص تحویل بدهیم.
/// </summary>
public sealed class BootService
{
    // ---------- تنظیمات زمانی ----------
    private static readonly TimeSpan StepCap = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan TotalBudget = TimeSpan.FromSeconds(12);
    private const int ImageTimeoutMs = 4000;
    private const int FontTimeoutMs = 4000;

    // ---------- منابع واقعی ----------
    private const string HeroImagePath = "img/hero.jpg";
    private const string BodyFontSpec = "1rem Vazirmatn";

    /// <summary>سقف فاز اول. اگر روزی عوض شد، باید در splash.js هم عوض شود (BOOT_CAP).</summary>
    public const double PhaseStart = 0.45;

    /// <summary>تعداد منابع واقعی شبکه؛ اگر همه شکست بخورند خطا می‌دهیم.</summary>
    private const int NetworkResourceCount = 3;

    private readonly IJSRuntime _js;
    private readonly SalonSettingApiService _settingsApi;
    private readonly JwtAuthenticationStateProvider _auth;

    public BootService(IJSRuntime js, SalonSettingApiService settingsApi, JwtAuthenticationStateProvider auth)
    {
        _js = js;
        _settingsApi = settingsApi;
        _auth = auth;
    }

    /// <summary>تنظیمات سالن خوانده‌شده از API. اگر نشد null می‌ماند و رابط به مقادیر پیش‌فرض تکیه می‌کند.</summary>
    public Tbl_SalonSetting? Settings { get; private set; }

    /// <summary>توکن نشست کاربر، اگر وجود داشته باشد.</summary>
    public string? Token { get; private set; }

    /// <summary>اگر فاز دوم با خطا تمام شده باشد true است.</summary>
    public bool Failed { get; private set; }

    /// <summary>پیام خطا برای نمایش.</summary>
    public string? FailureMessage { get; private set; }

    public async Task RunAsync()
    {
        var clock = Stopwatch.StartNew();

        // پایان فاز اول. از این لحظه رصد متغیر Blazor و خزیدن متوقف می‌شود تا
        // فقط مراحل واقعی تعیین‌کننده باشند.
        await InvokeAsync("stopWatching");

        var networkFailures = 0;

        // ---------- گام ۱: نشست و توکن (۴۵٪ → ۵۸٪) ----------
        // خواندن توکن از حافظه مرورگر و بررسی انقضا. GetAuthenticationStateAsync
        // خودش اگر توکن منقضی باشد کاربر را خارج می‌کند.
        await ReportAsync(PhaseStart);
        await RunCappedAsync(async () =>
        {
            Token = await _auth.GetTokenAsync();
            await _auth.GetAuthenticationStateAsync();
        }, StepCap);
        await ReportAsync(0.58);

        // ---------- گام ۲: تنظیمات سالن از API (۵۸٪ → ۷۴٪) ----------
        await ReportAsync(0.58);
        await LoadSettingsStepAsync();

        if (Settings is not null)
        {
            // نام برند روی صفحه آغاز از همین‌جا می‌آید. اگر نرسد، همان مقدار
            // پیش‌فرض داخل index.html سر جایش می‌ماند.
            await InvokeAsync("setBrand", new
            {
                name = Settings.SalonName,
                latin = Settings.SalonLatinName,
                logo = string.IsNullOrWhiteSpace(Settings.LogoUrl) ? null : Settings.LogoUrl
            });
        }
        else
        {
            networkFailures++;
        }
        await ReportAsync(0.74);

        // ---------- گام ۳: لوگو (۷۴٪ → ۸۴٪) ----------
        // اگر سالن لوگو نداشته باشد این مرحله موفق است: مونوگرام متنی همان
        // جواب درست و موردانتظار است، نه شکست.
        await ReportAsync(0.74);
        var logoUrl = Settings?.LogoUrl;
        if (!string.IsNullOrWhiteSpace(logoUrl))
        {
            var logoOk = await InvokeBoolAsync("preloadImage", logoUrl, ImageTimeoutMs);
            if (!logoOk) networkFailures++;      // لوگوی خراب جای تصویر شکسته نمی‌گیرد
        }
        await ReportAsync(0.84);

        // ---------- گام ۴: فونت وزیرمتن (۸۴٪ → ۹۲٪) ----------
        await ReportAsync(0.84);
        var fontOk = await InvokeBoolAsync("awaitFont", BodyFontSpec, FontTimeoutMs);
        if (!fontOk) networkFailures++;
        await ReportAsync(0.92);

        // ---------- گام ۵: تصویر هیرو (۹۲٪ → ۱۰۰٪) ----------
        await ReportAsync(0.92);
        var heroOk = await InvokeBoolAsync("preloadImage", HeroImagePath, ImageTimeoutMs);
        if (!heroOk) networkFailures++;

        // ---------- داوری ----------
        if (networkFailures >= NetworkResourceCount)
        {
            await FailAsync("ارتباط با سرور برقرار نشد. اینترنت خود را بررسی کنید و دوباره تلاش کنید.");
            return;
        }

        if (clock.Elapsed > TotalBudget)
        {
            await FailAsync("بارگذاری نرم‌افزار بیش از حد طول کشید. دوباره تلاش کنید.");
            return;
        }

        // ---------- پایان واقعی ----------
        await ReportAsync(1.0);
        await InvokeAsync("finish");
    }

    /// <summary>
    /// خواندن تنظیمات سالن از WebApi با سقف زمانی.
    /// توجه: خود SalonSettingApiService هر استثنایی را می‌گیرد و پاسخ ناموفق
    /// برمی‌گرداند، ولی اینجا هم محافظت شده تا هیچ مسیری استثنا ندهد.
    /// </summary>
    private async Task LoadSettingsStepAsync()
    {
        Task<Tbl_SalonSetting?> task;

        try
        {
            task = LoadSettingsAsync();
        }
        catch
        {
            Settings = null;
            return;
        }

        if (await Task.WhenAny(task, Task.Delay(StepCap)) != task)
        {
            Forget(task);                        // از سقف گذشت؛ رهایش می‌کنیم
            Settings = null;
            return;
        }

        try
        {
            Settings = await task;
        }
        catch
        {
            Settings = null;
        }
    }

    private async Task<Tbl_SalonSetting?> LoadSettingsAsync()
    {
        var response = await _settingsApi.GetSettingsAsync();
        return response.Success ? response.Data : null;
    }

    // ======================================================================
    // ابزارها
    // ======================================================================

    /// <summary>گزارش پیشرفت به لایه نمایش. شکست فراخوانی هرگز بارگذاری را متوقف نمی‌کند.</summary>
    private async Task ReportAsync(double value)
    {
        await InvokeAsync("setProgress", value);
    }

    /// <summary>
    /// فراخوانی امن یک تابع جاوااسکریپت.
    /// چرا try/catch: اگر splash.js به هر دلیلی بار نشده باشد، نبودِ تابع
    /// نباید بالا آمدن کل نرم‌افزار را خراب کند.
    /// </summary>
    private async Task InvokeAsync(string fn, params object?[] args)
    {
        try
        {
            await _js.InvokeVoidAsync("solonSplash." + fn, args);
        }
        catch
        {
            // عمداً بی‌صدا: صفحه آغاز یک لایه تجربه کاربری است و نباید مانع
            // کارکرد اصلی نرم‌افزار شود.
        }
    }

    /// <summary>فراخوانی تابعی که مقدار برمی‌گرداند. در خطا یا گذشتن از سقف، false می‌دهد.</summary>
    private async Task<bool> InvokeBoolAsync(string fn, params object?[] args)
    {
        try
        {
            // AsTask چون IJSRuntime مقدار ValueTask برمی‌گرداند و WhenAny با Task کار می‌کند.
            var task = _js.InvokeAsync<bool>("solonSplash." + fn, args).AsTask();

            if (await Task.WhenAny(task, Task.Delay(StepCap)) != task)
            {
                Forget(task);
                return false;
            }

            return await task;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>اجرای یک گام بدون مقدار بازگشتی، با سقف زمانی. نتیجه‌اش «انجام شد یا نه» است.</summary>
    private static async Task<bool> RunCappedAsync(Func<Task> work, TimeSpan cap)
    {
        Task task;

        try
        {
            task = work();
        }
        catch
        {
            return false;
        }

        if (await Task.WhenAny(task, Task.Delay(cap)) != task)
        {
            Forget(task);
            return false;
        }

        try
        {
            await task;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>مصرف کردن نتیجه یک کار رهاشده تا استثنای بی‌صاحب نماند.</summary>
    private static void Forget(Task task)
    {
        _ = task.ContinueWith(
            t => { _ = t.Exception; },
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
    }

    private async Task FailAsync(string message)
    {
        Failed = true;
        FailureMessage = message;
        await InvokeAsync("fail", message);
    }
}
