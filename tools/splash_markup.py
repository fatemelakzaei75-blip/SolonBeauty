#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
منبع واحد مارک‌آپ صفحه آغاز (نماد بی‌نهایت).

چرا این فایل جدا شده: همین مارک‌آپ در دو جا لازم است — در index.html که
کاربر واقعی می‌بیند، و در splash-preview.html که برای بازبینی چشمی ساخته
می‌شود. اگر هر کدام نسخه خودش را داشت، دیر یا زود از هم می‌افتادند. حالا هر
دو از همین یک تابع می‌آیند.

استفاده‌کننده‌ها:
    tools/build-splash-preview.py   پیش‌نمایش چند‌قابی
    tools/sync-splash-markup.py     نوشتن همین مارک‌آپ در index.html
"""

# ─────────────────────────── هندسه مسیر ───────────────────────────
# مسیر بی‌نهایت از چهار قطعه مکعبی ساخته می‌شود و از مرکز تقاطع شروع و به
# همان نقطه برمی‌گردد. خروجی و ورودی هر قطعه در مرکز هم‌جهت است؛ یعنی هر رشته
# در مرکز پیوسته است و دو رشته با یک زاویه از هم می‌گذرند — همان تقاطعی که
# شکل ∞ را می‌سازد.
#
# اعداد: viewBox ۳۲۰×۱۸۰، مرکز تقاطع (۱۶۰،۹۰)، دو انتها (۳۴،۹۰) و (۲۸۶،۹۰).
# هر چهار قطعه دقیقاً هم‌طول‌اند (۱۷۵٫۰۵۲ واحد) و همین باعث می‌شود ۲۵٪ و ۷۵٪
# طول مسیر دقیقاً روی انتها و ۵۰٪ روی مرکز بیفتد. tools/test-splash.js این
# ویژگی‌ها را عددی بررسی می‌کند.
PATH_D = ("M160 90 C190 28 286 20 286 90 C286 160 190 152 160 90 "
          "C130 28 34 20 34 90 C34 160 130 152 160 90")

DEFS = """<svg class="s-splash__defs" width="0" height="0" aria-hidden="true" focusable="false">
    <defs>
      <!-- تنها تعریف هندسه در کل سند. هر سه لایه با use به همین ارجاع می‌دهند.
           pathLength="1" یعنی dasharray و dashoffset در بازه ۰ تا ۱ کار می‌کنند
           و هیچ عدد جادویی طول مسیر در CSS نمی‌آید. -->
      <path class="s-inf__geom" id="solon-inf-path" pathLength="1" fill="none" d="__PATH_D__" />

      <linearGradient id="solon-inf-grad" x1="0" y1="0" x2="1" y2="0">
        <!-- رنگ‌ها از CSS می‌آیند تا هیچ کدرنگ خامی در مارک‌آپ نماند -->
        <stop class="s-inf__stop-a" offset="0" />
        <stop class="s-inf__stop-b" offset=".5" />
        <stop class="s-inf__stop-c" offset="1" />
      </linearGradient>

      <filter id="solon-inf-soft" x="-30%" y="-30%" width="160%" height="160%">
        <feGaussianBlur stdDeviation="5" />
      </filter>
    </defs>
  </svg>"""

# متن‌های پیش‌فرض = همان مقادیر پیش‌فرض Tbl_SalonSetting در دیتابیس.
# پس اگر پاسخ API نرسد، کاربر همان چیزی را می‌بیند که قرار بوده ببیند و هیچ
# جای خالی یا متنی جا نمی‌ماند.
FALLBACK_NAME = "سالن زیبایی حدیث"
FALLBACK_LATIN = "HADIS BEAUTY"

# نشانگرهای جای‌گذاری در index.html
START_MARKER = "<!-- SOLON-SPLASH:START -->"
END_MARKER = "<!-- SOLON-SPLASH:END -->"


def splash(progress=None, element_id=None, with_defs=False, failed=False):
    """یک نمونه کامل صفحه آغاز.

    Args:
        progress: مقدار پیشرفت ثابت برای پیش‌نمایش. None یعنی «حالت زنده»:
                  مقدار اولیه صفر است و مراحل واقعی بارگذاری آن را بالا
                  می‌برند. در اپ همیشه None است.
        element_id: شناسه عنصر؛ نمونه اصلی باید solon-splash باشد تا
                    setProgress به آن برسد.
        with_defs: فقط نمونه اول تعریف‌ها را همراه دارد. بقیه با use به همان
                   ارجاع می‌دهند و شناسه تکراری در سند ساخته نمی‌شود.
        failed: حالت خطا — نماد خشک، پیام و دکمه تلاش دوباره.
    """
    id_attr = ' id="%s"' % element_id if element_id else ""
    prog_attr = "" if progress is None else ' data-progress="%s"' % progress
    defs = DEFS.replace("__PATH_D__", PATH_D) if with_defs else ""
    cls = "s-splash is-failed" if failed else "s-splash"
    hidden_attr = "" if failed else " hidden"
    status_style = ' style="display:none"' if failed else ""

    return """<div class="__CLS__"__ID____PROG__>
  __DEFS__

  <!-- تزئینی: هاله نفس‌کش و ذرات. aria-hidden چون هیچ اطلاعاتی حمل نمی‌کنند -->
  <div class="s-splash__glow" aria-hidden="true"></div>
  <div class="s-splash__particles" aria-hidden="true"><i></i><i></i><i></i></div>

  <div class="s-splash__stage">
    <div class="s-splash__brand">
      <!-- لوگو از Tbl_SalonSetting.LogoUrl می‌آید. تا وقتی تصویر واقعاً بار
           نشده hidden است، پس هرگز کادر خالی دیده نمی‌شود. -->
      <img class="s-splash__brand-logo" data-logo alt="" hidden />
      <span class="s-splash__brand-latin" data-brand-latin>__LATIN__</span>
      <span class="s-splash__brand-name" data-brand>__NAME__</span>
    </div>

    <svg class="s-inf" viewBox="0 0 320 180" role="presentation" aria-hidden="true" focusable="false">
      <use class="s-inf__track" href="#solon-inf-path" />
      <use class="s-inf__glow" href="#solon-inf-path" />
      <use class="s-inf__fill" href="#solon-inf-path" />
      <!-- نقطه شروع همان مرکز تقاطع ست شده تا پیش از اجرای جاوااسکریپت در
           گوشه ۰٬۰ (بیرون نماد) دیده نشود. -->
      <g class="s-inf__head" style="transform: translate(160px, 90px)">
        <circle class="s-inf__head-halo" r="13" cx="0" cy="0" />
        <circle class="s-inf__head-core" r="4.6" cx="0" cy="0" />
      </g>
    </svg>

    <!-- role=status + aria-live: متن و درصد به‌عنوان یک وضعیت زنده اعلام می‌شوند.
         عدد دیده‌شدنی aria-hidden است چون صدها بار بازنویسی می‌شود؛ اعلام‌ها از
         عنصر پنهان s-splash__sr می‌آید که فقط سر هر ۱۰ درصد عوض می‌شود. -->
    <div class="s-splash__status" role="status" aria-live="polite"__STATUS_STYLE__>
      <p class="s-splash__loading">در حال بارگذاری نرم‌افزار<i class="s-splash__dots" aria-hidden="true"><b></b><b></b><b></b></i></p>
      <p class="s-splash__pct" aria-hidden="true"><span data-pct>۰٪</span></p>
      <span class="s-splash__sr" data-sr></span>
    </div>

    <div class="s-splash__error" data-error__HIDDEN__>
      <p class="s-splash__error-msg" data-error-msg>بارگذاری نرم‌افزار کامل نشد. ارتباط با سرور را بررسی کنید.</p>
      <button type="button" class="s-splash__retry" data-retry>تلاش دوباره</button>
    </div>
  </div>
</div>""" \
        .replace("__CLS__", cls) \
        .replace("__ID__", id_attr) \
        .replace("__PROG__", prog_attr) \
        .replace("__DEFS__", defs) \
        .replace("__LATIN__", FALLBACK_LATIN) \
        .replace("__NAME__", FALLBACK_NAME) \
        .replace("__STATUS_STYLE__", status_style) \
        .replace("__HIDDEN__", hidden_attr)
