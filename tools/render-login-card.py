#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
ماکت تصویری کارت صفحه ورود — رندر عددی بدون مرورگر.

چرا این فایل هست:
در محیط توسعه ما مرورگر نیست، پس «چیدمان و موج» را نمی‌توان چشمی سنجید.
این اسکریپت همان چیزی را که مرورگر می‌ساخت، با عدد و پیکسل رندر می‌کند:
صفحه، کارت، تصویر، موج ۵ لایه با سایه هر لایه، خط برش ستون فرم و اجزای فرم.

چرا از قالب‌های درست (نه سلیقه‌ای) استفاده می‌کند:
رنگ‌ها از همان توکن‌های login.css خوانده می‌شوند و منحنی موج از همان
tools/wave_geometry.py می‌آید که WaveDivider.razor از آن ساخته می‌شود.
پس این ماکت نمی‌تواند از کد اصلی جدا بیفتد.

خروجی: docs/login/03-desktop-render.png و docs/login/03-mobile-render.png

اجرا:  python3 tools/render-login-card.py
"""
import io
import math
import os
import re
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from wave_geometry import DESKTOP, MOBILE, GOLD_GAP_PX  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CSS = os.path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'css', 'login.css')
FONT_AR = os.path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'fonts', 'vazirmatn-arabic.woff2')

S = 2                       # سوپرسمپلینگ: متن و لبه‌ها نرم شوند
FA_RANGE = '\u0600-\u06FF\u200c\u200f'      # بازه یونیکد برای تشخیص متن فارسی


# ───────────────────────── خواندن توکن‌ها از CSS واقعی ─────────────────────────

def read_tokens():
    """توکن‌های --lg-* را از خود login.css می‌خواند (حالت روشن و تیره)."""
    with io.open(CSS, encoding='utf-8') as f:
        css = f.read()

    light_block = re.search(r'^\.lg \{(.*?)\n\}', css, re.S | re.M)
    dark_block = re.search(r'\[data-theme="dark"\] \.lg,\n\.lg--dark \{(.*?)\n\}', css, re.S)

    def parse(block):
        out = {}
        for m in re.finditer(r'(--lg-[a-z0-9-]+)\s*:\s*([^;]+);', block):
            out[m.group(1)] = m.group(2).strip()
        return out

    light = parse(light_block.group(1))
    dark = dict(light)
    if dark_block:
        dark.update(parse(dark_block.group(1)))
    return light, dark


def hex2rgb(v):
    v = v.strip()
    m = re.match(r'#([0-9A-Fa-f]{6})', v)
    if m:
        h = m.group(1)
        return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))
    m = re.match(r'rgba?\(([^)]+)\)', v)
    if m:
        parts = [p.strip() for p in m.group(1).split(',')]
        return tuple(int(float(p)) for p in parts[:3])
    raise ValueError('رنگ ناشناخته: %s' % v)


def rgba(v):
    """رنگ + آلفا از یک مقدار CSS."""
    v = v.strip()
    m = re.match(r'rgba\(([^)]+)\)', v)
    if m:
        parts = [p.strip() for p in m.group(1).split(',')]
        a = float(parts[3]) if len(parts) > 3 else 1.0
        return tuple(int(float(p)) for p in parts[:3]) + (int(round(a * 255)),)
    r, g, b = hex2rgb(v)
    return (r, g, b, 255)


def px(v):
    """مقدار CSS به عدد پیکسل."""
    m = re.match(r'(-?[\d.]+)px', v.strip())
    return float(m.group(1)) if m else float(v)


def metallic(stops, n):
    """نوار رنگ از ایستگاه‌های گرادیان فلزی (درون‌یابی خطی).

    چرا اینجا هم درون‌یابی: مرورگر خودش بین ایستگاه‌ها درون‌یابی می‌کند؛ ماکت
    باید همان کار را بکند تا رنگ دیده‌شده با مرورگر یکی باشد.
    """
    cols = [hex2rgb(c) for c in stops]
    out = []
    for i in range(n):
        t = (i / float(max(1, n - 1))) * (len(cols) - 1)
        a = int(math.floor(t))
        b = min(a + 1, len(cols) - 1)
        f = t - a
        out.append(tuple(int(round(cols[a][k] + (cols[b][k] - cols[a][k]) * f)) for k in range(3)))
    return out


# ───────────────────────── ابزارهای تصویری ─────────────────────────

def canvas(w, h, colour=(0, 0, 0, 0)):
    im = Image.new('RGBA', (int(w * S), int(h * S)), colour)
    return im


def draw_layer(base, mask, colour):
    """رنگ را با ماسک روی تصویر می‌نشاند."""
    tile = Image.new('RGBA', base.size, colour if len(colour) == 4 else colour + (255,))
    return Image.alpha_composite(base, Image.composite(tile, Image.new('RGBA', base.size), mask))


def rounded_mask(size, box, radius):
    m = Image.new('L', size, 0)
    ImageDraw.Draw(m).rounded_rectangle([c * S for c in box], radius * S, fill=255)
    return m


def rect_mask(size, box):
    m = Image.new('L', size, 0)
    ImageDraw.Draw(m).rectangle([c * S for c in box], fill=255)
    return m


def drop_shadow(base, mask, dx, dy, sigma, colour, alpha):
    """جایگزین عددی CSS drop-shadow: ماسک را محو و جابه‌جا می‌کند."""
    sh = mask.filter(ImageFilter.GaussianBlur(sigma * S))
    if dx or dy:
        sh = sh.transform(sh.size, Image.AFFINE, (1, 0, -dx * S, 0, 1, -dy * S),
                          resample=Image.BILINEAR)
    a = sh.point(lambda v: int(v * alpha))
    return draw_layer(base, a, colour + (255,))


def radial_page(w, h, c1, c2):
    """گرادیان شعاعی پس‌زمینه صفحه (radial-gradient(120% 90% at 50% 0%))."""
    yy, xx = np.mgrid[0:int(h * S), 0:int(w * S)]
    cx, cy = w * S * 0.5, 0.0
    rx, ry = w * S * 1.2, h * S * 0.9
    t = np.sqrt(((xx - cx) / rx) ** 2 + ((yy - cy) / ry) ** 2)
    t = np.clip(t / 0.7, 0, 1)[..., None]
    a = np.array(c1[:3], float)
    b = np.array(c2[:3], float)
    rgb = (a * (1 - t) + b * t).astype(np.uint8)
    return Image.fromarray(np.dstack([rgb, np.full(rgb.shape[:2], 255, np.uint8)]), 'RGBA')


# ───────────────────────── فونت ─────────────────────────

class Fonts:
    def __init__(self):
        self.cache = {}

    def get(self, size_px, weight='Regular'):
        key = (round(size_px, 1), weight)
        if key not in self.cache:
            f = ImageFont.truetype(FONT_AR, int(round(size_px * S)))
            try:
                f.set_variation_by_name(weight)
            except Exception:
                pass
            self.cache[key] = f
        return self.cache[key]


def text(draw, xy, s, font, colour, align='right'):
    """متن با جهت درست: فارسی راست‌به‌چپ، عدد/لاتین چپ‌به‌راست."""
    x, y = xy[0] * S, xy[1] * S
    fa = bool(re.search('[%s]' % FA_RANGE, s))
    if fa:
        draw.text((x, y), s, font=font, fill=colour, anchor='ra',
                  direction='rtl', language='fa')
    else:
        draw.text((x, y), s, font=font, fill=colour, anchor='la',
                  direction='ltr', language='en')


# ───────────────────────── آیکون‌ها (تقریب با شکل‌های ساده) ─────────────────────────
# آیکون‌های واقعی در AuthIcons.razor مسیر SVG دارند؛ اینجا فقط برای ماکت
# چیدمان، شکل ساده‌شان کشیده می‌شود. صحت خود آیکون‌ها آزمون جداگانه دارد.

def icon_phone(d, x, y, size, colour, w=1.7):
    """گوشی: مستطیل گرد + دو خط."""
    bw, bh = size * 0.42, size * 0.8
    box = [x + (size - bw) / 2, y + (size - bh) / 2, x + (size + bw) / 2, y + (size + bh) / 2]
    d.rounded_rectangle([c * S for c in box], radius=size * 0.11 * S, outline=colour, width=max(1, int(w * S)))
    d.line([(x + size * 0.44) * S, (y + size * 0.25) * S,
            (x + size * 0.56) * S, (y + size * 0.25) * S], fill=colour, width=max(1, int(w * S)))
    d.line([(x + size * 0.46) * S, (y + size * 0.75) * S,
            (x + size * 0.54) * S, (y + size * 0.75) * S], fill=colour, width=max(1, int(w * S)))


def icon_lock(d, x, y, size, colour, w=1.7):
    """قفل: بدنه گرد + کمان."""
    bw, bh = size * 0.62, size * 0.44
    top = y + size * 0.42
    box = [x + (size - bw) / 2, top, x + (size + bw) / 2, top + bh]
    d.rounded_rectangle([c * S for c in box], radius=size * 0.13 * S, outline=colour, width=max(1, int(w * S)))
    d.arc([(x + size * 0.24) * S, (y + size * 0.14) * S,
           (x + size * 0.76) * S, (y + size * 0.62) * S],
          180, 360, fill=colour, width=max(1, int(w * S)))
    d.line([(x + size * 0.5) * S, (top + bh * 0.18) * S,
            (x + size * 0.5) * S, (top + bh * 0.62) * S], fill=colour, width=max(1, int(w * S)))


def icon_eye(d, x, y, size, colour, w=1.5):
    """چشم: بیضی + مردمک."""
    d.arc([x * S, (y + size * 0.2) * S, (x + size) * S, (y + size * 0.8) * S],
          0, 360, fill=colour, width=max(1, int(w * S)))
    r = size * 0.13
    d.ellipse([(x + size / 2 - r) * S, (y + size / 2 - r) * S,
               (x + size / 2 + r) * S, (y + size / 2 + r) * S], fill=colour)


def icon_sparkle(d, x, y, size, colour):
    """درخشش چهارپر (نشان برند جانشین)."""
    cx, cy = x + size / 2, y + size / 2
    r, rr = size / 2, size * 0.13
    pts = [(cx, cy - r), (cx + rr, cy - rr), (cx + r, cy), (cx + rr, cy + rr),
           (cx, cy + r), (cx - rr, cy + rr), (cx - r, cy), (cx - rr, cy - rr)]
    d.polygon([(a * S, b * S) for a, b in pts], fill=colour)


# ───────────────────────── موج ─────────────────────────

def wave_masks(size, wave, box, scale):
    """
    ماسک هر لایه موج.

    ⚠ درس گرفته‌شده: دو ضریب مقیاس، نه یکی.
    موج با preserveAspectRatio="none" کشیده می‌شود؛ یعنی ضریب محور افقی و
    عمودی مستقل‌اند (۲۴۰÷۱۰۰ در برابر ۶۰۰÷۶۰۰). اگر یک ضریب برای هر دو
    محور به‌کار برود، موج عمودی ۲٫۴ برابر کشیده می‌شود و فقط نیمه بالایی
    (که تقریباً مستقیم است) دیده می‌شود — همان باگی که یک‌بار در همین ماکت
    رخ داد و فقط با اندازه‌گیری عددی روی تصویر لو رفت.

    box = (x0, y0, x1, y1) کادر موج؛ scale = (sx, sy).
    """
    sx, sy = scale
    masks = []
    for i in range(wave.layers):
        pts = wave.sample(i, 40)
        m = Image.new('L', size, 0)
        d = ImageDraw.Draw(m)
        poly = [(box[0] + p[0] * sx, box[1] + p[1] * sy) for p in pts]
        if wave.fill_side == 'right':
            poly = poly + [(box[2], box[3]), (box[2], box[1])]
        else:
            poly = poly + [(box[2], box[3]), (box[0], box[3])]
        d.polygon([(a * S, b * S) for a, b in poly], fill=255)
        masks.append(m)
    return masks


def draw_wave(img, wave, box, toks, per_layer=True, gold=True, stop1=None):
    """موج را با سایه کل، سایه لایه‌ها، خط طلایی دوگانه و درخشش می‌کشد.

    ترتیب دقیقاً مثل قالب واقعی است:
      ۱) سایه بیرونی کل موج (جانشین سافاری)
      ۲) لایه‌های ۱ تا ۵ — لایه ۱ سایه لایه‌ای ندارد (سایه‌اش روی تصویر
         می‌افتاد، کنار خط طلایی)
      ۳) درخشش نرم و روشن
      ۴) خط طلایی دوگانه روی لبه لایه ۱
    """
    scale = ((box[2] - box[0]) / wave.view[0], (box[3] - box[1]) / wave.view[1])
    masks = wave_masks(img.size, wave, box, scale)

    # ۱) سایه بیرونی کل موج
    union = masks[0]
    for m in masks[1:]:
        union = Image.composite(Image.new('L', img.size, 255), union, m)
    alpha = float(rgba(toks['--lg-wave-shadow-soft'])[3]) / 255.0
    if wave.fill_side == 'right':
        img = drop_shadow(img, union, -8, 10, 24, hex2rgb(toks['--lg-wave-shadow-soft']), alpha)
    else:
        img = drop_shadow(img, union, 0, 14, 26, hex2rgb(toks['--lg-wave-shadow-soft']), alpha)

    # ۲) لایه‌ها
    for i, m in enumerate(masks):
        if per_layer and i > 0:                 # لایه ۱ عمداً سایه ندارد
            c = hex2rgb(toks['--lg-wave-shadow'])
            a = float(rgba(toks['--lg-wave-shadow'])[3]) / 255.0
            if wave.fill_side == 'right':
                img = drop_shadow(img, m, -4, 2, 10, c, a)
            else:
                img = drop_shadow(img, m, 2, -4, 8, c, a)
        img = draw_layer(img, m, hex2rgb(toks['--lg-wave-%d' % (i + 1)]))

    # ۳) درخشش نرم و روشن (هاله دور خط طلایی)
    gcol = rgba(toks['--lg-gold-glow'])
    img = drop_shadow(img, union, 0, 0, 3.5, gcol[:3], gcol[3] / 255.0 * 0.85)

    # ۴) خط طلایی دوگانه
    if gold:
        img = draw_gold(img, wave, box, toks, stop1)
    return img


def draw_gold(img, wave, box, toks, stop1=None):
    """خط طلایی دوگانه با گرادیان فلزی — همان چیزی که SVG می‌سازد."""
    sx = (box[2] - box[0]) / wave.view[0]
    sy = (box[3] - box[1]) / wave.view[1]
    main_pts, thin_pts = wave.gold_points()

    stops = [toks['--lg-gold-metal-%d' % i] for i in range(1, 6)]
    if stop1:
        stops = [stop1] + stops[1:]

    w1 = px(toks['--lg-gold-line-w1'])
    w2 = px(toks['--lg-gold-line-w2'])
    op2 = float(toks['--lg-gold-line-op2'])

    def polyline(pts, width_px):
        m = Image.new('L', img.size, 0)
        d = ImageDraw.Draw(m)
        d.line([((box[0] + p[0] * sx) * S, (box[1] + p[1] * sy) * S) for p in pts],
               fill=255, width=max(1, int(round(width_px * S))), joint='curve')
        return m

    # گرادیان هم‌راستا با خط: دسکتاپ عمودی، موبایل افقی.
    # ⚠ ترتیب برگشتی PIL: (عرض، ارتفاع) — یک‌بار جابه‌جا گرفتم و تصویر
    # ناهم‌اندازه ساخت (خطای «images do not match» در putalpha).
    w, h = img.size
    if wave.offset_axis == 'x':
        n = int((box[3] - box[1]) * S)
        band = metallic(stops, n)
        grad = Image.new('RGB', (w, h))
        gp = grad.load()
        for y in range(h):
            k = min(max(int((y / S - box[1])), 0), n - 1)
            c = band[k]
            for x in range(w):
                gp[x, y] = c
    else:
        n = int((box[2] - box[0]) * S)
        band = metallic(stops, n)
        grad = Image.new('RGB', (w, h))
        gp = grad.load()
        for x in range(w):
            k = min(max(int((x / S - box[0])), 0), n - 1)
            c = band[k]
            for y in range(h):
                gp[x, y] = c

    out = img.copy()
    for pts, width, op in ((thin_pts, w2, op2), (main_pts, w1, 1.0)):
        m = polyline(pts, width)
        if op < 1.0:
            m = m.point(lambda v: int(v * op))
        col = grad.convert('RGBA')
        col.putalpha(m)
        out = Image.alpha_composite(out, col)
    return out


# ───────────────────────── تصویرسازی ─────────────────────────

def paste_cover(img, box, src_path, pos_x, pos_y, fit='cover'):
    """همان کاری که object-fit/object-position مرورگر می‌کند."""
    src = Image.open(src_path).convert('RGB')
    bw, bh = (box[2] - box[0]), (box[3] - box[1])
    iw, ih = src.size
    if fit == 'contain':
        s = min(bw / iw, bh / ih)
    else:
        s = max(bw / iw, bh / ih)
    rw, rh = iw * s, ih * s
    src = src.resize((max(1, int(round(rw * S))), max(1, int(round(rh * S)))), Image.LANCZOS)

    canvas_box = (int(box[0] * S), int(box[1] * S), int(box[2] * S), int(box[3] * S))
    layer = Image.new('RGB', (canvas_box[2] - canvas_box[0], canvas_box[3] - canvas_box[1]),
                      hex2rgb('#000000'))
    ox = (canvas_box[2] - canvas_box[0] - src.size[0]) * pos_x
    oy = (canvas_box[3] - canvas_box[1] - src.size[1]) * pos_y
    layer.paste(src, (int(round(ox)), int(round(oy))))
    img.paste(layer.convert('RGBA'), canvas_box[:2])
    return img


# ───────────────────────── اجزای فرم ─────────────────────────

def draw_field(d, F, toks, box, label, *, value='', placeholder='', icon=None,
               eye=False, dir_ltr=False):
    """فیلد کپسولی مطابق CapsuleField.razor + login.css."""
    x0, y0, x1, y1 = box
    right = x1                                   # در راست‌به‌چپ، شروع از راست
    lab_font = F.get(13.12, 'SemiBold')
    text(d, (right - 20, y0), label, lab_font, hex2rgb(toks['--lg-text-dim']))

    box_top = y0 + 21 + 7
    box_h = 52
    # قاب: کادر پر + حاشیه (کپسولی)
    bg = Image.new('RGBA', d._image.size, (0, 0, 0, 0))
    ImageDraw.Draw(bg).rounded_rectangle([x0 * S, (box_top + 1.5) * S, x1 * S, (box_top + box_h - 1.5) * S],
                                         (999) * S, fill=hex2rgb(toks['--lg-field-bg']) + (255,))
    ImageDraw.Draw(bg).rounded_rectangle([x0 * S, box_top * S, x1 * S, (box_top + box_h) * S],
                                         (999) * S, outline=hex2rgb(toks['--lg-field-border']) + (255,),
                                         width=max(1, int(1.5 * S)))
    d._image.alpha_composite(bg)

    dd = ImageDraw.Draw(d._image)
    # آیکون‌ها و متن داخل کپسول
    if icon == 'phone':
        icon_phone(dd, right - 20 - 20, box_top + box_h / 2 - 10, 20, hex2rgb(toks['--lg-text-dim']))
    elif icon == 'lock':
        icon_lock(dd, right - 20 - 20, box_top + box_h / 2 - 10, 20, hex2rgb(toks['--lg-text-dim']))
    if eye:
        icon_eye(dd, x0 + 6 + 5, box_top + box_h / 2 - 10, 20, hex2rgb(toks['--lg-text-dim']))

    txt = value or placeholder
    colour = hex2rgb(toks['--lg-text']) if value else hex2rgb(toks['--lg-text-dim'])
    f = F.get(15.2, 'Regular')
    ty = box_top + box_h / 2 - 9
    if dir_ltr:
        text(dd, (x0 + (26 if eye else 20), ty), txt, f, colour)
    return box_top + box_h


def draw_button(d, F, toks, box, label, *, variant='primary', icon=None, circle=False):
    x0, y0, x1, y1 = box
    h = y1 - y0
    col_bg = hex2rgb(toks['--lg-btn-bg']) if variant == 'primary' else None
    col_tx = hex2rgb(toks['--lg-btn-text'] if variant == 'primary' else toks['--lg-gold-deep'])
    layer = Image.new('RGBA', d._image.size, (0, 0, 0, 0))
    dl = ImageDraw.Draw(layer)
    dl.rounded_rectangle([x0 * S, y0 * S, x1 * S, y1 * S], (h / 2) * S,
                         fill=(col_bg + (255,)) if col_bg else None,
                         outline=None if col_bg else hex2rgb(toks['--lg-gold']) + (255,),
                         width=max(1, int(1.5 * S)))
    d._image.alpha_composite(layer)
    dd = ImageDraw.Draw(d._image)
    f = F.get(15.52, 'Bold')
    fa = bool(re.search('[%s]' % FA_RANGE, label))
    tw = dd.textlength(label, font=f, direction='rtl' if fa else 'ltr')
    total = tw + (9 * S + 19 * S if icon else 0)
    cx = (x0 + x1) / 2 * S
    start = cx - total / 2
    if icon:
        icon_phone(dd, start / S, y0 + h / 2 - 9.5, 19, col_tx)
        start += (9 + 19) * S
    ty = y0 + h / 2 - 9.5
    text(dd, ((start + tw) / S if fa else start / S, ty), label, f, col_tx)
    return y1


def draw_divider(d, F, toks, box, label='یا'):
    x0, y0, x1, y1 = box
    dd = ImageDraw.Draw(d._image)
    f = F.get(12.8, 'Regular')
    tw = dd.textlength(label, font=f, direction='rtl')
    gap = 14
    cy = (y0 + y1) / 2
    cx = (x0 + x1) / 2 * S
    text(dd, ((cx + tw / 2) / S, cy - 8), label, f, hex2rgb(toks['--lg-text-dim']))
    col = hex2rgb(toks['--lg-divider'])
    dd.line([x0 * S, cy * S, (cx - tw / 2 - gap * S), cy * S], fill=col, width=max(1, S))
    dd.line([(cx + tw / 2 + gap * S), cy * S, x1 * S, cy * S], fill=col, width=max(1, S))
    return y1


def draw_brand_and_title(d, F, toks, x_right, y, salon):
    """نشان برند + نام + تیتر. برمی‌گرداند: ارتفاع مصرف‌شده."""
    box = (x_right - 46, y, x_right, y + 46)
    layer = Image.new('RGBA', d._image.size, (0, 0, 0, 0))
    dl = ImageDraw.Draw(layer)
    dl.rounded_rectangle([box[0] * S, box[1] * S, box[2] * S, box[3] * S], 15 * S,
                         fill=hex2rgb(toks['--lg-gold']) + (255,))
    d._image.alpha_composite(layer)
    icon_sparkle(ImageDraw.Draw(d._image), box[0] + 11, box[1] + 11, 24, hex2rgb(toks['--lg-btn-bg']))
    text(ImageDraw.Draw(d._image), (x_right - 58, y + 12), salon, F.get(16.32, 'Bold'),
         hex2rgb(toks['--lg-text']))
    return 46


# ───────────────────────── دو قالب ─────────────────────────

def render_desktop(out):
    toks, _ = read_tokens()
    W, H = 1000.0, 600.0
    form_share = 0.43478
    boundary = W * (1 - form_share)                  # در راست‌به‌چپ: فرم سمت راست
    img = radial_page(W, H + 120, hex2rgb(toks['--lg-page-1']), hex2rgb(toks['--lg-page-2']))
    # کارت با گوشه گرد
    card_mask = rounded_mask(img.size, (0, 60, W, 60 + H), 34)
    img = draw_layer(img, card_mask, hex2rgb(toks['--lg-surface']))

    # تصویر (ستون دوم = سمت چپ)
    photo = paste_cover(img, (0, 60, boundary, 60 + H),
                        os.path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'img', 'login-illustration.jpg'),
                        0.5, 0.30)

    # موج عمودی، روی مرز ستون‌ها
    wave_box = (boundary - 120, 60, boundary + 120, 60 + H)
    photo = draw_wave(photo, DESKTOP, wave_box, toks)

    # مهار موج و تصویر داخل گوشه‌های گرد کارت
    img = Image.composite(photo, img, card_mask)

    # محتوای فرم
    d = ImageDraw.Draw(img)
    F = Fonts()
    right = W - 44
    pad_block = 40.0
    content_h = 46 + 26 + (36 + 16 + (80 + 16 + 80) + 16 + 52 + 16 + 19 + 16 + 52) + 26 + 44
    y = 60 + max(pad_block, (H - content_h) / 2)

    draw_brand_and_title(d, F, toks, right, y, 'سالن زیبایی حدیث')
    y += 46 + 26
    text(d, (right, y), 'ورود به حساب', F.get(24, 'ExtraBold'), hex2rgb(toks['--lg-text']))
    y += 36 + 16
    left = boundary + 72
    y = draw_field(d, F, toks, (left, y, right, 0), 'شماره موبایل',
                   placeholder='09xxxxxxxxx', icon='phone', dir_ltr=True) + 16
    y = draw_field(d, F, toks, (left, y, right, 0), 'رمز عبور',
                   placeholder='••••••••', icon='lock', eye=True) + 16
    y = draw_button(d, F, toks, (left, y, right, y + 52), 'ورود') + 16
    y = draw_divider(d, F, toks, (left, y, right, y + 19)) + 16
    y = draw_button(d, F, toks, (left, y, right, y + 52), 'ورود با کد یکبار مصرف',
                    variant='ghost', icon='phone') + 26
    text(d, ((left + right) / 2, y + 12), 'رمز عبور را فراموش کرده‌اید؟',
         F.get(13.76, 'SemiBold'), hex2rgb(toks['--lg-gold-deep']))

    # ---------- برش ۱:۱ برای بازرسی خودِ خط طلایی ----------
    # چرا: رندر نهایی کوچک می‌شود و خط ۱٫۸ پیکسلی در آن نرم و بی‌قاضی می‌شود.
    # این برش بدون تغییر اندازه ذخیره می‌شود تا ضخامت و فاصله دو خط دیده شود.
    # ⚠ تصویر سوپرسمپل است: مختصات باید در ضریب S ضرب شوند، وگرنه برش از
    # گوشه بالا-چپ درمی‌آید (یک‌بار همین اتفاق افتاد).
    crop = img.crop((int((boundary - 40) * S), int(60 * S),
                     int((boundary + 230) * S), int(460 * S))).convert('RGB')
    crop.save(os.path.join(os.path.dirname(out), '04-gold-line.png'))

    img.resize((int(W * 0.72), int((H + 120) * 0.72)), Image.LANCZOS).convert('RGB').save(out)
    return out


def render_mobile(out):
    toks, _ = read_tokens()
    W = 390.0
    media_h = 371.4
    form_h = 581.0
    H = media_h + form_h - 30
    img = radial_page(W, H, hex2rgb(toks['--lg-page-1']), hex2rgb(toks['--lg-page-2']))

    # کارت: تمام‌عرض، بدون گردی
    img = draw_layer(img, rect_mask(img.size, (0, 0, W, media_h)), hex2rgb(toks['--lg-surface']))
    img = paste_cover(img, (0, 0, W, media_h),
                      os.path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'img', 'login-illustration.jpg'),
                      0.5, 0.24)

    # موج افقی
    wave_top = media_h - 96 + 30
    img = draw_wave(img, MOBILE, (0, wave_top, W, wave_top + 96), toks)

    # کارت شیشه‌ای فرم: بلور پس‌زمینه + گرادیان آلفا
    card = (12.0, media_h - 30, W - 12, H)
    blurred = img.filter(ImageFilter.GaussianBlur(18 * S))
    reg = (int(card[0] * S), int(card[1] * S), int(card[2] * S), int(card[3] * S))
    glass = blurred.crop(reg)
    solid_at = 140.0
    hh = reg[3] - reg[1]
    arr = np.array(glass).astype(float)
    yy = np.arange(hh) / S
    alpha = np.ones(hh)
    a0 = float(rgba(toks['--lg-glass-bg'])[3]) / 255.0
    fade = np.clip((yy - 44) / (solid_at - 44), 0, 1)
    alpha = a0 + (1 - a0) * fade
    ivory = np.array(hex2rgb(toks['--lg-surface']), float)
    al = alpha[:, None, None]
    arr[..., :3] = arr[..., :3] * (1 - al) + ivory * al
    arr[..., 3] = 255
    glass = Image.fromarray(arr.astype(np.uint8), 'RGBA')

    m = rounded_mask(img.size, card, 34)
    full = Image.new('RGBA', img.size, (0, 0, 0, 0))
    full.paste(glass, reg[:2])
    # سایه کارت شیشه‌ای روی موج
    img = drop_shadow(img, m, 0, 20, 26, hex2rgb(toks['--lg-wave-shadow']),
                      float(rgba(toks['--lg-wave-shadow'])[3]) / 255.0 * 0.55)
    img = Image.composite(full, img, m)

    # محتوای فرم
    d = ImageDraw.Draw(img)
    F = Fonts()
    right = W - 12 - 20
    y = card[1] + 30
    draw_brand_and_title(d, F, toks, right, y, 'سالن زیبایی حدیث')
    y += 46 + 26
    text(d, (right, y), 'ورود به حساب', F.get(24, 'ExtraBold'), hex2rgb(toks['--lg-text']))
    y += 36 + 26
    left = 12 + 20
    y = draw_field(d, F, toks, (left, y, right, 0), 'شماره موبایل',
                   placeholder='09xxxxxxxxx', icon='phone', dir_ltr=True) + 16
    y = draw_field(d, F, toks, (left, y, right, 0), 'رمز عبور',
                   placeholder='••••••••', icon='lock', eye=True) + 16
    y = draw_button(d, F, toks, (left, y, right, y + 52), 'ورود') + 16
    y = draw_divider(d, F, toks, (left, y, right, y + 19)) + 16
    y = draw_button(d, F, toks, (left, y, right, y + 52), 'ورود با کد یکبار مصرف',
                    variant='ghost', icon='phone') + 26
    text(d, ((left + right) / 2, y + 12), 'رمز عبور را فراموش کرده‌اید؟',
         F.get(13.76, 'SemiBold'), hex2rgb(toks['--lg-gold-deep']))

    img.resize((int(W), int(H)), Image.LANCZOS).convert('RGB').save(out)
    return out


if __name__ == '__main__':
    import os
    docs = os.path.join(ROOT, 'docs', 'login')
    os.makedirs(docs, exist_ok=True)
    a = render_desktop(os.path.join(docs, '03-desktop-render.png'))
    b = render_mobile(os.path.join(docs, '03-mobile-render.png'))
    for p in (a, b):
        print('%s  %d KB  %s' % (p, os.path.getsize(p) // 1024, Image.open(p).size))
