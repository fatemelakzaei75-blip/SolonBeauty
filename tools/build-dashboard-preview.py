#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سازنده پیش‌نمایش داشبورد مدیریتی.

CSS و JS را از فایل‌های واقعی پروژه می‌خواند و درون HTML جای می‌دهد تا
پیش‌نمایش هرگز از کد اصلی جدا نیفتد. Sidebar از ابزار پیش‌نمایش قبلی
وارد می‌شود تا مارک‌آپ آن در دو فایل تکرار نشود.

اجرا:  python3 tools/build-dashboard-preview.py
خروجی: dashboard-preview.html
"""
import importlib.util
import io
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)

# ابزار پیش‌نمایش Sidebar را به‌عنوان ماژول بارگذاری کن. کد اجرایی آن زیر
# گارد __main__ است، پس import بی‌خطر است.
_spec = importlib.util.spec_from_file_location(
    "build_sidebar_preview", os.path.join(HERE, "build-sidebar-preview.py"))
bs = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(bs)

CSS_SIDEBAR = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "css", "panel-sidebar.css")
CSS_DASH = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "css", "panel-dashboard.css")
JS_SRC = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "js", "mobile-ux.js")
OUT = os.path.join(ROOT, "dashboard-preview.html")

# ─────────────────────────── داده نمایشی ───────────────────────────
# اعداد واقع‌گرایانه برای یک سالن زیبایی: نه آن‌قدر کوچک که بی‌اهمیت باشد،
# نه آن‌قدر بزرگ که باورنکردنی شود.

FA_DIGITS = "۰۱۲۳۴۵۶۷۸۹"


def fa(n):
    """تبدیل عدد به ارقام فارسی با جداکننده هزارگان — مثل اپ."""
    return "".join(FA_DIGITS[int(c)] if c.isdigit() else c
                   for c in f"{n:,}")


REVENUE_TODAY = 5_240_000
REVENUE_AVG_WED = 4_260_000
GOAL_MONTH = 120_000_000
GOAL_DONE = 81_600_000
SPARK = [3_200_000, 4_100_000, 2_600_000, 5_300_000, 4_800_000, 6_100_000, REVENUE_TODAY]

APPOINTMENTS = [
    ("۰۹:۳۰", "خانم احمدی", "رنگ و مش", "انجام شد", "done", "۲,۴۰۰,۰۰۰"),
    ("۱۰:۴۵", "خانم نوری", "کراتین و احیا", "انجام شد", "done", "۳,۱۰۰,۰۰۰"),
    ("۱۲:۰۰", "خانم صادقی", "پاکسازی پوست", "انجام شد", "done", "۱,۲۰۰,۰۰۰"),
    ("۱۳:۱۵", "خانم رحیمی", "مانیکور و پدیکور", "در حال انجام", "now", "۸۵۰,۰۰۰"),
    ("۱۴:۳۰", "خانم کاظمی", "رنگ و مش", "تأیید شده", "next", "۲,۴۰۰,۰۰۰"),
    ("۱۵:۴۵", "خانم یوسفی", "براشینگ و شنیون", "تأیید شده", "next", "۷۵۰,۰۰۰"),
    ("۱۷:۰۰", "خانم شریفی", "کراتین و احیا", "در انتظار تأیید", "wait", "۳,۱۰۰,۰۰۰"),
    ("۱۸:۱۵", "خانم مرادی", "پاکسازی پوست", "تأیید شده", "next", "۱,۲۰۰,۰۰۰"),
    ("۱۹:۳۰", "خانم تقوی", "مانیکور و پدیکور", "پرداخت ناموفق", "wait", "۸۵۰,۰۰۰"),
]

# روزها به ترتیب هفته ایرانی. جمعه تعطیل است.
HEAT_DAYS = ["شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه"]
HEAT_HOURS = ["۹", "۱۰", "۱۱", "۱۲", "۱۳", "۱۴", "۱۵", "۱۶", "۱۷", "۱۸"]
HEAT = [
    [2, 3, 4, 5, 5, 4, 3, 4, 3, 2],
    [1, 2, 3, 4, 4, 3, 2, 3, 2, 1],
    [2, 2, 3, 4, 5, 4, 3, 3, 2, 2],
    [1, 2, 3, 3, 4, 3, 3, 2, 2, 1],
    [3, 4, 5, 5, 5, 4, 4, 3, 3, 2],
    [4, 5, 5, 5, 4, 3, 2, 1, 1, 0],
    None,  # جمعه
]

STAFF = [
    ("ر", "خانم رضایی", 24_800_000, 82, 78),
    ("م", "خانم موسوی", 18_200_000, 71, 65),
    ("ک", "خانم کریمی", 15_600_000, 64, 59),
    ("ا", "خانم احمدی", 12_400_000, 53, 48),
]

SERVICES = [
    ("رنگ و مش", 12, 34),
    ("کراتین و احیا", 9, 26),
    ("پاکسازی پوست", 8, 22),
    ("مانیکور و پدیکور", 7, 19),
    ("براشینگ و شنیون", 5, 14),
]

PAYMENTS = [
    ("کارت‌خوان سالن", 41, "var(--d-gold-deep)"),
    ("نقدی", 32, "rgba(198,164,106,.62)"),
    ("درگاه آنلاین", 22, "rgba(198,164,106,.34)"),
    ("کارت به کارت", 5, "rgba(42,36,32,.16)"),
]

RISK = [
    ("خانم عطایی", "۵۲ روز از آخرین مراجعه", False),
    ("خانم بهرامی", "۴۸ روز", False),
    ("خانم زمانی", "۴۱ روز", True),
]


# ─────────────────────────── سازنده بخش‌ها ───────────────────────────
def sparkline(values):
    """نمودار خطی کوچک — SVG درون‌خطی، بدون کتابخانه نمودار."""
    w, h, pad = 300, 46, 4
    lo, hi = min(values), max(values)
    span = (hi - lo) or 1
    pts = []
    for i, v in enumerate(values):
        x = pad + i * (w - 2 * pad) / (len(values) - 1)
        y = h - pad - (v - lo) / span * (h - 2 * pad)
        pts.append((x, y))
    line = " ".join(f"{x:.1f},{y:.1f}" for x, y in pts)
    area = f"{pad},{h - pad} " + line + f" {w - pad},{h - pad}"
    return f"""<svg class="dash-spark" viewBox="0 0 {w} {h}" preserveAspectRatio="none"
     role="img" aria-label="روند درآمد هفت روز گذشته">
  <defs><linearGradient id="sparkFill" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0%" stop-color="#C6A46A" stop-opacity=".38"/>
    <stop offset="100%" stop-color="#C6A46A" stop-opacity="0"/>
  </linearGradient></defs>
  <polygon points="{area}" fill="url(#sparkFill)"/>
  <polyline points="{line}" fill="none" stroke="#C6A46A" stroke-width="2"
            stroke-linecap="round" stroke-linejoin="round"/>
