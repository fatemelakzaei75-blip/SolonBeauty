#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
هندسه موج چندلایه صفحه ورود — منبع واحد.

چرا این فایل هست:
مسیرهای SVG موج باید دقیقاً یک چیز باشند، ولی در سه جا لازم می‌شوند:
  ۱) کامپوننت رِیزور (WaveDivider.razor) که کاربر می‌بیند
  ۲) پیش‌نمایش HTML که برای بازبینی چشمی ساخته می‌شود
  ۳) ماکت تصویری (PNG) که بدون مرورگر رندر می‌شود
اگر هر کدام عددهای خودش را داشته باشد، دیر یا زود از هم می‌افتند. پس هندسه
فقط اینجا تعریف می‌شود و آن دو جای دیگر از همین‌جا ساخته می‌شوند:
  tools/build-wave-component.py  →  WaveDivider.razor   (با حالت --check)
  tools/render-wave-check.py     →  ماکت PNG

شکل موج:
منحنی از نقاط راه (waypoints) با اسپلاین Catmull-Rom ساخته می‌شود. چرا
اسپلاین و نه یک cubic دست‌ساز: اسپلاین از هر نقطه راه می‌گذرد و مشتق‌ش
پیوسته است (C1)، پس لبه بدون شکستگی و «ارگانیک» می‌شود؛ ضمناً تنظیم شکل
یعنی جابه‌جا کردن یک نقطه، نه بازنویسی چهار عدد کنترل.

