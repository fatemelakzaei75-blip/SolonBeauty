#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سازنده پیش‌نمایش مرحله ۵ — دراور و نوار ناوبری پایین موبایل.

از همان CSS و JS واقعی پروژه ساخته می‌شود تا پیش‌نمایش هرگز از کد اپ جدا
نیفتد. دکمه «منو» و دکمه بستن دراور واقعاً کار می‌کنند، چون همان منطق
سمت اپ درون فایل درون‌خطی شده است.

اجرا:  python3 tools/build-step5-preview.py
خروجی: step5-preview.html
"""
import importlib.util
import io
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)

_spec = importlib.util.spec_from_file_location(
    "build_sidebar_preview", os.path.join(HERE, "build-sidebar-preview.py"))
bs = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(bs)

CSS_SIDEBAR = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "css", "panel-sidebar.css")
CSS_DASH = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "css", "panel-dashboard.css")
JS_SRC = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "js", "mobile-ux.js")
OUT = os.path.join(ROOT, "step5-preview.html")

FA = "۰۱۲۳۴۵۶۷۸۹"


def fa(n):
    return "".join(FA[int(c)] if c.isdigit() else c for c in f"{n:,}")


# ─────────────── آیتم‌های منو — عیناً همان PanelNavConfig ───────────────
# (href, عنوان، ایموجی‌نمای، آیکون واقعی، badge)
ADMIN_GROUPS = [
    ("مدیریت کلان سالن", [
        ("panel/admin", "داشبورد و شاخص‌ها", "dashboard", 0),
        ("panel/admin/base-data", "اطلاعات پایه", "layers", 0),
    ]),
    ("نوبت‌دهی و مراجعین", [
        ("panel/admin/data/reservation", "مدیریت نوبت‌ها", "calendar", 0),
        ("panel/admin/data/waiting-list", "لیست انتظار", "hourglass", 2),
        ("panel/admin/data/customer", "پرونده مشتریان", "users", 0),
    ]),
    ("تیم و خدمات", [
        ("panel/admin/data/personal", "پرسنل سالن", "userCheck", 0),
        ("panel/admin/data/salon-service", "خدمات و تعرفه‌ها", "sparkles", 0),
        ("panel/admin/data/portfolio", "گالری نمونه‌کارها", "image", 0),
    ]),
    ("تنظیمات و تعاملات", [
        ("panel/admin/data/salon-setting", "تنظیمات سالن و تم", "settings", 0),
        ("panel/admin/data/discount", "کدهای تخفیف", "tag", 0),
        ("panel/admin/data/news-day", "اخبار و وبلاگ", "megaphone", 0),
        ("panel/admin/data/notification", "اعلانات سیستم", "bell", 3),
    ]),
]

# چهار بخش پرکاربرد که InBottomNav = true دارند
BOTTOM = [
    ("panel/admin", "شاخص‌ها", "dashboard", 0, True),
    ("panel/admin/data/reservation", "نوبت‌ها", "calendar", 0, False),
    ("panel/admin/base-data", "پایه‌ها", "layers", 0, False),
    ("panel/admin/data/notification", "اعلانات", "bell", 3, False),
]

# آیکون‌های واقعی — همان مسیرهای PanelNavIcons.razor
ICONS = {
    "dashboard": '<path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/>',
    "layers": '<path d="M12 3 2.5 8 12 13l9.5-5L12 3Z"/><path d="M2.5 13 12 18l9.5-5"/>'
              '<path d="M2.5 17.5 12 22.5l9.5-5"/>',
    "calendar": '<rect x="3" y="5" width="18" height="16" rx="3"/><path d="M3 10h18M8 3v4M16 3v4"/>',
    "hourglass": '<path d="M7 3h10M7 21h10"/>'
                 '<path d="M8 3v4.2c0 2.2 4 3.9 4 4.8 0 .9-4 2.6-4 4.8V21"/>'
                 '<path d="M16 3v4.2c0 2.2-4 3.9-4 4.8 0 .9 4 2.6 4 4.8V21"/>',
    "users": '<circle cx="9" cy="8" r="3.4"/><path d="M2.8 20c.5-3.6 3.2-6 6.2-6s5.7 2.4 6.2 6"/>'
             '<path d="M16.5 5.2a3.4 3.4 0 0 1 0 6.6"/><path d="M18 20h3.2c-.3-2.4-1.5-4.3-3.3-5.3"/>',
    "userCheck": '<circle cx="10" cy="7.5" r="3.6"/>'
                 '<path d="M3.5 20c.5-4 3.3-6.6 6.5-6.6 1.6 0 3.1.6 4.3 1.7"/>'
                 '<path d="M15.2 18.4l2.2 2.2 4.4-4.6"/>',
    "sparkles": '<path d="M12 3.5c.9 3.6 1.9 4.6 5.5 5.5-3.6.9-4.6 1.9-5.5 5.5-.9-3.6-1.9-4.6-5.5-5.5'
                ' 3.6-.9 4.6-1.9 5.5-5.5Z"/>'
                '<path d="M18.5 15c.45 1.8.95 2.3 2.75 2.75-1.8.45-2.3.95-2.75 2.75-.45-1.8-.95-2.3-2.75-2.75'
                ' 1.8-.45 2.3-.95 2.75-2.75Z"/>',
    "image": '<rect x="3" y="4" width="18" height="16" rx="3"/><circle cx="8.6" cy="9.6" r="1.7"/>'
             '<path d="m4 17.5 4.6-4.6c.7-.7 1.8-.7 2.5 0l3 3c.7.7 1.8.7 2.5 0L20 12.5"/>',
    "settings": '<circle cx="12" cy="12" r="3.2"/><path d="M12 3v2M12 19v2M3 12h2M19 12h2'
                'M5.6 5.6l1.4 1.4M17 17l1.4 1.4M18.4 5.6 17 7M7 17l-1.4 1.4"/>',
    "tag": '<path d="M20.6 13.4 13.4 20.6a2 2 0 0 1-2.83 0l-7.2-7.2A2 2 0 0 1 2.8 12V4.8'
           'A2 2 0 0 1 4.8 2.8H12a2 2 0 0 1 1.41.58l7.2 7.2a2 2 0 0 1 0 2.83Z"/>'
           '<circle cx="7.9" cy="7.9" r="1.4"/>',
    "megaphone": '<path d="M3 11v2a1 1 0 0 0 1 1h2l4 3.5V6.5L6 10H4a1 1 0 0 0-1 1Z"/>'
                 '<path d="M13 7.5c2.2.8 3.5 2.5 3.5 4.5s-1.3 3.7-3.5 4.5"/>'
                 '<path d="M17.5 5.5c3 1.3 4.8 3.8 4.8 6.5s-1.8 5.2-4.8 6.5"/>',
    "bell": '<path d="M18 8a6 6 0 1 0-12 0c0 6-2 7-2 7h16s-2-1-2-7"/>'
            '<path d="M10.3 20a2 2 0 0 0 3.4 0"/>',
    "logout": '<path d="M9.5 21H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h3.5"/>'
              '<path d="m15 16.5 4.5-4.5L15 7.5"/><path d="M19.5 12H9"/>',
    "menu": '<path d="M4 7h16M4 12h16M4 17h16"/>',
    "close": '<path d="M6 6l12 12M18 6L6 18"/>',
}


def icon(name, size=24):
    return (f'<svg class="pnav-svg" width="{size}" height="{size}" viewBox="0 0 24 24" fill="none" '
            f'stroke="currentColor" stroke-width="1.7" stroke-linecap="round" '
            f'stroke-linejoin="round" aria-hidden="true">{ICONS[name]}</svg>')


def pill(href, label, icon_name, badge, active):
    b = (f'<span class="pnav-badge" aria-hidden="true">{fa(badge)}</span>'
         if badge else "")
    return f'''<li class="pnav-row">
          <a class="pnav-item {"is-active" if active else ""}" href="{href}"
             {'aria-current="page"' if active else ''}>
            <span class="pnav-icon" aria-hidden="true">{icon(icon_name)}</span>
            <span class="pnav-label">{label}</span>{b}
          </a>
        </li>'''


def drawer(active_href):
    groups = []
    for title, items in ADMIN_GROUPS:
        rows = "\n        ".join(
            pill(h, l, ic, bd, h == active_href) for h, l, ic, bd in items)
        groups.append(f'''    <div class="pnav-drawer-group">
      <span class="pnav-drawer-group-title">{title}</span>
      <ul class="pnav-group-items">
        {rows}
      </ul>
    </div>''')
    body = "\n".join(groups)
    return f'''<div class="pnav-scrim" data-scrim aria-hidden="true"></div>

<aside class="pnav-drawer" id="pv-drawer" role="dialog" aria-modal="true"
       aria-label="منوی پنل کاربری" aria-hidden="true" inert>

  <div class="pnav-drawer-head">
    <a class="pnav-logo" href="#" aria-label="سالن زیبایی حدیث">
      <span class="pnav-logo-fallback" aria-hidden="true">HB</span>
    </a>
    <span class="pnav-drawer-brand">
      <strong>سالن زیبایی حدیث</strong>
      <span>مدیر سالن</span>
    </span>
    <button type="button" class="pnav-drawer-close" data-close aria-label="بستن منو">
      {icon("close", 20)}
    </button>
  </div>

  <nav class="pnav-drawer-nav" aria-label="منوی پنل کاربری">
{body}
  </nav>

  <div class="pnav-foot">
    <button type="button" class="pnav-item pnav-item--logout">
      <span class="pnav-icon" aria-hidden="true">{icon("logout")}</span>
      <span class="pnav-label">خروج از حساب</span>
    </button>
  </div>
</aside>'''


def bottom_nav(menu_open=False):
    cells = []
    for href, label, ic, badge, active in BOTTOM:
        b = (f'<span class="pnav-bnav-badge">{fa(badge)}</span>' if badge else "")
        cells.append(f'''      <li class="pnav-bnav-cell">
        <a class="pnav-bnav-item {"is-active" if active else ""}" href="{href}"
           {'aria-current="page"' if active else ''}>
          <span class="pnav-bnav-icon" aria-hidden="true">{icon(ic, 22)}{b}</span>
          <span class="pnav-bnav-label">{label}</span>
        </a>
      </li>''')
    cells.append(f'''      <li class="pnav-bnav-cell">
        <button type="button" class="pnav-bnav-item {"is-active" if menu_open else ""}"
                data-toggle aria-label="باز کردن منوی کامل" aria-expanded="false"
                aria-controls="pv-drawer">
          <span class="pnav-bnav-icon" aria-hidden="true">
            <span data-icon-menu>{icon("menu", 22)}</span>
            <span data-icon-close hidden>{icon("close", 22)}</span>
          </span>
          <span class="pnav-bnav-label">منو</span>
        </button>
      </li>''')
    body = "\n".join(cells)
    return f'''<nav class="pnav-bnav" aria-label="ناوبری سریع پنل">
    <ul class="pnav-bnav-list">
{body}
    </ul>
  </nav>'''


# ─────────────── محتوای نمایشی پنل ───────────────
PANEL = '''      <div class="dash">
        <header class="dash-head">
          <div>
            <h1>داشبورد مدیریت سالن</h1>
            <p>چهارشنبه ۱۰ مهر ۱۴۰۵ · ساعت ۱۴:۲۰</p>
          </div>
        </header>

        <div class="dash-hero-row">
          <article class="dash-hero">
            <span class="dash-label">درآمد امروز</span>
            <strong class="dash-hero-num">۵,۲۴۰,۰۰۰<small>تومان</small></strong>
            <span class="dash-delta dash-delta--up">▲ ۲۳٪ بالاتر از چهارشنبه‌های این ماه</span>
          </article>
          <article class="dash-goal">
            <span class="dash-label">هدف مهر ماه</span>
            <div class="dash-goal-num">۸۱,۶۰۰,۰۰۰ <span>/ ۱۲۰,۰۰۰,۰۰۰ تومان</span></div>
            <div class="dash-progress" role="img" aria-label="پیشرفت ۶۸ درصد از هدف">
              <i style="inline-size:68%"></i>
              <span class="dash-progress-marker" style="inset-inline-start:63%"></span>
            </div>
            <div class="dash-goal-foot">
              <span><b>۶۸٪</b> تحقق‌یافته</span>
              <span>۱۱ روز مانده</span>
            </div>
          </article>
        </div>

        <div class="dash-kpis">
          <article class="dash-kpi">
            <div class="dash-kpi-top"><span class="dash-label">نوبت‌های امروز</span></div>
            <strong class="dash-kpi-val">۹<small>نوبت</small></strong>
            <span class="dash-kpi-sub">۳ نوبت انجام‌شده</span>
          </article>
          <article class="dash-kpi">
            <div class="dash-kpi-top"><span class="dash-label">میانگین سبد</span></div>
            <strong class="dash-kpi-val">۵۸۲,۰۰۰<small>تومان</small></strong>
            <span class="dash-kpi-sub">▲ ۸٪ نسبت به ماه قبل</span>
          </article>
        </div>

        <section class="dash-actions">
          <span class="dash-label">امروز به این موارد رسیدگی کن</span>
          <div class="dash-actions-row">
            <button type="button" class="dash-chip">در انتظار تأیید
              <span class="dash-chip-count">۳</span></button>
            <button type="button" class="dash-chip">پرداخت ناموفق
              <span class="dash-chip-count dash-chip-count--bad">۱</span></button>
            <button type="button" class="dash-chip">لیست انتظار
              <span class="dash-chip-count">۲</span></button>
          </div>
        </section>
      </div>'''


def phone(content_has_drawer, menu_open, title):
    return f'''<section class="stage">
  <h2 class="stage-title">{title}</h2>
  <div class="phone">
    <div class="phone-scroll">
{bottom_nav(menu_open)}
{PANEL}
    </div>
{content_has_drawer}
  </div>
</section>'''


def build():
    with io.open(CSS_SIDEBAR, encoding="utf-8") as f:
        css_sidebar = f.read()
    with io.open(CSS_DASH, encoding="utf-8") as f:
        css_dash = f.read()
    with io.open(JS_SRC, encoding="utf-8") as f:
        js = f.read()

    css_sidebar = css_sidebar.replace(
        "var(--font-brand, 'Playfair Display', Georgia, serif)",
        "'Playfair Display', Georgia, serif")

    page = f'''<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>مرحله ۵ — نوار ناوبری پایین و دراور | SolonBeauty</title>
<style>
{css_sidebar}

{css_dash}

/* ==========================================================================
   چیدمان خودِ صفحه پیش‌نمایش — بخشی از اپ نیست
   ========================================================================== */
body {{
    margin: 0;
    background: var(--sb-page-bg);
    background-attachment: fixed;
    font-family: 'Vazirmatn', Tahoma, 'Segoe UI', system-ui, sans-serif;
    color: #EDE6DA;
    -webkit-font-smoothing: antialiased;
}}

.note {{
    max-width: 820px; margin: 0 auto; padding: 34px 28px 8px;
    font-size: .82rem; line-height: 2; color: rgba(237,230,218,.68);
}}
.note h1 {{ margin: 0 0 10px; font-size: 1.05rem; color: #F7F2EA; font-weight: 800; }}
.note strong {{ color: #C6A46A; font-weight: 700; }}
.note code {{
    font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
    background: rgba(237,230,218,.08); padding: 1px 6px; border-radius: 5px; font-size: .92em;
}}
.note ul {{ margin: 8px 0 0; padding-inline-start: 20px; }}
.note li {{ margin-block-end: 5px; }}

.stage-wrap {{
    display: flex; flex-wrap: wrap; gap: 40px; align-items: flex-start;
    padding: 28px 28px 80px; overflow-x: auto;
}}
.stage {{ margin: 0; flex: 0 0 auto; }}
.stage-title {{
    font-size: .78rem; font-weight: 700; letter-spacing: .04em;
    color: rgba(237,230,218,.55); margin: 0 0 12px; text-align: center;
}}

/* قاب گوشی: عرض ۳۹۰px — اندازه یک موبایل استاندارد */
.phone {{
    position: relative;
    inline-size: 390px;
    block-size: 760px;
    border-radius: 30px;
    overflow: hidden;
    background: #F7F2EA;
    box-shadow: 0 26px 64px rgba(0,0,0,.5), 0 0 0 1px rgba(237,230,218,.09);
}}
.phone-scroll {{
    position: absolute;
    inset: 0;
    overflow-y: auto;
    overscroll-behavior: contain;
    /* جا برای کپسول شناور پایین — همان توکنی که در اپ هم استفاده می‌شود */
    padding-block-end: var(--sb-bnav-space);
}}
.phone-scroll .dash {{ padding: 16px; }}
/* در پیش‌نمایش، نوار و دراور نسبت به قاب گوشی موقعیت می‌گیرند نه کل صفحه */
.phone .pnav-bnav,
.phone .pnav-drawer,
.phone .pnav-scrim {{ position: absolute; }}
.phone .pnav-bnav {{ z-index: 20; }}
.phone .pnav-scrim {{ z-index: 30; }}
.phone .pnav-drawer {{ z-index: 40; visibility: visible; transform: none; }}
.phone .pnav-drawer:not(.is-open) {{ visibility: hidden; transform: translateX(var(--sb-drawer-off)); }}
.phone .pnav-drawer.is-open {{ visibility: visible; transform: none; }}

/* بیرون از قاب، دراور نسبت به پنجره است — رفتار واقعی اپ */
.stage--full {{ display: block; }}
.stage--full .stage-title {{ text-align: start; }}
</style>
</head>
<body>

<div class="note">
    <h1>مرحله ۵ — ناوبری موبایل: نوار پایین و دراور</h1>
    <p>
        این فایل از <strong>CSS و JS واقعی پروژه</strong> ساخته شده
        (<code>panel-sidebar.css</code> و <code>mobile-ux.js</code>).
        دکمه «منو» و دکمه بستن دراور <strong>واقعاً کار می‌کنند</strong>، چون همان
        منطق سمت اپ درون فایل درون‌خطی شده است: Escape، تله فوکوس، و برگشت فوکوس
        به دکمه قبلی.
    </p>
    <p><strong>چه چیزی را ببینید:</strong></p>
    <ul>
        <li>نوار پایین یک <strong>کپسول شناور تیره</strong> است، نه نوار سفید چسبیده به لبه.
        دلیل: نوار سفید امضای «سایت» است و کپسول شناور امضای «اپ». همچنین همان گرادیان و
        قاب طلایی Sidebar را تکرار می‌کند تا موبایل و دسکتاپ یک محصول واحد حس شوند.</li>
        <li>دراور از <strong>سمت راست</strong> می‌آید — همان سمت Sidebar — و ظاهر قرص‌های
        کپسول را عیناً بازاستفاده می‌کند.</li>
        <li>گروه‌های منو در دراور <strong>آکاردئون نیستند</strong> و همه باز هستند.
        در دراور، اسکرول کردن ارزان‌تر از باز کردن چند آکاردئون است.</li>
        <li>عدد اعلان روی گوشه آیکون نوار پایین می‌نشیند و معنایش در
        <code>aria-label</code> آمده، پس صفحه‌خوان هم آن را می‌شنود.</li>
    </ul>
</div>

<div class="stage-wrap">
{phone(drawer("panel/admin"), False, "موبایل ۳۹۰px — حالت عادی")}
{phone(drawer("panel/admin/data/reservation"), True, "موبایل ۳۹۰px — دراور باز")}
</div>

<script>
{js}
</script>
<script>
/* شبیه‌ساز کوچک رفتار اپ: در اپ، وضعیت دراور در Blazor است و JS فقط خبر
   می‌دهد. اینجا وضعیت در DOM نگه داشته می‌شود تا پیش‌نمایش تعاملی باشد. */
(function () {{
    var FA = '۰۱۲۳۴۵۶۷۸۹';
    function toFa(s) {{ return String(s).replace(/\\d/g, function (d) {{ return FA[+d]; }}); }}

    var frames = document.querySelectorAll('.phone');

    Array.prototype.forEach.call(frames, function (frame) {{
        var drawer = frame.querySelector('.pnav-drawer');
        var scrim = frame.querySelector('[data-scrim]');
        var toggle = frame.querySelector('[data-toggle]');

        function setOpen(open) {{
            if (!drawer) return;
            drawer.classList.toggle('is-open', open);
            drawer.setAttribute('aria-hidden', open ? 'false' : 'true');
            if (open) {{ drawer.removeAttribute('inert'); }} else {{ drawer.setAttribute('inert', ''); }}
            if (scrim) scrim.hidden = !open;
            if (toggle) {{
                toggle.classList.toggle('is-active', open);
                toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
                var m = toggle.querySelector('[data-icon-menu]');
                var c = toggle.querySelector('[data-icon-close]');
                if (m) m.hidden = open;
                if (c) c.hidden = !open;
            }}
            if (scrim) scrim.style.display = open ? '' : 'none';
        }}

        if (toggle) toggle.addEventListener('click', function () {{
            setOpen(!drawer.classList.contains('is-open'));
        }});
        if (scrim) scrim.addEventListener('click', function () {{ setOpen(false); }});
        var close = frame.querySelector('[data-close]');
        if (close) close.addEventListener('click', function () {{ setOpen(false); }});
        Array.prototype.forEach.call(frame.querySelectorAll('.pnav-drawer .pnav-item'), function (a) {{
            a.addEventListener('click', function (e) {{ e.preventDefault(); setOpen(false); }});
        }});

        setOpen(drawer.classList.contains('is-open'));
    }});
}})();
</script>
</body>
</html>
'''
    with io.open(OUT, "w", encoding="utf-8") as f:
        f.write(page)
    print(f"ساخته شد: {OUT}  ({os.path.getsize(OUT) // 1024} KB)")


if __name__ == "__main__":
    build()