</svg>"""


def hero():
    delta = round((REVENUE_TODAY / REVENUE_AVG_WED - 1) * 100)
    return f"""      <article class="dash-hero dash-rise">
        <span class="dash-label">درآمد امروز</span>
        <strong class="dash-hero-num"><span data-count="{REVENUE_TODAY}">{fa(REVENUE_TODAY)}</span><small>تومان</small></strong>
        <span class="dash-delta dash-delta--up">▲ {fa(delta)}٪ بالاتر از چهارشنبه‌های این ماه</span>
        {sparkline(SPARK)}
      </article>"""


def goal():
    pct = round(GOAL_DONE / GOAL_MONTH * 100)
    # روز ۱۹ از ۳۰ ⇒ انتظار ۶۳٪ پیشرفت. ما ۶۸٪ هستیم: جلوتر از تقویم.
    return f"""      <article class="dash-goal dash-rise">
        <span class="dash-label">هدف مهر ماه</span>
        <div class="dash-goal-num">{fa(GOAL_DONE)} <span>/ {fa(GOAL_MONTH)} تومان</span></div>
        <div class="dash-progress" role="img" aria-label="پیشرفت {fa(pct)} درصد از هدف">
          <i style="inline-size:{pct}%"></i>
          <span class="dash-progress-marker" style="inset-inline-start:63%"></span>
        </div>
        <div class="dash-goal-foot">
          <span><b>{fa(pct)}٪</b> تحقق‌یافته</span>
          <span>۱۱ روز مانده</span>
          <span>پیش‌بینی پایان ماه: <b>۱۲۸ میلیون</b></span>
        </div>
      </article>"""


def icon(name):
    """آیکون‌های مخصوص داشبورد — شبکه ۲۴×۲۴، ضخامت ۱.۷ مثل بقیه پنل."""
    paths = {
        "calendar": '<rect x="3" y="5" width="18" height="16" rx="3"/><path d="M3 10h18M8 3v4M16 3v4"/>',
        "wallet": '<rect x="2.5" y="6" width="19" height="13" rx="3"/><path d="M2.5 10h19"/>'
                  '<circle cx="17" cy="14.5" r="1.3"/>',
        "gauge": '<path d="M12 20a8 8 0 1 1 8-8"/><path d="M12 12l4.5-3"/>',
        "bell": '<path d="M18 8a6 6 0 1 0-12 0c0 6-2 7-2 7h16s-2-1-2-7"/>'
                '<path d="M10.3 20a2 2 0 0 0 3.4 0"/>',
        "bulb": '<path d="M9 18h6M10 21h4"/><path d="M12 3a6 6 0 0 0-3.5 10.9V16h7v-2.1A6 6 0 0 0 12 3Z"/>',
        "gift": '<rect x="3" y="8" width="18" height="13" rx="2"/><path d="M3 12h18M12 8v13"/>'
                '<path d="M12 8S10.5 3 8 3a2.5 2.5 0 0 0 0 5M12 8s1.5-5 4-5a2.5 2.5 0 0 1 0 5"/>',
        "star": '<path d="M12 3.6l2.6 5.3 5.9.8-4.3 4.1 1 5.8-5.2-2.7-5.2 2.7 1-5.8-4.3-4.1 5.9-.8Z"/>',
        "trend": '<path d="M3 17l6-6 4 4 8-8"/><path d="M15 7h6v6"/>',
    }
    return (f'<svg width="18" height="18" viewBox="0 0 24 24" fill="none" '
            f'stroke="currentColor" stroke-width="1.7" stroke-linecap="round" '
            f'stroke-linejoin="round" aria-hidden="true">{paths[name]}</svg>')


def kpis():
    # (آیکون، کلاس رنگی، عنوان، عدد خام، واحد، زیرنویس)
    # عدد خام می‌دهیم تا هم نمایش فارسی و هم انیمیشن شمارش از یک منبع بیاید.
    items = [
        ("calendar", "", "نوبت‌های امروز", 9, "نوبت", "۳ نوبت انجام‌شده"),
        ("wallet", "", "میانگین سبد خرید", 582_000, "تومان", "▲ ۸٪ نسبت به ماه قبل"),
        ("gauge", "dash-kpi-icon--warn", "نرخ اشغال امروز", 76, "٪", "۲ ساعت خالی تا پایان روز"),
        ("bell", "dash-kpi-icon--bad", "در انتظار اقدام", 5, "مورد", "۲ مورد فوری"),
    ]
    out = []
    for ic, mod, title, num, unit, sub in items:
        out.append(f"""        <article class="dash-kpi dash-rise">
          <div class="dash-kpi-top">
            <span class="dash-label">{title}</span>
            <span class="dash-kpi-icon {mod}">{icon(ic)}</span>
          </div>
          <strong class="dash-kpi-val"><span data-count="{num}">{fa(num)}</span><small>{unit}</small></strong>
          <span class="dash-kpi-sub">{sub}</span>
        </article>""")
    return "\n".join(out)


def action_queue():
    chips = [
        ("در انتظار تأیید", 3, ""),
        ("پرداخت ناموفق", 1, "dash-chip-count--bad"),
        ("درخواست لغو", 1, "dash-chip-count--warn"),
        ("لیست انتظار", 2, ""),
    ]
    html = "\n".join(
        f"""          <button type="button" class="dash-chip">{t}
            <span class="dash-chip-count {m}">{fa(n)}</span>
          </button>""" for t, n, m in chips)
    return f"""      <section class="dash-actions dash-rise" aria-label="کارهای در انتظار اقدام">
        <span class="dash-label">امروز به این موارد رسیدگی کن</span>
        <div class="dash-actions-row">
{html}
          <button type="button" class="dash-chip">دیدن همه موارد ›</button>
        </div>
      </section>"""


def timeline():
    cards = []
    for time, name, service, status, kind, price in APPOINTMENTS:
        cards.append(f"""        <article class="dash-appt dash-appt--{kind}">
          <div class="dash-appt-time"><span>{time}</span>
            <span class="dash-appt-badge">{status}</span></div>
          <span class="dash-appt-name">{name}</span>
          <span class="dash-appt-service">{service}</span>
          <div class="dash-appt-foot"><span>{price} تومان</span></div>
        </article>""")
    body = "\n".join(cards)
    return f"""      <section class="dash-card dash-rise">
        <div class="dash-card-head">
          <h2 class="dash-card-title">نوبت‌های امروز</h2>
          <a class="dash-card-link" href="#">مدیریت نوبت‌ها ›</a>
        </div>
        <div class="dash-timeline">
{body}
        </div>
      </section>"""


def heatmap():
    head_cells = "".join(f'<span class="heat-hour">{h}</span>' for h in HEAT_HOURS)
    rows = [f'<span class="heat-hour"></span>{head_cells}']
    for day, loads in zip(HEAT_DAYS, HEAT):
        cells = []
        if loads is None:
            for _ in HEAT_HOURS:
                cells.append('<span class="heat-cell heat-cell--closed"></span>')
        else:
            for v in loads:
                cells.append(f'<span class="heat-cell" data-load="{v}"></span>')
        rows.append(f'<span class="heat-day">{day}</span>' + "".join(cells))
    body = "\n".join("        " + r for r in rows)
    legend = "".join(
        f'<i style="background:rgba(198,164,106,{a})"></i>'
        for a in (".16", ".32", ".52", ".74", ".92"))
    return f"""      <section class="dash-card dash-span-2 dash-rise">
        <div class="dash-card-head">
          <h2 class="dash-card-title">نقشه شلوغی هفته</h2>
          <span class="dash-label">۵ هفته گذشته</span>
        </div>
        <div class="heat" role="img"
             aria-label="نقشه حرارتی شلوغی: چهارشنبه و پنجشنبه شلوغ‌ترین و صبح‌های یکشنبه خلوت‌ترین هستند">
{body}
        </div>
        <div class="heat-legend">
          <span>خلوت</span>{legend}<span>پر</span>
          <span style="margin-inline-start:auto">خانه‌های راه‌راه: تعطیل</span>
        </div>
        <div class="dash-insight">
          <span class="dash-insight-icon">{icon("bulb")}</span>
          <div class="dash-insight-body">
            <span class="dash-insight-title">فردا ۱۴:۰۰ تا ۱۷:۰۰ خالی است</span>
            <span class="dash-insight-text">
              این بازه در پنج هفته گذشته همیشه خلوت بوده. یک کمپین تخفیف
              محدود برای همین ساعت‌ها می‌تواند ظرفیت بی‌استفاده را
              به درآمد تبدیل کند.
            </span>
          </div>
        </div>
      </section>"""


def staff_card():
    rows = []
    for initial, name, revenue, occ, rebook in STAFF:
        rows.append(f"""        <div class="bar-row">
          <div class="bar-top">
            <span class="bar-name"><span class="bar-avatar">{initial}</span>{name}</span>
            <span class="bar-val">{fa(round(revenue / 1_000_000, 1)).replace('.', '٫')} <small>میلیون</small></span>
          </div>
          <div class="bar-track"><i class="bar-fill" style="inline-size:{round(revenue / 24_800_000 * 100)}%"></i></div>
          <div class="bar-sub">
            <span>اشغال {fa(occ)}٪</span>
            <span class="bar-track bar-track--thin" style="flex:1">
              <i class="bar-fill bar-fill--occ" style="inline-size:{occ}%"></i></span>
            <span>رزرو مجدد {fa(rebook)}٪</span>
          </div>
        </div>""")
    body = "\n".join(rows)
    return f"""      <section class="dash-card dash-rise">
        <div class="dash-card-head">
          <h2 class="dash-card-title">عملکرد پرسنل — این هفته</h2>
          <a class="dash-card-link" href="#">گزارش کامل ›</a>
        </div>
        <div class="bars">
{body}
        </div>
        <p class="dash-note">درآمد هر نفر، درصد اشغال وقت و نرخ رزرو مجدد مشتریانش.
        این سه عدد با هم نشان می‌دهند چه کسی مشتری را نگه می‌دارد.</p>
      </section>"""


def services_card():
    rows = []
    for i, (name, count, pct) in enumerate(SERVICES, 1):
        rows.append(f"""        <div class="rank-row">
          <span class="rank-no">{fa(i)}</span>
          <div class="rank-body">
            <span class="rank-name">{name}</span>
            <span class="rank-track"><i style="inline-size:{pct * 2.6}%"></i></span>
          </div>
          <span class="rank-val">{fa(count)} نوبت</span>
        </div>""")
    body = "\n".join(rows)
    return f"""      <section class="dash-card dash-rise">
        <div class="dash-card-head">
          <h2 class="dash-card-title">پرفروش‌ترین خدمات — این ماه</h2>
        </div>
        <div class="rank">
{body}
        </div>
      </section>"""


def customers_card():
    risk_rows = []
    for name, when, warn in RISK:
        dot = "risk-dot risk-dot--warn" if warn else "risk-dot"
        risk_rows.append(f"""          <div class="risk-item">
            <span class="{dot}"></span><b>{name}</b><span>{when}</span>
          </div>""")
    body = "\n".join(risk_rows)
    return f"""      <section class="dash-card dash-rise">
        <div class="dash-card-head">
          <h2 class="dash-card-title">سلامت مشتریان</h2>
          <a class="dash-card-link" href="#">گزارش ریزش ›</a>
        </div>
        <div class="cust-row">
          <div class="cust-box"><b>{fa(18)}</b><span>مشتری جدید</span></div>
          <div class="cust-box"><b>{fa(64)}</b><span>مشتری بازگشتی</span></div>
          <div class="cust-box cust-box--risk"><b>{fa(6)}</b><span>در خطر ریزش</span></div>
        </div>
        <div class="risk-list">
{body}
        </div>
        <p class="dash-note">این افراد بین ۴۰ تا ۶۰ روز مراجعه نکرده‌اند، در حالی که
        فاصله معمولشان ۳۰ روز بود. توجه: برای پیامک یادآوری، شماره موبایل
        دائمی مشتری در دیتابیس ذخیره نمی‌شود و باید اضافه شود.</p>
      </section>"""


def payment_card():
    legend = "\n".join(
        f"""          <div class="legend-row">
            <span class="legend-swatch" style="background:{color}"></span>
            {label}<b>{fa(pct)}٪</b>
          </div>""" for label, pct, color in PAYMENTS)
    return f"""      <section class="dash-card dash-rise">
        <div class="dash-card-head">
          <h2 class="dash-card-title">روش پرداخت — این ماه</h2>
        </div>
        <div class="donut-wrap">
          <div class="donut-shell" role="img"
               aria-label="سهم روش‌های پرداخت: کارت‌خوان ۴۱ درصد، نقدی ۳۲ درصد، درگاه آنلاین ۲۲ درصد، کارت به کارت ۵ درصد">
            <div class="donut"></div>
            <span class="donut-center"><b>۴۱٪</b><span>کارت‌خوان</span></span>
          </div>
          <div class="donut-legend">
{legend}
          </div>
        </div>
        <p class="dash-note">تطبیق صندوق شبانه: مجموع نقدی و کارت‌خوان باید با
        دستگاه‌ها بخواند. سهم درگاه آنلاین هزینه کارمزد دارد.</p>
      </section>"""


def extras_card():
    return f"""      <section class="dash-card dash-rise">
        <div class="dash-card-head">
          <h2 class="dash-card-title">تخفیف، رضایت و مناسبت‌ها</h2>
        </div>
        <div class="simple-rows">
          <div class="simple-row">
            <span>تخفیف داده‌شده این ماه</span><b>{fa(4_250_000)} تومان<span> · ۵.۲٪ درآمد</span></b>
          </div>
          <div class="simple-row">
            <span>پرمصرف‌ترین کد</span><b>NOWRUZ25<span> · ۱۴ بار</span></b>
          </div>
          <div class="simple-row">
            <span>رضایت مشتریان</span>
            <span class="dash-rating"><b>۴٫۶</b>
              <span class="stars" aria-hidden="true">{icon("star")}{icon("star")}{icon("star")}{icon("star")}</span>
              <span>از ۵ · ۳۸ نظر</span></span>
          </div>
          <div class="simple-row">
            <span>تولدهای این هفته</span><b>{fa(3)} نفر</b>
          </div>
        </div>
      </section>"""


def dashboard():
    return f"""      <div class="dash">

        <header class="dash-head dash-rise">
          <div>
            <h1>داشبورد مدیریت سالن</h1>
            <p>چهارشنبه ۱۰ مهر ۱۴۰۵ · ساعت ۱۴:۲۰</p>
          </div>
          <div class="dash-head-actions">
            <button type="button" class="dash-btn dash-btn--primary">+ ثبت رزرو حضوری</button>
            <button type="button" class="dash-btn">گزارش‌ها</button>
          </div>
        </header>

        <div class="dash-hero-row">
{hero()}
{goal()}
        </div>

        <div class="dash-kpis">
{kpis()}
        </div>

