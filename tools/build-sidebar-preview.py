#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سازنده پیش‌نمایش Sidebar کپسولی.

CSS را از فایل واقعی پروژه می‌خواند و درون HTML جای می‌دهد تا پیش‌نمایش
هرگز از کد اصلی جدا نیفتد. به‌دلیل نبود دسترسی شبکه در iframe پیش‌نمایش،
استایل‌ها باید درون‌خطی باشند.

اجرا:  python3 tools/build-sidebar-preview.py
خروجی: sidebar-preview.html
"""
import io
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CSS_SRC = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "css", "panel-sidebar.css")
OUT = os.path.join(ROOT, "sidebar-preview.html")
JS_SRC = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "js", "mobile-ux.js")

# آیکون‌ها — دقیقاً همان مسیرهای PanelNavIcons.razor تا پیش‌نمایش با اپ یکی باشد
I = {
    "dashboard": '<path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/>',
    "layers": '<path d="M12 3 2.5 8 12 13l9.5-5L12 3Z"/><path d="M2.5 13 12 18l9.5-5"/><path d="M2.5 17.5 12 22.5l9.5-5"/>',
    "calendar": '<rect x="3" y="5" width="18" height="16" rx="3"/><path d="M3 10h18M8 3v4M16 3v4"/>'
                '<circle cx="8.5" cy="14.5" r="1" fill="currentColor" stroke="none"/>'
                '<circle cx="12.5" cy="14.5" r="1" fill="currentColor" stroke="none"/>',
    "hourglass": '<path d="M7 3h10M7 21h10"/><path d="M8 3v4.2c0 2.2 4 3.9 4 4.8 0 .9-4 2.6-4 4.8V21"/>'
                 '<path d="M16 3v4.2c0 2.2-4 3.9-4 4.8 0 .9 4 2.6 4 4.8V21"/>',
    "users": '<circle cx="9" cy="8" r="3.4"/><path d="M2.8 20c.5-3.6 3.2-6 6.2-6s5.7 2.4 6.2 6"/>'
             '<path d="M16.5 5.2a3.4 3.4 0 0 1 0 6.6"/><path d="M18 20h3.2c-.3-2.4-1.5-4.3-3.3-5.3"/>',
    "userCheck": '<circle cx="10" cy="7.5" r="3.6"/><path d="M3.5 20c.5-4 3.3-6.6 6.5-6.6 1.6 0 3.1.6 4.3 1.7"/>'
                 '<path d="M15.2 18.4l2.2 2.2 4.4-4.6"/>',
    "sparkles": '<path d="M12 3.5c.9 3.6 1.9 4.6 5.5 5.5-3.6.9-4.6 1.9-5.5 5.5-.9-3.6-1.9-4.6-5.5-5.5 3.6-.9 4.6-1.9 5.5-5.5Z"/>'
                '<path d="M18.5 15c.45 1.8.95 2.3 2.75 2.75-1.8.45-2.3.95-2.75 2.75-.45-1.8-.95-2.3-2.75-2.75 1.8-.45 2.3-.95 2.75-2.75Z"/>'
                '<path d="M6 16.5c.3 1.2.65 1.55 1.85 1.85-1.2.3-1.55.65-1.85 1.85-.3-1.2-.65-1.55-1.85-1.85 1.2-.3 1.55-.65 1.85-1.85Z"/>',
    "image": '<rect x="3" y="4" width="18" height="16" rx="3"/><circle cx="8.6" cy="9.6" r="1.7"/>'
             '<path d="m4 17.5 4.6-4.6c.7-.7 1.8-.7 2.5 0l3 3c.7.7 1.8.7 2.5 0L20 12.5"/>',
    "settings": '<circle cx="12" cy="12" r="3.2"/><path d="M19.6 14.4a1.7 1.7 0 0 0 .34 1.87l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.7 1.7 0 0 0-1.87-.34 1.7 1.7 0 0 0-1.03 1.56V22a2 2 0 1 1-4 0v-.11a1.7 1.7 0 0 0-1.11-1.56 1.7 1.7 0 0 0-1.87.34l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.7 1.7 0 0 0 .34-1.87 1.7 1.7 0 0 0-1.56-1.03H8a2 2 0 1 1 0-4h.11A1.7 1.7 0 0 0 9.67 9.8a1.7 1.7 0 0 0-.34-1.87l-.06-.06A2 2 0 1 1 12.1 5.04l.06.06a1.7 1.7 0 0 0 1.87.34H14a1.7 1.7 0 0 0 1.03-1.56V3.77a2 2 0 1 1 4 0v.11a1.7 1.7 0 0 0 1.03 1.56 1.7 1.7 0 0 0 1.87-.34l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.7 1.7 0 0 0-.34 1.87V10a1.7 1.7 0 0 0 1.56 1.03h.11a2 2 0 1 1 0 4h-.11a1.7 1.7 0 0 0-1.42 1.03Z" transform="translate(-3.2 -1.8) scale(0.92)"/>',
    "tag": '<path d="M20.6 13.4 13.4 20.6a2 2 0 0 1-2.83 0l-7.2-7.2A2 2 0 0 1 2.8 12V4.8A2 2 0 0 1 4.8 2.8H12a2 2 0 0 1 1.41.58l7.2 7.2a2 2 0 0 1 0 2.83Z"/>'
           '<circle cx="7.9" cy="7.9" r="1.4"/>',
    "megaphone": '<path d="M3.5 10.2v3.6a1.8 1.8 0 0 0 1.8 1.8h1.4l7.6 4.2V4.2L6.7 8.4H5.3a1.8 1.8 0 0 0-1.8 1.8Z"/>'
                 '<path d="M18 8.4a5 5 0 0 1 0 7.2"/><path d="M7.5 15.6V19a1.6 1.6 0 0 0 3.2 0v-1.6"/>',
    "bell": '<path d="M18 8.4a6 6 0 1 0-12 0c0 5.2-2 6.6-2 6.6h16s-2-1.4-2-6.6Z"/><path d="M13.7 19a2 2 0 0 1-3.4 0"/>',
    "user": '<circle cx="12" cy="7.8" r="3.8"/><path d="M4.6 20.2c.6-4.2 3.6-7 7.4-7s6.8 2.8 7.4 7"/>',
    "heart": '<path d="M12 20.3S3.6 15.4 3.6 9.6a4.6 4.6 0 0 1 8.4-2.7 4.6 4.6 0 0 1 8.4 2.7c0 5.8-8.4 10.7-8.4 10.7Z"/>',
    "logout": '<path d="M9.5 21H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h3.5"/><path d="m15 16.5 4.5-4.5L15 7.5"/><path d="M19.5 12H9"/>',
    "chevron": '<path d="m14.5 6-6 6 6 6"/>',
    "caret": '<path d="m6 9.5 6 6 6-6"/>',
}


def svg(name):
    return ('<svg class="pnav-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" '
            'stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'
            f'{I[name]}</svg>')


GROUPS = [
    ("مدیریت کلان سالن", [
        ("dashboard", "داشبورد و شاخص‌ها", True, 0),
        ("layers", "اطلاعات پایه", False, 0),
    ]),
    ("نوبت‌دهی و مراجعین", [
        ("calendar", "مدیریت نوبت‌ها", False, 3),
        ("hourglass", "لیست انتظار", False, 0),
        ("users", "پرونده مشتریان", False, 0),
    ]),
    ("تیم و خدمات", [
        ("userCheck", "پرسنل سالن", False, 0),
        ("sparkles", "خدمات و تعرفه‌ها", False, 0),
        ("image", "گالری نمونه‌کارها", False, 0),
    ]),
    ("تنظیمات و تعاملات", [
        ("settings", "تنظیمات سالن و تم", False, 0),
        ("tag", "کدهای تخفیف", False, 0),
        ("megaphone", "اخبار و وبلاگ", False, 0),
        ("bell", "اعلانات سیستم", False, 5),
    ]),
]


def items_html(rail, prefix):
    out = []
    for gi, (title, items) in enumerate(GROUPS):
        out.append('<li class="pnav-group is-open">')
        out.append(
            f'<button type="button" class="pnav-group-head">'
            f'<span class="pnav-group-title">{title}</span>'
            f'<span class="pnav-group-caret">{svg("caret")}</span></button>'
        )
        out.append(f'<ul class="pnav-group-items" id="{prefix}-g{gi}">')
        for icon, label, active, badge in items:
            cls = "pnav-item is-active" if active else "pnav-item"
            cur = ' aria-current="page"' if active else ""
            badge_html = ""
            if badge:
                fa = str(badge).translate(str.maketrans("0123456789", "۰۱۲۳۴۵۶۷۸۹"))
                badge_html = f'<span class="pnav-badge" aria-hidden="true">{fa}</span>'
            out.append(
                f'<li class="pnav-row"><a class="{cls}" href="#"{cur} title="{label}">'
                f'<span class="pnav-icon" aria-hidden="true">{svg(icon)}</span>'
                f'<span class="pnav-label">{label}</span>{badge_html}</a></li>'
            )
        out.append('</ul></li>')
    logout = (
        '<div class="pnav-foot"><button type="button" class="pnav-item pnav-item--logout">'
        f'<span class="pnav-icon" aria-hidden="true">{svg("logout")}</span>'
        '<span class="pnav-label">خروج از حساب</span></button></div>'
    )
    return "\n".join(out) + logout


def capsule(state, prefix, aria, content=None):
    """پوسته کامل Sidebar + پنل محتوا.

    content: اگر داده شود، جای محتوای نمایشی پیش‌فرض می‌نشیند. ابزار ساخت
    پیش‌نمایش داشبورد از همین قلاب استفاده می‌کند تا Sidebar هرگز در دو
    فایل جدا تکرار نشود."""
    rail = state == "rail"
    if content is None:
        content = PANEL_DEMO
    return f'''
<section class="stage">
  <h2 class="stage-title">{aria}</h2>
  <div class="panel-shell--capsule stage-shell">
    <nav class="panel-sidebar is-{state}" aria-label="ناوبری پنل کاربری — {aria}" data-state="{state}">
      <div class="pnav-capsule">
        <div class="pnav-bridge" aria-hidden="true"></div>
        <div class="pnav-brand">
          <a class="pnav-logo" href="#" aria-label="سالن زیبایی حدیث">
            <span class="pnav-logo-fallback" aria-hidden="true">HB</span>
          </a>
          <button type="button" class="pnav-toggle" aria-expanded="{'false' if rail else 'true'}"
                  aria-controls="{prefix}-list" title="{'باز کردن منو' if rail else 'جمع کردن منو'}">
            {svg("chevron")}
          </button>
        </div>
        <ul class="pnav-list" id="{prefix}-list">
          {items_html(rail, prefix)}
        </ul>
      </div>
    </nav>
    <div class="panel-content">
{content}
    </div>
  </div>
</section>'''


# محتوای نمایشی پیش‌فرض پنل — همان چیزی که پیش‌نمایش Sidebar نشان می‌دهد
PANEL_DEMO = """      <div class="pv-content">
        <p class="pv-crumb">پنل مدیریت / داشبورد</p>
        <h3 class="pv-h">نوبت‌های امروز</h3>
        <div class="pv-grid">
          <div class="pv-card"><span>نوبت فعال</span><strong>۱۲</strong></div>
          <div class="pv-card"><span>در انتظار</span><strong>۴</strong></div>
          <div class="pv-card"><span>لغوشده</span><strong>۲</strong></div>
        </div>
        <p class="pv-line">این پنل محتوا فقط برای نمایش دقیق محل اتصال قرص فعال است.
        عرض و گردی گوشه‌ها از همان متغیرهای CSS خودِ اپ می‌آید.</p>
      </div>"""


def build():
    with io.open(CSS_SRC, encoding="utf-8") as f:
        css = f.read()

    # نگاشت متغیرها به مقادیر واقعی app.css چون فایل جداگانه است
    css = css.replace("var(--font-brand, 'Playfair Display', Georgia, serif)",
                      "'Playfair Display', Georgia, serif")

    # اسکریپت واقعی اپ درون‌خطی می‌شود تا پیش‌نمایش با همان موتور پل اتصال
    # ساخته شود. اگر کد JS عوض شود، پیش‌نمایش هم خودبه‌خود عوض می‌شود.
    with io.open(JS_SRC, encoding="utf-8") as f:
        js = f.read()

    page = f'''<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>پیش‌نمایش Sidebar کپسولی — SolonBeauty</title>
<style>
{css}

/* ---------- چیدمان خودِ صفحه پیش‌نمایش (بخشی از اپ نیست) ---------- */
body {{
    margin: 0;
    background: var(--sb-page-bg);
    background-attachment: fixed;
    font-family: 'Vazirmatn', Tahoma, system-ui, sans-serif;
    color: #EDE6DA;
}}
.stage-wrap {{
    display: flex;
    flex-wrap: wrap;
    overflow-x: auto;
    gap: 48px;
    align-items: flex-start;
    padding: 40px 32px 64px;
}}
.stage {{ margin: 0; }}
.stage-title {{
    font-size: .8rem;
    font-weight: 600;
    letter-spacing: .06em;
    color: rgba(237,230,218,.55);
    margin: 0 0 14px;
}}
/* ---------- بازنویسی‌های مخصوص پیش‌نمایش ----------
   در iframe، عرض ممکن است کمتر از ۱۰۲۴px باشد و قاعده موبایل‌اول اپ
   کپسول را پنهان می‌کند. اینجا چیدمان دسکتاپ به‌زور روشن می‌شود تا
   پیش‌نمایش در هر عرضی قابل مشاهده باشد. هیچ‌کدام از این قواعد در اپ
   وجود ندارند. */
.stage-shell {{
    display: grid;
    grid-template-columns: auto minmax(0, 1fr);
    gap: var(--sb-gap);
    padding: 0;
    min-height: 0;
    background: none;
    background-attachment: scroll;
    /* عرض ثابت تا هندسه‌ی پل قطعی و قابل پیش‌بینی باشد و به عرض iframe
       وابسته نشود. */
    inline-size: 900px;
    max-inline-size: none;
    overflow: visible;
}}
.stage-shell::before {{ content: none; }}
.stage .panel-sidebar {{ display: block; position: static; }}
/* کپسول باید relative بماند تا پل اتصال نسبت به خودش موقعیت بگیرد،
   نه نسبت به کل صحنه. */
.stage .pnav-capsule {{ position: relative; block-size: 720px; }}
.stage .panel-content {{ block-size: 720px; }}

/* ---------- محتوای نمایشی داخل پنل ---------- */
.pv-content {{ padding: 26px; }}
.pv-crumb {{
    margin: 0 0 6px; font-size: .74rem; letter-spacing: .04em;
    color: rgba(42,36,32,.5);
}}
.pv-h {{ margin: 0 0 18px; font-size: 1.1rem; color: #2A2420; font-weight: 700; }}
.pv-grid {{ display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 18px; }}
.pv-card {{
    display: flex; flex-direction: column; gap: 8px; padding: 14px;
    border-radius: 16px; background: rgba(42,36,32,.05);
    border: 1px solid rgba(42,36,32,.08);
}}
.pv-card span {{ font-size: .74rem; color: rgba(42,36,32,.6); }}
.pv-card strong {{ font-size: 1.4rem; color: #2A2420; }}
.pv-line {{ margin: 0; font-size: .8rem; line-height: 1.9; color: rgba(42,36,32,.62); }}
.note {{
    max-width: 720px;
    margin: 0 auto 8px;
    padding: 0 32px;
    font-size: .82rem;
    line-height: 2;
    color: rgba(237,230,218,.62);
}}
.note strong {{ color: #C6A46A; font-weight: 600; }}
</style>
</head>
<body>
<div class="note">
    <p><strong>پیش‌نمایش مرحله ۴</strong> — قرص فعال از Sidebar بیرون می‌زند،
    از روی فاصله می‌گذرد و با دو <strong>گوشه مقعر</strong> به پنل محتوا جوش می‌خورد.
    در هر دو حالت باز و بسته اعمال می‌شود.</p>
    <p>CSS و JS این فایل از فایل‌های واقعی اپ درون‌خطی شده‌اند
    (<code>panel-sidebar.css</code> و <code>mobile-ux.js</code>)، پس هرگز از کد اپ
    جدا نمی‌افتند. بازسازی با:</p>
    <p><code>python3 tools/build-sidebar-preview.py</code></p>
</div>
<div class="stage-wrap">
{capsule("expanded", "pv2", "حالت باز (Expanded) — جوش کامل به پنل")}
{capsule("rail", "pv1", "حالت بسته (Rail) — همان جوش، در عرض کم")}
</div>
<script>
{js}
</script>
<script>
// همان تابعی که کامپوننت PanelSidebar پس از هر رندر صدا می‌زند
if (window.solonUx && window.solonUx.initActiveBridge) {{
    window.solonUx.initActiveBridge();
}}
</script>
</body>
</html>
'''
    with io.open(OUT, "w", encoding="utf-8") as f:
        f.write(page)
    size = os.path.getsize(OUT)
    print(f"ساخته شد: {OUT}  ({size // 1024} KB)")


if __name__ == "__main__":
    build()
