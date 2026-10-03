#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سازنده پیش‌نمایش صفحه آغاز (نماد بی‌نهایت).

CSS و JS را از فایل‌های واقعی پروژه می‌خواند تا پیش‌نمایش هرگز از کد اصلی
جدا نیفتد. مارک‌آپ تولیدشده اینجا همان مارک‌آپی است که در index.html می‌نشیند.

اجرا:  python3 tools/build-splash-preview.py
خروجی: splash-preview.html
"""
import io
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from splash_markup import splash   # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CSS_SRC = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "css", "splash.css")
JS_SRC = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "js", "splash.js")
OUT = os.path.join(ROOT, "splash-preview.html")

TEMPLATE = """<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover" />
<title>صفحه آغاز — نماد بی‌نهایت | SolonBeauty</title>
<style>
__CSS__

/* ==========================================================================
   چیدمان خودِ صفحه پیش‌نمایش — بخشی از اپ نیست
   ========================================================================== */
body {
    margin: 0;
    background: #0C0A08;
    color: #EDE6DA;
    font-family: 'Vazirmatn', Tahoma, system-ui, sans-serif;
    -webkit-font-smoothing: antialiased;
}

.wrap { max-width: 1500px; margin: 0 auto; padding: 26px; }

.note {
    max-width: 900px; padding: 26px 0 10px;
    font-size: .82rem; line-height: 2; color: rgba(237,230,218,.68);
}
.note h1 { margin: 0 0 14px; font-size: 1.12rem; color: #F7F2EA; font-weight: 800; }
.note h2 { margin: 26px 0 8px; font-size: .92rem; color: #C6A46A; font-weight: 700; }
.note strong { color: #C6A46A; font-weight: 700; }
.note code {
    font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
    background: rgba(237,230,218,.08); padding: 1px 6px; border-radius: 5px; font-size: .92em;
}
.note ul { margin: 8px 0 0; padding-inline-start: 20px; }
.note li { margin-block-end: 5px; }
.note .ok { color: #8FCB9B; }
.note .warn { color: #E8C07D; }

/* قاب آزمایش: اندازه‌های واقعی دستگاه‌ها.
   .s-splash در اپ fixed است؛ اینجا absolute می‌شود تا داخل قاب بماند. */
.frames { display: flex; flex-wrap: wrap; gap: 22px; align-items: flex-start; margin-top: 22px; }
.frame { display: flex; flex-direction: column; gap: 9px; }
.frame-label {
    font-size: .74rem; font-weight: 700; color: rgba(237,230,218,.55);
    display: flex; align-items: center; gap: 8px;
}
.frame-label b { color: #C6A46A; font-variant-numeric: tabular-nums; }

.lab {
    position: relative;
    border-radius: 20px;
    border: 1px solid rgba(237,230,218,.1);
    overflow: hidden;
    flex: none;
}
.lab .s-splash { position: absolute; }

/* عرض‌های واقعی دستگاه‌ها */
.w320 { inline-size: 320px; block-size: 560px; }
.w390 { inline-size: 390px; block-size: 760px; }
.w768 { inline-size: 768px; block-size: 620px; }
.w1100 { inline-size: 1100px; block-size: 560px; }
.h320 { inline-size: 700px; block-size: 320px; }   /* گوشی افقی */

/* تخته تنظیم زنده */
.scrub { margin-top: 26px; max-width: 900px; }
.scrub-row {
    display: flex; align-items: center; gap: 14px;
    background: rgba(237,230,218,.05);
    border: 1px solid rgba(237,230,218,.12);
    border-radius: 14px; padding: 14px 16px;
}
.scrub input[type="range"] { flex: 1; accent-color: #C6A46A; height: 24px; cursor: pointer; }
.scrub-val {
    min-inline-size: 62px; text-align: center; font-weight: 800;
    color: #F1DDAE; font-variant-numeric: tabular-nums;
}
.scrub-btns { display: flex; gap: 8px; margin-top: 10px; flex-wrap: wrap; }
.scrub-btns button {
    font-family: inherit; font-size: .78rem; font-weight: 700;
    padding: 9px 16px; border-radius: 9px; cursor: pointer;
    border: 1px solid rgba(198,164,106,.45); background: transparent; color: #F1DDAE;
}
.scrub-btns button:hover { background: rgba(198,164,106,.14); }
</style>
</head>
<body>
<div class="wrap">

<div class="note">
    <h1>صفحه آغاز — نماد بی‌نهایت</h1>
    <p>
        این فایل از <strong>CSS و JS واقعی پروژه</strong> ساخته شده
        (<code>splash.css</code> و <code>splash.js</code>) و مارک‌آپش همان مارک‌آپی است
        که در <code>index.html</code> می‌نشیند. پس هر چه اینجا می‌بینید، همان چیزی است
        که کاربر می‌بیند.
    </p>

    <h2>۱) قاب‌های دستگاه — ببینید واکنش‌گرایی درست کار می‌کند</h2>
    <ul>
        <li>گوشی ۳۲۰px و ۳۹۰px: نماد ۹۰٪ عرض را می‌گیرد و وسط‌چین است.</li>
        <li>تبلت ۷۶۸px: عرض نماد به سقف ۴۶۰ پیکسل رسیده و وسط‌چین مانده — کشیده نشده.</li>
        <li>دسکتاپ ۱۱۰۰px: همان سقف ۴۶۰ پیکسل. بدون سقف، نماد غول‌آسا می‌شد.</li>
        <li>گوشی افقی (۳۲۰px ارتفاع): قاعده مخصوص صفحه‌های کوتاه نماد و متن را
        کوچک می‌کند تا چیزی از کادر بیرون نزند.</li>
    </ul>

    <h2>۲) تخته تنظیم — حرکت سر نور و شمارش درصد</h2>
    <ul>
        <li>نوار را بالا ببرید: خط طلایی پشت سر نور پر می‌شود و درصد فارسی می‌شمارد.</li>
        <li><span class="warn">نوار فقط بالا می‌رود</span> — این عمدی است. نگهبان یک‌نوا
        در <code>setProgress</code> هر مقدار نزولی را دور می‌ریزد تا نوار عقب نرود.
        برای برگشت، «از صفر» را بزنید.</li>
    </ul>

    <h2>۳) دکمه‌ها — سه وضعیت واقعی</h2>
    <ul>
        <li><strong>خطا</strong>: نماد همان‌جا خشک می‌شود، ردیف درصد کنار می‌رود و پیام
        فارسی با دکمه «تلاش دوباره» می‌آید.</li>
        <li><strong>پایان</strong>: لایه با یک محو شدن نرم کنار می‌رود (روی نمونه اصلی).</li>
    </ul>

    <h2>۴) چه چیزی را چشمی تأیید کنید</h2>
    <ul>
        <li>شکل ∞ متقارن است و دو رشته در وسط با زاویه از هم می‌گذرند، نه چسبیده.</li>
        <li>ذرات بسیار کم‌رنگ‌اند و «نفس می‌کشند» — نباید شبیه ستاره‌های شلوغ باشند.</li>
        <li>هاله پشت نماد هم با همان ریتم نفس می‌کشد.</li>
        <li>نام فارسی حروفش به هم چسبیده مانده (فاصله حروف فقط روی نام لاتین باز است).</li>
    </ul>
</div>

<div class="scrub">
    <div class="scrub-row">
        <span style="font-size:.78rem;color:rgba(237,230,218,.6)">پیشرفت نمونه اصلی</span>
        <input type="range" id="scrub" min="0" max="1" step="0.01" value="0.45"
               aria-label="تنظیم دستی پیشرفت" />
        <span class="scrub-val" id="scrubval">۴۵٪</span>
    </div>
    <div class="scrub-btns">
        <button type="button" id="btn-reset">از صفر</button>
        <button type="button" id="btn-finish">پایان (محو شدن)</button>
        <button type="button" id="btn-fail">خطا</button>
        <button type="button" id="btn-40">برو به ۴۰٪</button>
    </div>
</div>

<div class="frames">
    <div class="frame">
        <span class="frame-label">نمونه اصلی — زنده، با تخته تنظیم <b>#solon-splash</b></span>
        <div class="lab w390">__MAIN__</div>
    </div>
    <div class="frame">
        <span class="frame-label">گوشی کوچک <b>320 × 560</b></span>
        <div class="lab w320">__S320__</div>
    </div>
    <div class="frame">
        <span class="frame-label">تبلت <b>768</b></span>
        <div class="lab w768">__S768__</div>
    </div>
    <div class="frame">
        <span class="frame-label">دسکتاپ <b>1100</b></span>
        <div class="lab w1100">__S1100__</div>
    </div>
    <div class="frame">
        <span class="frame-label">گوشی افقی <b>700 × 320</b></span>
        <div class="lab h320">__SLAND__</div>
    </div>
    <div class="frame">
        <span class="frame-label">حالت خطا — نماد خشک‌شده <b>is-failed</b></span>
        <div class="lab w320">__FAIL__</div>
    </div>
</div>

</div>

<script>
__JS__
</script>
<script>
/* فقط برای همین پیش‌نمایش: تخته تنظیم دستی. در اپ پیشرفت از مراحل واقعی
   بارگذاری می‌آید (BootService.cs). */
(function () {
    var FA = '۰۱۲۳۴۵۶۷۸۹';
    function pct(v) {
        var n = Math.round(v * 100);
        return String(n).replace(/\\d/g, function (d) { return FA[+d]; }) + '٪';
    }

    var slider = document.getElementById('scrub');
    var label = document.getElementById('scrubval');
    if (!slider || !window.solonSplash) return;

    slider.addEventListener('input', function () {
        var v = parseFloat(slider.value);
        label.textContent = pct(v);
        window.solonSplash.setProgress(v);
    });

    document.getElementById('btn-40').addEventListener('click', function () {
        slider.value = 0.4; label.textContent = pct(0.4);
        window.solonSplash.setProgress(0.4);
    });

    document.getElementById('btn-finish').addEventListener('click', function () {
        slider.value = 1; label.textContent = pct(1);
        window.solonSplash.finish();
    });

    document.getElementById('btn-fail').addEventListener('click', function () {
        window.solonSplash.fail('بارگذاری نرم‌افزار کامل نشد. ارتباط با سرور را بررسی کنید.');
    });

    document.getElementById('btn-reset').addEventListener('click', function () {
        location.reload();
    });
})();
</script>
</body>
</html>
"""


def build():
    with io.open(CSS_SRC, encoding="utf-8") as f:
        css = f.read()
    with io.open(JS_SRC, encoding="utf-8") as f:
        js = f.read()

    page = (TEMPLATE
            .replace("__CSS__", css)
            .replace("__JS__", js)
            .replace("__MAIN__", splash(0, "solon-splash", with_defs=True))
            .replace("__S320__", splash(0.62))
            .replace("__S768__", splash(0.28))
            .replace("__S1100__", splash(0.85))
            .replace("__SLAND__", splash(0.5))
            .replace("__FAIL__", splash(0.42, failed=True)))

    with io.open(OUT, "w", encoding="utf-8") as f:
        f.write(page)
    print("ساخته شد: %s  (%d KB)" % (OUT, os.path.getsize(OUT) // 1024))


if __name__ == "__main__":
    build()