{action_queue()}

{timeline()}

        <div class="dash-grid">
{heatmap()}
{staff_card()}
{services_card()}
{customers_card()}
{payment_card()}
{extras_card()}
        </div>

      </div>"""


# ─────────────────────────── صفحه پیش‌نمایش ───────────────────────────
COUNT_UP_JS = """
/* شمارش صعودی عدد قهرمان — همان حسی که مثل شبکه‌های اجتماعی هر روز
   کاربر را برمی‌گرداند. اگر JS اجرا نشود، عدد نهایی از خود HTML خوانده
   می‌شود، پس نمایش هرگز خالی نمی‌ماند. */
(function () {
    var FA = '۰۱۲۳۴۵۶۷۸۹';
    function toFa(s) { return String(s).replace(/\\d/g, function (d) { return FA[+d]; }); }
    function fmt(n) { return toFa(n.toLocaleString('en-US')); }

    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduce) return;

    var nodes = document.querySelectorAll('[data-count]');
    Array.prototype.forEach.call(nodes, function (el) {
        var raw = el.getAttribute('data-count');
        if (!raw) return;
        var target = parseInt(raw, 10);
        if (isNaN(target)) return;
        var dur = 900, t0 = null;
        function step(ts) {
            if (t0 === null) t0 = ts;
            var p = Math.min((ts - t0) / dur, 1);
            var eased = 1 - Math.pow(1 - p, 3);
            el.textContent = fmt(Math.round(target * eased));
            if (p < 1) requestAnimationFrame(step);
        }
        el.textContent = fmt(0);
        requestAnimationFrame(step);
    });
})();
"""


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

    desktop = bs.capsule("expanded", "pv2", "دسکتاپ عریض — Sidebar باز", content=dashboard())
    mobile = f'''<section class="stage stage--phone">
  <h2 class="stage-title">موبایل (۳۹۰px) — Sidebar پنهان، محتوا تمام‌عرض</h2>
  <div class="panel-shell--capsule stage-shell">
    <div class="panel-content">
{dashboard()}
    </div>
  </div>
</section>'''

    page = f'''<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>ماکت داشبورد مدیریتی — SolonBeauty</title>
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
    max-width: 780px;
    margin: 0 auto;
    padding: 34px 28px 6px;
    font-size: .82rem;
    line-height: 2;
    color: rgba(237,230,218,.66);
}}
.note h1 {{ margin: 0 0 10px; font-size: 1.05rem; color: #F7F2EA; font-weight: 800; }}
.note strong {{ color: #C6A46A; font-weight: 700; }}
.note code {{
    font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
    background: rgba(237,230,218,.08);
    padding: 1px 6px;
    border-radius: 5px;
    font-size: .92em;
}}
.note ul {{ margin: 8px 0 0; padding-inline-start: 20px; }}
.note li {{ margin-block-end: 4px; }}

.stage-wrap {{
    display: flex;
    flex-wrap: wrap;
    gap: 44px;
    align-items: flex-start;
    padding: 30px 28px 80px;
    overflow-x: auto;
}}
.stage {{ margin: 0; flex: 0 0 auto; }}
.stage-title {{
    font-size: .78rem;
    font-weight: 700;
    letter-spacing: .04em;
    color: rgba(237,230,218,.55);
    margin: 0 0 12px;
}}

/* پوسته دسکتاپ: گرید به‌زور روشن می‌شود چون عرض iframe ممکن است کمتر از
   ۱۰۲۴px باشد. این بازنویسی فقط مخصوص پیش‌نمایش است. */
.stage-shell {{
    display: grid;
    grid-template-columns: auto minmax(0, 1fr);
    gap: var(--sb-gap);
    padding: 0;
    min-height: 0;
    background: none;
    background-attachment: scroll;
    overflow: visible;
}}
.stage-shell::before {{ content: none; }}
.stage .panel-sidebar {{ position: static; }}
.stage .pnav-capsule {{ position: relative; block-size: 960px; }}

/* قاب موبایل: ۳۹۰ پیکسل. قاعده‌های دسکتاپ اپ به‌زور خاموش می‌شوند تا
   همان چیزی دیده شود که کاربر واقعی موبایل می‌بیند. */
.stage--phone .stage-shell {{
    display: block;
    inline-size: 390px;
    background: #F7F2EA;
    border-radius: 26px;
    overflow: hidden;
    box-shadow: 0 24px 60px rgba(0,0,0,.45);
}}
.stage--phone .panel-sidebar {{ display: none; }}
.stage--phone .panel-content {{
    border-radius: 0;
    box-shadow: none;
    min-block-size: 0;
}}
.stage--phone .dash-hero-num {{ font-size: 2.05rem; }}
.stage--phone .dash {{
    /* جا برای نوار ناوبری پایین که در مرحله ۵ ساخته می‌شود */
    padding-block-end: 86px;
}}

/* راهنمای رنگ‌ها برای بازبینی طراحی */
.palette {{
    display: flex; flex-wrap: wrap; gap: 10px;
    padding: 0 28px 40px; max-width: 1180px;
}}
.swatch {{
    display: flex; align-items: center; gap: 8px;
    font-size: .72rem; color: rgba(237,230,218,.6);
    background: rgba(237,230,218,.06);
    border: 1px solid rgba(237,230,218,.1);
    border-radius: 999px; padding: 6px 12px 6px 6px;
}}
.swatch i {{
    inline-size: 20px; block-size: 20px; border-radius: 50%;
    display: block; flex: 0 0 auto;
    box-shadow: inset 0 0 0 1px rgba(255,255,255,.18);
}}
</style>
</head>
<body>

<div class="note">
    <h1>ماکت داشبورد مدیریتی سالن زیبایی</h1>
    <p>
        این فایل از <strong>CSS و JS واقعی پروژه</strong> ساخته شده
        (<code>panel-dashboard.css</code> ، <code>panel-sidebar.css</code> ،
        <code>mobile-ux.js</code>). یعنی آنچه می‌بینید همان کدی است که وارد اپ
        می‌شود، نه یک ماکت جدا که بعداً دور ریخته شود. بازسازی با:
        <code>python3 tools/build-dashboard-preview.py</code>
    </p>
    <p><strong> چه چیزی را ببینید:</strong></p>
    <ul>
        <li>عدد قهرمان درآمد، با مقایسه با <strong>میانگین چهارشنبه‌های ماه</strong> —
        نه دیروز، چون دیروز ممکن است تعطیل بوده باشد.</li>
        <li>نوار هدف ماه با خط تقویم: اگر نوار از خط جلو بزند، از برنامه جلوتریم.</li>
        <li>صف اقدام: «امروز به این موارد رسیدگی کن» — مهم‌ترین بخش برای مدیرِ سرپا.</li>
        <li>نقشه شلوغی هفته و جعبه بینش که خلوتِ فردا را به پیشنهاد تبدیل می‌کند.</li>
        <li>سلامت مشتریان با فهرست مشتریان در خطر ریزش — آماری که <em>قابل اقدام</em> است.</li>
    </ul>
    <p>
        <strong>توجه:</strong> اعداد نمایشی و واقع‌گرایانه‌اند. همه از داده‌های
        موجود دیتابیس قابل استخراج‌اند، به‌جز پورسانت پرسنل، هدف ماهانه و
        موجودی مواد مصرفی که به فیلدهای جدید نیاز دارند.
    </p>
</div>

<div class="stage-wrap">
{desktop}
{mobile}
</div>

<div class="palette">
  <span class="swatch"><i style="background:#14110F"></i>#14110F — پس‌زمینه کپسول</span>
  <span class="swatch"><i style="background:#C6A46A"></i>#C6A46A — طلایی شامپاینی</span>
  <span class="swatch"><i style="background:#A8843F"></i>#A8843F — طلایی عمیق</span>
  <span class="swatch"><i style="background:#F7F2EA"></i>#F7F2EA — عاجی پنل</span>
  <span class="swatch"><i style="background:#3F6B52"></i>#3F6B52 — موفق</span>
  <span class="swatch"><i style="background:#9A6520"></i>#9A6520 — هشدار</span>
  <span class="swatch"><i style="background:#A8445C"></i>#A8445C — فوری</span>
</div>

<script>
{js}
</script>
<script>
/* همان تابعی که کامپوننت PanelSidebar پس از هر رندر صدا می‌زند */
if (window.solonUx && window.solonUx.initActiveBridge) {{
    window.solonUx.initActiveBridge();
}}
</script>
<script>
{COUNT_UP_JS}
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