نکته مهم درباره لایه‌ها:
همه لایه‌ها عیناً یک منحنی‌اند که به اندازه «فاصله» جابه‌جا شده. همین
هم‌شکلی است که حس «ورق‌های کاغذ بریده روی هم» را می‌دهد. آزمون هم دقیقاً
همین را بررسی می‌کند: لایه n+1 = لایه n + فاصله.
"""

import math

VIEW_DESKTOP = (100, 600)
VIEW_MOBILE = (600, 120)

# فاصله لایه‌ها — بریف: «نازک، حدود ۷ واحد از ۱۰۰ فاصله»
GAP = 7

# ---------------------------------------------------------------------------
# خط طلایی دوگانه (مرحله ۴)
# ---------------------------------------------------------------------------
# خط اصلی دقیقاً روی لبه «لایه سمت تصویر» می‌نشیند (لایه ۱ = لایه دیده‌شده
# کنار تصویر) و خط دوم کمی داخل نوار.
GOLD_LAYER = 0

# فاصله دو خط بر حسب **پیکسل دیده‌شده**، نه واحد viewBox.
# چرا پیکسل: موج با preserveAspectRatio="none" کشیده می‌شود، یعنی ضریب افقی و
# عمودی یکی نیستند؛ اگر فاصله را در واحد viewBox می‌دادیم، فاصله دیده‌شده
# بسته به جهت منحنی تغییر می‌کرد.
GOLD_GAP_PX = 4.0

# اندازه مرجعی که فاصله پیکسلی بر مبنای آن به واحد viewBox تبدیل می‌شود.
# چرا این دو اندازه: در هر قالب، محوری که فاصله عمدتاً در آن می‌افتد ضریب
# ثابتی دارد (دسکتاپ: افقی ۲۴۰÷۱۰۰، موبایل: عمودی ۹۶÷۱۲۰)، پس فاصله دیده‌شده
# روی همه اندازه‌های صفحه تقریباً ثابت می‌ماند.
GOLD_REF = {'vertical': (240.0, 600.0), 'mobile': (390.0, 96.0)}

# تراکم نمونه‌برداری مسیر خط دوم بر حسب واحد viewBox
GOLD_STEP = 5.0

# نام کلاس ایستگاه‌های گرادیان فلزی. رنگ‌ها اینجا نمی‌آیند: مقدارشان در
# login.css به‌صورت توکن (--lg-gold-metal-1..5) است و از راه کلاس CSS روی هر
# stop می‌نشیند. پس SVG هیچ رنگ خامی ندارد.
GOLD_STOPS = ['lg-gold__stop-%d' % i for i in range(1, 6)]

# ---------------------------------------------------------------------------
# نقاط راه موج دسکتاپ: (x, y) در viewBox 0 0 100 600
# ---------------------------------------------------------------------------
# x=70 یعنی نزدیک ستون فرم (کم‌عمق‌ترین حالت) و x=4 یعنی عمیق‌ترین نفوذ به تصویر.
# منحنی از بالا آرام حرکت می‌کند و «شکم» بزرگ زیر میانه (y≈470 از ۶۰۰) به
# تصویر نفوذ می‌کند؛ بریف: «S عمودی با یک برجستگی بزرگ پایین‌تر از میانه».
# سپس در انتها کمی برمی‌گردد — همان برآمدگی که حس کنده‌کاری کاغذ می‌دهد.
DESKTOP_WAYPOINTS = [
    (69, 0),
    (72, 90),
    (68, 180),
    (60, 250),
    (46, 330),
    (14, 410),
    (4, 470),
    (22, 540),
    (34, 600),
]

# ---------------------------------------------------------------------------
# نقاط راه موج موبایل: (x, y) در viewBox 0 0 600 120
# ---------------------------------------------------------------------------
# در موبایل تصویر بالاست، پس لبه بالای هر لایه مرزِ دیده‌شده است و هر لایه
# به سمت پایین پر می‌شود. پروفایل شکل عیناً همان پروفایل دسکتاپ است (از فریم
# ۰ تا ۱)، فقط محور عمودی و افقی‌شان جابه‌جا شده: عمیق‌ترین نفوذ به تصویر
# در ۷۸٪ طول افقی می‌افتد — یعنی زیر موی سمت راست تصویرسازی، و صورت در
# ناحیه کم‌عمق سمت چپ می‌ماند.
MOBILE_WAYPOINTS = [
    (0, 70.0),
    (120, 67.6),
    (240, 62.1),
    (330, 51.1),
    (410, 25.9),
    (470, 18.0),
    (540, 32.2),
    (600, 41.6),
]


class Wave:
    """یک موج: viewBox، تعداد لایه، جهت جابه‌جایی لایه‌ها و سمت پر شدن."""

    def __init__(self, name, view, waypoints, layers, offset_axis, fill_side,
                 gap=GAP):
        self.name = name
        self.view = view
        self.waypoints = waypoints
        self.layers = layers
        self.offset_axis = offset_axis      # 'x' دسکتاپ | 'y' موبایل
        self.fill_side = fill_side          # 'right' دسکتاپ | 'bottom' موبایل
        self.gap = gap

    # ---------------- هندسه پایه ----------------

    def points(self, layer):
        """نقاط راه یک لایه پس از جابه‌جایی."""
        shift = self.gap * layer
        return [
            (x + shift if self.offset_axis == 'x' else x,
             y + shift if self.offset_axis == 'y' else y)
            for x, y in self.waypoints
        ]

    def segments(self, layer):
        """قطعه‌های مکعبی لایه، به‌صورت ((x0,y0),(c1x,c1y),(c2x,c2y),(x1,y1)).

        اسپلاین Catmull-Rom: نقطه کنترل اول در راستای وتر قبل و نقطه کنترل
        دوم در راستای وتر بعد قرار می‌گیرد؛ به همین دلیل مشتق در محل هر نقطه
        راه از دو طرف یکی است و لبه هیچ شکستگی‌ای ندارد.
        """
        pts = self.points(layer)
        ext = [pts[0]] + list(pts) + [pts[-1]]
        segs = []
        for i in range(1, len(ext) - 2):
            p0, p1, p2, p3 = ext[i - 1], ext[i], ext[i + 1], ext[i + 2]
            c1 = (p1[0] + (p2[0] - p0[0]) / 6.0, p1[1] + (p2[1] - p0[1]) / 6.0)
            c2 = (p2[0] - (p3[0] - p1[0]) / 6.0, p2[1] - (p3[1] - p1[1]) / 6.0)
            segs.append((p1, c1, c2, p2))
        return segs

    def sample(self, layer, per_segment=24):
        """نمونه‌برداری از منحنی لبه (برای آزمون و رندر)."""
        out = []
        for (p0, c1, c2, p1) in self.segments(layer):
            for i in range(per_segment + (0 if out else 1)):
                t = i / float(per_segment)
                mt = 1 - t
                x = (mt ** 3) * p0[0] + 3 * (mt ** 2) * t * c1[0] \
                    + 3 * mt * (t ** 2) * c2[0] + (t ** 3) * p1[0]
                y = (mt ** 3) * p0[1] + 3 * (mt ** 2) * t * c1[1] \
                    + 3 * mt * (t ** 2) * c2[1] + (t ** 3) * p1[1]
                out.append((x, y))
        return out

    # ---------------- خروجی SVG ----------------

    def edge_d(self, layer):
        """مسیر باز لبه — پایه خط طلایی مرحله ۴."""
        segs = self.segments(layer)
        d = 'M %s %s' % (fmt(segs[0][0][0]), fmt(segs[0][0][1]))
        for (_, c1, c2, p1) in segs:
            d += ' C %s %s %s %s %s %s' % (fmt(c1[0]), fmt(c1[1]),
                                           fmt(c2[0]), fmt(c2[1]),
                                           fmt(p1[0]), fmt(p1[1]))
        return d

    def fill_d(self, layer):
        """مسیر بسته پرکردنی: از لبه تا سمت سطح فرم."""
        segs = self.segments(layer)
        first = segs[0][0]
        last = segs[-1][3]
        d = self.edge_d(layer)
        if self.fill_side == 'right':
            w = self.view[0]
            d += ' L %s %s L %s %s Z' % (fmt(w), fmt(last[1]), fmt(w), fmt(first[1]))
        else:                                   # bottom
            h = self.view[1]
            d += ' L %s %s L %s %s Z' % (fmt(last[0]), fmt(h), fmt(first[0]), fmt(h))
        return d

    def fills(self):
        return [self.fill_d(i) for i in range(self.layers)]

    def edges(self):
        return [self.edge_d(i) for i in range(self.layers)]

    # ---------------- خط طلایی دوگانه (مرحله ۴) ----------------

    def gold_points(self, gap_px=GOLD_GAP_PX, ref=None):
        """نقاط دو خط طلایی در واحد viewBox.

        خروجی: (نقاط خط اصلی، نقاط خط دوم)
        خط اصلی روی خودِ لبه است و خط دوم با فاصله عمودِ صحیح، به سمت داخل
        نوار جابه‌جا شده. فاصله در فضای پیکسلی حساب می‌شود و بعد به واحد
        viewBox برمی‌گردد — وگرنه کشیدگی غیریکنواخت فاصله را خراب می‌کرد.
        """
        ref = ref or GOLD_REF['mobile' if self.offset_axis == 'y' else 'vertical']
        sx = ref[0] / float(self.view[0])
        sy = ref[1] / float(self.view[1])

        pts = self.sample(GOLD_LAYER, 60)
        # ۱) به فضای پیکسلی
        pix = [(x * sx, y * sy) for x, y in pts]

        # ۲) جابه‌جایی عمود بر منحنی، به سمت داخل نوار
        shifted = []
        for i, p in enumerate(pix):
            a = pix[max(0, i - 1)]
            b = pix[min(len(pix) - 1, i + 1)]
            tx, ty = b[0] - a[0], b[1] - a[1]
            norm = math.hypot(tx, ty) or 1.0
            nx, ny = -ty / norm, tx / norm
            # جهت نرمال را طوری می‌چرخانیم که به سمت پرشدن نوار برود
            inside = (nx > 0) if self.fill_side == 'right' else (ny > 0)
            if not inside:
                nx, ny = -nx, -ny
            shifted.append((p[0] + nx * gap_px, p[1] + ny * gap_px))

        # ۳) بازگشت به واحد viewBox و کم‌کردن تعداد نقاط با نمونه‌برداری
        #    یکنواخت بر حسب طول کمان (مسیر کوتاه‌تر، بدون افت کیفیت دیداری)
        #
        # مهار به کادر دید: جابه‌جایی عمود بر منحنی در فضای پیکسلی، در دو سرِ
        # مسیر کمی مؤلفه عمودی هم دارد (چون ضریب دو محور یکی نیست)، پس نقطه
        # ابتدا/انتها می‌تواند زیر ۰٫۵ واحد از کادر بیرون بزند و بریده شود.
        # مهار، فقط همان چند صدم واحد انتهایی را برمی‌گرداند؛ جای دیگری اثر ندارد.
        w, h = self.view
        user = [(min(max(x / sx, 0.0), float(w)), min(max(y / sy, 0.0), float(h)))
                for x, y in shifted]
        return pts, resample(user, GOLD_STEP)

    def gold_d(self, gap_px=GOLD_GAP_PX, ref=None):
        """(مسیر خط اصلی، مسیر خط دوم) — آماده برای ویژگی d در SVG."""
        main, thin = self.gold_points(gap_px, ref)
        return self.edge_d(GOLD_LAYER), polyline_d(thin)

    def gold_gradient(self):
        """مبدأ و مقصد گرادیان فلزی — هم‌راستا با خط.

        خط دسکتاپ عمودی است و خط موبایل افقی، پس گرادیان هم باید در همان
        راستا بدود؛ وگرنه فلز بودنش دیده نمی‌شود.
        """
        if self.offset_axis == 'x':
            return (0.0, 0.0, 0.0, float(self.view[1]))
        return (0.0, 0.0, float(self.view[0]), 0.0)

    def gold_offsets(self, gap_px=GOLD_GAP_PX, ref=None):
        """فاصله دیده‌شده هر نقطه خط دوم از خط اصلی، بر حسب پیکسل.

        چرا نقطه‌به‌نقطه نیست: خط اصلی با اسپلاین و خط دوم با نمونه‌برداری
        یکنواخت ساخته می‌شود، پس نقطه i ام دو مسیر روی یک جای منحنی نیستند.
        اینجا برای هر نقطه خط دوم، نزدیک‌ترین نقطه خط اصلی پیدا و فاصله
        سنجیده می‌شود (جست‌وجوی دو اشاره‌گری، چون هر دو مسیر مرتب‌اند).

        خروجی: (فهرست فاصله‌ها، علامت «به سمت داخل نوار»)
        """
        ref = ref or GOLD_REF['mobile' if self.offset_axis == 'y' else 'vertical']
        sx = ref[0] / float(self.view[0])
        sy = ref[1] / float(self.view[1])

        main, thin = self.gold_points(gap_px, ref)
        px_main = [(x * sx, y * sy) for x, y in main]
        px_thin = [(x * sx, y * sy) for x, y in thin]

        inward = (1.0, 0.0) if self.fill_side == 'right' else (0.0, 1.0)
        axis = 0 if self.fill_side == 'right' else 1

        # جست‌وجوی کامل، نه دو اشاره‌گری.
        # چرا: دو اشاره‌گریِ ساده («یک قدم جلو ببین کوتاه‌تر است؟») در جاهایی که
        # منحنی خم می‌شود در کمینه محلی گیر می‌کند و از آن به بعد همه فاصله‌ها
        # اشتباه می‌شوند — یک‌بار همین اتفاق افتاد و ۵۱۵ پیکسل گزارش شد.
        # این تابع فقط ابزار بررسی است، پس هزینه ۱۲۴×۴۸۱ محاسبه اهمیتی ندارد.
        dists, signs, nearest = [], [], []
        for b in px_thin:
            j = min(range(len(px_main)),
                    key=lambda k: (px_main[k][0] - b[0]) ** 2 + (px_main[k][1] - b[1]) ** 2)
            a = px_main[j]
            dists.append(math.hypot(a[0] - b[0], a[1] - b[1]))
            signs.append(1 if b[axis] - a[axis] > 0 else -1)
            nearest.append(j)
        return dists, signs

    # ---------------- عددهای گزارش ----------------

    def band(self):
        """بازه دیداری کل نوار (کمینه و بیشینه در محور عمود)."""
        axis = 0 if self.offset_axis == 'x' else 1
        vals = []
        for i in range(self.layers):
            vals += [p[axis] for p in self.sample(i)]
        return min(vals), max(vals)

    def deepest(self, layer=0):
        """عمیق‌ترین نفوذ به سمت تصویر: (مقدار محور عمود، موقعیت روی محور طول)."""
        pts = self.sample(layer)
        if self.offset_axis == 'x':
            return min(pts, key=lambda p: p[0])
        return min(pts, key=lambda p: p[1])

    def band_px(self, rendered):
        """پهنای نوار بر حسب پیکسل، وقتی عنصر تا اندازه داده‌شده کشیده شود."""
        lo, hi = self.band()
        units = self.view[0] if self.offset_axis == 'x' else self.view[1]
        return (hi - lo) / units * rendered


def fmt(v):
    """عدد جمع‌وجور: دو رقم اعشار، بدون صفر اضافه."""
    s = ('%.2f' % v).rstrip('0').rstrip('.')
    return s if s != '-0' else '0'


def resample(points, step):
    """نمونه‌برداری یکنواخت از یک مسیر، بر حسب طول کمان."""
    lengths = [0.0]
    for a, b in zip(points, points[1:]):
        lengths.append(lengths[-1] + math.hypot(b[0] - a[0], b[1] - a[1]))
    total = lengths[-1]
    if total <= 0 or step <= 0:
        return list(points)

    out = [points[0]]
    target = step
    seg = 1
    while target < total:
        while seg < len(lengths) - 1 and lengths[seg] < target:
            seg += 1
        span = lengths[seg] - lengths[seg - 1] or 1.0
        t = (target - lengths[seg - 1]) / span
        a, b = points[seg - 1], points[seg]
        out.append((a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t))
        target += step
    out.append(points[-1])
    return out


def polyline_d(points):
    """مسیر چندضلعی از نقاط — سبک‌تر از اسپلاین برای یک خط ۱ پیکسلی."""
    head = 'M %s %s' % (fmt(points[0][0]), fmt(points[0][1]))
    return head + ''.join(' L %s %s' % (fmt(x), fmt(y)) for x, y in points[1:])


DESKTOP = Wave('desktop', VIEW_DESKTOP, DESKTOP_WAYPOINTS, 5, 'x', 'right')
MOBILE = Wave('mobile', VIEW_MOBILE, MOBILE_WAYPOINTS, 4, 'y', 'bottom')


def verify():
    """بررسی خواصی که بریف خواسته — همان‌ها که در آزمون هم تکرار می‌شوند."""
    problems = []

    def need(cond, msg):
        if not cond:
            problems.append(msg)

    for w in (DESKTOP, MOBILE):
        # همه مختصات داخل viewBox
        ww, hh = w.view
        for i in range(w.layers):
            for (x, y) in w.sample(i):
                need(-0.01 <= x <= ww + 0.01 and -0.01 <= y <= hh + 0.01,
                     '%s: لایه %d از viewBox بیرون زده' % (w.name, i + 1))

        # هم‌شکلی لایه‌ها: فاصله ثابت
        axis = 0 if w.offset_axis == 'x' else 1
        base = w.sample(0)
        for i in range(1, w.layers):
            pts = w.sample(i)
            need(len(pts) == len(base), '%s: تعداد نمونه‌ها یکی نیست' % w.name)
            for (p, q) in zip(base, pts):
                need(abs((q[axis] - p[axis]) - w.gap * i) < 1e-6,
                     '%s: فاصله لایه %d دقیق نیست' % (w.name, i + 1))
                need(abs(q[1 - axis] - p[1 - axis]) < 1e-6,
                     '%s: لایه %d در محور دیگر جابه‌جا شده' % (w.name, i + 1))

        # یکنوایی در محور طول (بدون حلقه و برگشت)
        long_axis = 1 if w.offset_axis == 'x' else 0
        pts = w.sample(0)
        back = sum(1 for a, b in zip(pts, pts[1:]) if b[long_axis] < a[long_axis])
        need(back == 0, '%s: منحنی در محور طول عقب می‌رود (%d نمونه)' % (w.name, back))

        # برجستگی بزرگ پایین‌تر از میانه (دسکتاپ) / راست‌تر از میانه (موبایل)
        deep_axis, long_len = (0, w.view[1]) if w.offset_axis == 'x' else (1, w.view[0])
        d = w.deepest(0)
        need(d[long_axis] > long_len * 0.5,
             '%s: عمیق‌ترین نقطه در نیمه اول است (%.1f از %.1f)'
             % (w.name, d[long_axis], long_len))

        # نازکی لایه‌ها: فاصله ۷ واحد از ۱۰۰
        need(abs(w.gap - 7) < 1e-9, '%s: فاصله لایه‌ها ۷ واحد نیست' % w.name)

        # ---------- خط طلایی دوگانه ----------
        main_pts, thin_pts = w.gold_points()

        # هر دو خط داخل کادر دید بمانند (وگرنه بریده می‌شوند)
        for label, pts_ in (('اصلی', main_pts), ('دوم', thin_pts)):
            for (x, y) in pts_:
                need(-0.01 <= x <= w.view[0] + 0.01 and -0.01 <= y <= w.view[1] + 0.01,
                     '%s: خط %s از viewBox بیرون زده' % (w.name, label))

        # فاصله دیده‌شده دو خط، در همه جای منحنی نزدیک GOLD_GAP_PX بماند.
        # چرا کران بالا: اگر فاصله در واحد viewBox حساب می‌شد (باگ محتمل)،
        # در نقاط شیب‌دار چند برابر می‌شد و همین بررسی می‌گرفتش.
        dists, signs = w.gold_offsets()
        need(min(dists) > 0.75 * GOLD_GAP_PX,
             '%s: جایی از خط دوم به خط اول چسبیده (%.2f پیکسل)' % (w.name, min(dists)))
        need(max(dists) < 1.5 * GOLD_GAP_PX,
             '%s: فاصله دو خط در جایی زیاد شده (%.2f پیکسل از %.1f)'
             % (w.name, max(dists), GOLD_GAP_PX))

        # خط دوم همیشه سمت داخل نوار باشد، نه روی تصویر
        need(all(s > 0 for s in signs),
             '%s: خط دوم در %d نقطه سمت تصویر افتاده' % (w.name, signs.count(-1)))

        # گرادیان هم‌راستا با خط: دسکتاپ عمودی، موبایل افقی
        g = w.gold_gradient()
        if w.offset_axis == 'x':
            need(g[0] == g[2] and g[3] > g[1], '%s: گرادیان باید عمودی باشد' % w.name)
        else:
            need(g[1] == g[3] and g[2] > g[0], '%s: گرادیان باید افقی باشد' % w.name)

        need(len(GOLD_STOPS) == 5, 'گرادیان باید ۵ ایستگاه داشته باشد')

    need(DESKTOP.layers == 5, 'دسکتاپ باید ۵ لایه باشد')
    need(MOBILE.layers == 4, 'موبایل باید ۴ لایه باشد')

    return problems


if __name__ == '__main__':
    print('موج دسکتاپ  viewBox=%s' % (VIEW_DESKTOP,))
    lo, hi = DESKTOP.band()
    print('   بازه نوار: %.2f .. %.2f واحد  →  %.1f پیکسل در عرض ۲۴۰ پیکسل'
          % (lo, hi, DESKTOP.band_px(240)))
    print('   عمیق‌ترین نفوذ لایه ۱: x=%.2f در y=%.1f (از ۶۰۰)'
          % (DESKTOP.deepest(0)[0], DESKTOP.deepest(0)[1]))
    print('موج موبایل   viewBox=%s' % (VIEW_MOBILE,))
    lo, hi = MOBILE.band()
    print('   بازه نوار: %.2f .. %.2f واحد  →  %.1f پیکسل در ارتفاع ۹۶ پیکسل'
          % (lo, hi, MOBILE.band_px(96)))
    print('   عمیق‌ترین نفوذ لایه ۱: y=%.2f در x=%.1f (از ۶۰۰)'
          % (MOBILE.deepest(0)[1], MOBILE.deepest(0)[0]))

    for w in (DESKTOP, MOBILE):
        dists, signs = w.gold_offsets()
        print('خط طلایی %-8s فاصله دیده‌شده دو خط: %.2f .. %.2f پیکسل (هدف %.1f) — %d نقطه'
              % (w.name, min(dists), max(dists), GOLD_GAP_PX, len(dists)))

    problems = verify()
    print('\n%s' % ('✅ هندسه سالم است' if not problems
                    else '❌ %d ایراد:\n   - %s' % (len(problems), '\n   - '.join(problems))))
    raise SystemExit(0 if not problems else 1)
