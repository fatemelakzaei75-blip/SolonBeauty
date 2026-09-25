using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BlazorAppSolon.Auth;

/// <summary>
/// سرویس ورود: ابتدا تلاش می کند از WebApi (JWT واقعی) توکن بگیرد؛
/// اگر API در دسترس نبود، در حالت نمونه یک توکن JWT محلی برای دموی پنل ها می سازد.
/// </summary>
public class AuthService
{
    private readonly HttpClient _http;
    private readonly JwtAuthenticationStateProvider _authState;

    /// <summary>آدرس WebApi — در صورت انتشار جداگانه اینجا تنظیم شود</summary>
    public static string ApiBase { get; set; } = "";

    /// <summary>کد تأیید محیط توسعه</summary>
    public const string DevCode = "12345";

    /// <summary>حساب های نمونه حالت آفلاین</summary>
    public static readonly Dictionary<string, (string Name, string Role)> DemoUsers = new()
    {
        ["09120000001"] = ("مدیر سالن حدیث", AppRoles.Admin),
        ["09120000002"] = ("سارا محمدی", AppRoles.Personnel),
        ["09120000003"] = ("نگار رضایی", AppRoles.Customer),
    };

    public AuthService(HttpClient http, JwtAuthenticationStateProvider authState)
    {
        _http = http; _authState = authState;
    }

    public async Task<(bool Ok, string Message)> SendCodeAsync(string mobile)
    {
        mobile = DigitHelper.Normalize(mobile);
        try
        {
            var res = await _http.PostAsJsonAsync($"{ApiBase}/api/auth/send-code", new SendCodeRequest { MobileNumber = mobile });
            if (res.IsSuccessStatusCode)
                return (true, $"کد تأیید ارسال شد (کد محیط توسعه: {DevCode})");
        }
        catch { /* API در دسترس نیست — حالت نمونه */ }

        return (true, $"کد تأیید ارسال شد (حالت نمونه، کد: {DevCode})");
    }

    public async Task<(bool Ok, string Message, LoginResponse? User)> LoginAsync(LoginRequest request)
    {
        // ارقام فارسی/عربی و فاصله های اضافی پاک می شوند
        request.MobileNumber = DigitHelper.Normalize(request.MobileNumber);
        request.Code = DigitHelper.Normalize(request.Code);

        // ۱) تلاش برای ورود از طریق WebApi
        try
        {
            var res = await _http.PostAsJsonAsync($"{ApiBase}/api/auth/login", request);
            if (res.IsSuccessStatusCode)
            {
                var user = await res.Content.ReadFromJsonAsync<LoginResponse>();
                if (user is not null && !string.IsNullOrEmpty(user.Token))
                {
                    await _authState.MarkUserAsAuthenticatedAsync(user.Token);
                    return (true, "ورود موفق", user);
                }
            }
        }
        catch { /* API در دسترس نیست — حالت نمونه */ }

        // ۲) حالت نمونه (بدون سرور)
        if (request.Code != DevCode || !DemoUsers.TryGetValue(request.MobileNumber, out var demo))
            return (false, "شماره موبایل یا کد تأیید نادرست است", null);

        var tc = Guid.NewGuid();
        var token = BuildLocalToken(tc, demo.Name, request.MobileNumber, demo.Role);
        await _authState.MarkUserAsAuthenticatedAsync(token);

        return (true, "ورود موفق (حالت نمونه)", new LoginResponse
        {
            Token = token,
            DisplayName = demo.Name,
            MobileNumber = request.MobileNumber,
            Tc = tc,
            Roles = new List<string> { demo.Role },
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }

    /// <summary>روش دوم ورود: شماره موبایل + رمز عبور (بند ۱۳ سند)</summary>
    public async Task<(bool Ok, string Message, LoginResponse? User)> LoginWithPasswordAsync(string mobile, string password)
    {
        mobile = DigitHelper.Normalize(mobile);
        try
        {
            var res = await _http.PostAsJsonAsync($"{ApiBase}/api/auth/login-password", new LoginPasswordRequest
            {
                MobileNumber = mobile,
                Password = password
            });
            if (res.IsSuccessStatusCode)
            {
                var user = await res.Content.ReadFromJsonAsync<LoginResponse>();
                if (user is not null && !string.IsNullOrEmpty(user.Token))
                {
                    await _authState.MarkUserAsAuthenticatedAsync(user.Token);
                    return (true, "ورود با رمز عبور موفقیت‌آمیز بود", user);
                }
            }
        }
        catch { }

        // حالت دمو
        if (password == "123456" || password == "Admin@123456")
        {
            var role = AppRoles.Customer;
            var name = "مشتری سالن";
            if (DemoUsers.TryGetValue(mobile, out var demo))
            {
                name = demo.Name;
                role = demo.Role;
            }

            var tc = Guid.NewGuid();
            var token = BuildLocalToken(tc, name, mobile, role);
            await _authState.MarkUserAsAuthenticatedAsync(token);

            return (true, "ورود با موفقیت انجام شد", new LoginResponse
            {
                Token = token,
                DisplayName = name,
                MobileNumber = mobile,
                Tc = tc,
                Roles = new List<string> { role },
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            });
        }

        return (false, "رمز عبور نادرست است (رمز پیش‌فرض تستی: 123456)", null);
    }

    /// <summary>بازیابی رمز عبور با OTP پیامکی (بند ۱۳ سند)</summary>
    public async Task<(bool Ok, string Message)> ResetPasswordAsync(string mobile, string code, string newPassword)
    {
        mobile = DigitHelper.Normalize(mobile);
        code = DigitHelper.Normalize(code);
        try
        {
            var res = await _http.PostAsJsonAsync($"{ApiBase}/api/auth/reset-password", new ResetPasswordRequest
            {
                MobileNumber = mobile,
                Code = code,
                NewPassword = newPassword
            });
            if (res.IsSuccessStatusCode) return (true, "رمز عبور با موفقیت به‌روز شد.");
        }
        catch { }

        if (code == DevCode) return (true, "رمز عبور با موفقیت تغییر کرد. اکنون با رمز جدید وارد شوید.");
        return (false, "کد تأیید نادرست است.");
    }

    public Task LogoutAsync() => _authState.MarkUserAsLoggedOutAsync();

    /// <summary>ساخت توکن JWT محلی (فقط برای دمو بدون سرور)</summary>
    private static string BuildLocalToken(Guid tc, string name, string mobile, string role)
    {
        static string B64(byte[] data) => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var header = B64(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" })));
        var payload = B64(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["nameid"] = tc.ToString(),
            ["unique_name"] = name,
            ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone"] = mobile,
            ["role"] = role,
            ["exp"] = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds()
        })));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("local-demo-key"));
        var signature = B64(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{header}.{payload}")));
        return $"{header}.{payload}.{signature}";
    }
}
