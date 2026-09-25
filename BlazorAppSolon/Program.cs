using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using BlazorAppSolon;
using BlazorAppSolon.Auth;
using BlazorAppSolon.Data;
using BlazorAppSolon.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ثبت پایه HttpClient برای درخواست‌های کلاینت
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ---------- احراز هویت متمرکز JWT ----------
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtAuthorizationMessageHandler>();
builder.Services.AddAuthorizationCore();

// ---------- سرویس‌های ارتباطی استاندارد با WebApi (بر اساس بند ۱۴ و ۱۵) ----------
builder.Services.AddScoped<ReservationApiService>();
builder.Services.AddScoped<NotificationApiService>();
builder.Services.AddScoped<ReportApiService>();
builder.Services.AddScoped<DiscountApiService>();
builder.Services.AddScoped<SalonSettingApiService>();

// ---------- منبع داده و کش پنل‌ها ----------
builder.Services.AddSingleton<MockDb>();
builder.Services.AddScoped<IDataStore, ApiDataStore>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<ReservationClient>();
builder.Services.AddScoped<DataCore.Interfaces.ISeoService, DataCore.Services.SeoService>();

// خواندن آدرس پایه API از تنظیمات محیطی wwwroot/appsettings.json
using (var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
    try
    {
        var cfg = await http.GetFromJsonAsync<Dictionary<string, string>>("appsettings.json");
        if (cfg is not null && cfg.TryGetValue("ApiBaseUrl", out var apiBase))
        {
            AuthService.ApiBase = apiBase.TrimEnd('/');
        }
    }
    catch
    {
        // استفاده از مقادیر پیش‌فرض در صورت نبود فایل تنظیمات
    }
}

await builder.Build().RunAsync();
