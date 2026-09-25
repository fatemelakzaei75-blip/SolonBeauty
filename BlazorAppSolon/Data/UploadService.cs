using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorAppSolon.Auth;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAppSolon.Data;

/// <summary>نتیجه آپلود تصویر</summary>
public record UploadResult(bool Ok, string Message, string? Url, string? Hash, bool Duplicate);

/// <summary>
/// آپلود تصویر نمونه کار روی WebApi (ذخیره هش شده در ریشه API).
/// اگر سرویس در دسترس نباشد، تصویر به صورت Data URL محلی استفاده می شود تا کار پنل متوقف نشود.
/// </summary>
public class UploadService
{
    private readonly HttpClient _http;
    private readonly JwtAuthenticationStateProvider _auth;

    public const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] Allowed = { "image/jpeg", "image/png", "image/webp", "image/gif" };

    public UploadService(HttpClient http, JwtAuthenticationStateProvider auth)
    {
        _http = http; _auth = auth;
    }

    public async Task<UploadResult> UploadAsync(IBrowserFile file, string section = "portfolio")
    {
        if (file.Size > MaxBytes)
            return new UploadResult(false, "حجم تصویر نباید بیشتر از ۵ مگابایت باشد", null, null, false);

        if (!Allowed.Contains(file.ContentType.ToLowerInvariant()))
            return new UploadResult(false, "فرمت مجاز: jpg، png، webp، gif", null, null, false);

        // ۱) تلاش برای آپلود روی WebApi
        try
        {
            var token = await _auth.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                _http.DefaultRequestHeaders.Authorization = new("Bearer", token);

            using var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream(MaxBytes);
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var res = await _http.PostAsync($"{AuthService.ApiBase}/api/upload/image?section={section}", content);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadFromJsonAsync<JsonElement>();
                var data = json.GetProperty("data");
                var url = data.GetProperty("url").GetString();
                var hash = data.GetProperty("hash").GetString();
                var dup = data.TryGetProperty("duplicate", out var d) && d.GetBoolean();

                // آدرس مطلق تا تصویر از ریشه API خوانده شود
                var full = string.IsNullOrEmpty(AuthService.ApiBase) ? url : $"{AuthService.ApiBase}{url}";
                return new UploadResult(true, json.GetProperty("message").GetString() ?? "آپلود شد", full, hash, dup);
            }
        }
        catch { /* سرویس در دسترس نیست */ }

        // ۲) حالت آفلاین: تبدیل به Data URL برای پیش نمایش و ذخیره محلی
        try
        {
            using var ms = new MemoryStream();
            await file.OpenReadStream(MaxBytes).CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ms.ToArray())).ToLowerInvariant();

            return new UploadResult(true, "سرویس در دسترس نبود؛ تصویر به صورت محلی ثبت شد", 
                $"data:{file.ContentType};base64,{base64}", hash, false);
        }
        catch (Exception ex)
        {
            return new UploadResult(false, $"آپلود ناموفق بود: {ex.Message}", null, null, false);
        }
    }
}
