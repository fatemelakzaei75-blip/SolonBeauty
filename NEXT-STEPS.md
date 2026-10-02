# ادامه کار — دستورهای آماده

این فایل دستورهایی است که در محیط کاری (Workspace) به‌دلیل پاک شدن بین جلسات لازم می‌شود.
روی کامپیوتر خودتان معمولاً فقط بخش ۱ لازم است.

---

## ۱) ارسال تغییرات به GitHub

دو کامیت آماده است و روی `main` قرار دارند:

| کامیت | توضیح | فایل |
|---|---|---|
| `8807b4e` | رفع ۹ باگ بحرانی تجربه موبایل + حذف باقی‌مانده‌های قالب | ۳۰ |
| `a866925` | تبدیل جدول‌های پنل به کارت در موبایل | ۶ |

```bash
# اگر ریموت تنظیم نشده بود:
git remote add origin https://github.com/fatemelakzaei75-blip/SolonBeauty.git

# سپس:
git push origin main
```

اگر فقط می‌خواهید کار جدول را برگردانید (به‌خاطر ساختار دو کامیتی):
```bash
git revert a866925      # فقط تبدیل جدول به کارت را برمی‌گرداند
```

---

## ۲) اجرای پروژه

### پیش‌نیاز: نصب .NET 8 SDK
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0 --install-dir /var/tmp/dotnet
export PATH=$PATH:/var/tmp/dotnet
```
> نکته: SDK را **بیرون از پوشه پروژه** نصب کنید. نصب داخل پروژه
> حجم را به بیش از ۸۰۰ مگابایت می‌رساند و باعث ناقص شدن snapshot می‌شود.

### اجرای WebApi (اختیاری — برای داده واقعی)
```bash
dotnet run --project WebApiSolon
# Swagger: https://localhost:7182/swagger
```

### اجرای کلاینت Blazor
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://0.0.0.0:5142
dotnet run --project BlazorAppSolon --no-launch-profile
```

ورود به پنل: موبایل `09120000001` (ادمین) — کد تأیید `12345`

---

## ۳) نگه‌داشتن حجم ورک‌اسپیس کم

خروجی build حدود ۱۲۰ مگابایت است و در لیست مستثنای snapshot نیست
(فقط `node_modules`، `dist`، `build` و چند مورد دیگر مستثنا هستند).

پیش از دانلود، این را بزنید:
```bash
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
```

| حالت | حجم |
|---|---|
| بعد از پاکسازی | ~۸ MB ✅ |
| بعد از build | ~۲۰۸ MB ⚠️ |

---

## ۴) نکات باقی‌مانده از ممیزی

کارهای انجام‌نشده در `AUDIT-Mobile-First.md` بخش ۵ فهرست شده‌اند.
مهم‌ترینشان:

- **۹۵ ایموجی به‌جای آیکون** در پنل‌ها (کامپوننت `ServiceIcon.razor` با آیکون SVG ساخته شده ولی استفاده نمی‌شود)
- **WebP + `srcset`** برای صرفه‌جویی بیشتر در تصاویر
- **شکستن `app.css`** تک‌فایلی (۹۲KB، بدون تفکیک Critical)
- **حذف ۳۶ مورد `!important`**
- **امنیت:** کلید JWT هاردکد، CORS کاملاً باز، و پرداخت شبیه‌ساز (نه درگاه واقعی)
