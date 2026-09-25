# SolonProject — سایت و پنل کاربری سالن زیبایی حدیث (Hadis Beauty)

پروژه شامل سه لایه است:

| پروژه | توضیح |
|---|---|
| `BlazorAppSolon` | کلاینت Blazor WebAssembly (سایت عمومی + پنل مشتری/پرسنل/ادمین) |
| `WebApiSolon` | WebApi با احراز هویت JWT و کنترلرهای CRUD اطلاعات پایه |
| `DataCore` | اینترفیس ها و پیاده سازی سرویس ها (ریپازیتوری روی EF Core) |
| `DataLayer` | موجودیت ها، DbContext و Migration ها |

## اجرا

### ۱) WebApi
```bash
dotnet run --project WebApiSolon
```
- Swagger: `https://localhost:7182/swagger`
- برای اجرا **بدون SQL Server** در `appsettings.Development.json` مقدار `UseInMemoryDatabase: true` است
  (داده نمونه به صورت خودکار Seed می شود). برای استفاده از SQL Server آن را `false` کنید.

### ۲) کلاینت Blazor
```bash
dotnet run --project BlazorAppSolon
```
آدرس WebApi در `BlazorAppSolon/wwwroot/appsettings.json` → `ApiBaseUrl` تنظیم می شود.
اگر سرویس در دسترس نباشد، پنل ها به صورت خودکار با داده نمونه محلی کار می کنند
(نشانگر وضعیت در بالای پنل: «متصل به WebApi» / «داده نمونه»).

## ورود به پنل
صفحه ورود: `/login` — ورود دو مرحله ای (موبایل ← کد تأیید).
کد تأیید محیط توسعه: **12345**

| نقش | موبایل | پنل |
|---|---|---|
| ادمین | 09120000001 | `/panel/admin` |
| پرسنل | 09120000002 | `/panel/personnel` |
| مشتری | 09120000003 | `/panel/customer` |

## احراز هویت
- توکن JWT در `WebApiSolon` صادر می شود (`POST /api/auth/login`) و نقش ها از `Tbl_UserRole` + `Tbl_Roles` خوانده می شود.
- کلاینت توکن را در `localStorage` نگه می دارد و `JwtAuthenticationStateProvider` وضعیت ورود و نقش ها را مدیریت می کند.
- کنترلرهای اطلاعات پایه با `[Authorize(Roles = "Admin")]` محافظت شده اند.
- ⚠️ کلید JWT در `appsettings.json` قبل از انتشار حتماً تغییر کند و ارسال پیامک واقعی جایگزین کد ثابت شود.

## اطلاعات پایه (پنل ادمین)
CRUD کامل برای ۱۶ موجودیت: سالن، دسته بندی، خدمات، شبکه اجتماعی، خبر روز، پرسنل، مشتری،
موبایل، کد تأیید، نقش، دسترسی، نقش-دسترسی، نقش کاربر، دسته نمونه کار، نمونه کار، لایک.

## طراحی
رابط کاربری بر اساس بریف طراحی «Hadis Beauty» پیاده شده است: چیدمان RTL، پالت بژ/مشکی/قرمز،
تایپوگرافی سریف برای برند، هدر دو ردیفه، هیرو قوسی، گرید خدمات با آیکون های خطی، بخش CTA و فوتر تیره.
