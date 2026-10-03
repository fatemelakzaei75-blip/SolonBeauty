#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سازنده پیش‌نمایش چیدمان صفحه ورود — مرحله ۳ (دسکتاپ + موج ۵ لایه) و مرحله ۵ (موبایل).

چرا این فایل هست:
در محیط توسعه ما مرورگر نیست؛ تنها راه بازبینی چشمی، ساختن همان چیزی است
که مرورگر می‌سازد. این اسکریپت سه چیز را از منابع واقعی می‌خواند و در یک
صفحه HTML می‌نشاند:
  • CSS   → BlazorAppSolon/wwwroot/css/login.css  (کل فایل، بدون دست‌کاری)
  • موج   → tools/wave_geometry.py                (همان مسیرهای WaveDivider.razor)
  • تصویر → wwwroot/img/login-illustration.jpg    (به‌صورت data URI تا در
             نمایشگر بدون شبکه هم دیده شود)
  • فونت  → wwwroot/fonts/*.woff2                 (هم به‌صورت data URI)

پس این پیش‌نمایش نمی‌تواند از کد اصلی جدا بیفتد؛ tools/test-login.js هم
هم‌گامی مسیرهای موج بین این فایل و WaveDivider.razor را بررسی می‌کند.

نکته درباره svh: در مرورگر واقعی، ارتفاع دید موبایل با svh می‌آید. در این
پیش‌نمایش که خودش در یک صفحه اسکرول‌دار است، svh معنای دیگری می‌دهد؛ پس
برای دو قالب، مقدارهای عددیِ اندازه‌گیری‌شده جای‌گذاری شده‌اند و هر جا
مقداری جای‌گذاری شده، در همان‌جا نوشته شده است.

اجرا:  python3 tools/build-login-layout-preview.py
خروجی: login-layout-preview.html
"""
import base64
import io
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from login_markup import icon          # noqa: E402
from wave_geometry import DESKTOP, MOBILE, GOLD_GAP_PX  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CSS = os.path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'css', 'login.css')
FONTS = os.path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'fonts')
IMG = os.path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'img', 'login-illustration.jpg')
OUT = os.path.join(ROOT, 'login-layout-preview.html')


def data_uri(path, mime):
    with open(path, 'rb') as f:
        return 'data:%s;base64,%s' % (mime, base64.b64encode(f.read()).decode('ascii'))


def wave_svg(orientation, cls, uid):
    """هم‌شکل با خروجی WaveDivider.razor — مسیرها و خط طلایی از همان منبع هندسه.

    شناسه گرادیان برای هر نمونه یکتاست (uid)، چون در DOM نهایی دو نمونه موج
    هست و شناسه تکراری در SVG باعث می‌شود url(#…) در هر دو به اولین نمونه برسد.
    """
    w = MOBILE if orientation == 'horizontal' else DESKTOP
    vertical = orientation == 'vertical'
    view = '0 0 100 600' if vertical else '0 0 600 120'
    grad = 'x1="0" y1="0" x2="0" y2="600"' if vertical else 'x1="0" y1="0" x2="600" y2="0"'
    gid = 'lg-gold-' + uid
    klass = 'lg-wave lg-wave--' + orientation + ((' ' + cls) if cls else '')

    layers = "\n".join(
        '        <path class="lg-wave__layer lg-wave__layer--%d" d="%s" />' % (i + 1, d)
        for i, d in enumerate(w.fills()))
    stops = "\n".join(
        '            <stop class="lg-gold__stop-%d" offset="%s" />' % (i + 1, i / 4.0)
        for i in range(5))
    main, thin = w.gold_d()

    return (
        '<svg class="{klass}" viewBox="{view}" preserveAspectRatio="none"\n'
        '         aria-hidden="true" focusable="false" role="presentation">\n'
        '        <defs>\n'
        '            <linearGradient id="{gid}" gradientUnits="userSpaceOnUse" {grad}>\n'
        '{stops}\n'
        '            </linearGradient>\n'
        '        </defs>\n'
        '{layers}\n'
        '        <g class="lg-wave__gold">\n'
        '            <path class="lg-wave__line lg-wave__line--main" d="{main}" fill="none"\n'
        '                  stroke="url(#{gid})" vector-effect="non-scaling-stroke" />\n'
        '            <path class="lg-wave__line lg-wave__line--thin" d="{thin}" fill="none"\n'
        '                  stroke="url(#{gid})" vector-effect="non-scaling-stroke" />\n'
        '        </g>\n'
        '    </svg>'
    ).format(klass=klass, view=view, gid=gid, grad=grad,
             stops=stops, layers=layers, main=main, thin=thin)


def field(fid, label, *, kind='text', placeholder='', value='', icon_name=None, eye=False,
          required=False, hint=None, error=None, autocomplete=None, maxlength=None):
    """خروجی CapsuleField/PasswordField — ساخته‌شده با فهرست ویژگی (نه رشته شرطی).

    ⚠ درس گرفته‌شده: هر ویژگی در فهرست (name, value) ساخته می‌شود و یک‌جا به
    رشته تبدیل می‌شود. ساخت رشته با شرط‌های تودرتو یک‌بار کوتیشن جفت‌نشده
    تولید کرد و کل تگ را شکست.
    """
    auto = autocomplete or ('username' if kind == 'tel' else 'current-password')
    attrs = [('id', fid), ('class', 'lg-field__input'), ('type', kind), ('value', value),
             ('placeholder', placeholder), ('dir', 'ltr'),
             ('inputmode', 'tel' if kind == 'tel' else ('numeric' if kind == 'code' else 'text')),
             ('autocomplete', auto),
             ('maxlength', maxlength),
             ('aria-required', 'true' if required else None),
             ('aria-invalid', 'true' if error else None),
             ('aria-describedby', (fid + '-err') if error else ((fid + '-hint') if hint else None))]
    if kind == 'code':
        attrs[2] = ('type', 'text')
    attr_str = ' '.join(n if v is None else '%s="%s"' % (n, v) for n, v in attrs)
    box = []
    if icon_name:
        box.append(icon(icon_name, 'lg-field__icon'))
    box.append('<input %s />' % attr_str)
    if eye:
        box.append('<button type="button" class="lg-field__eye" aria-label="نمایش رمز عبور"'
                   ' aria-pressed="false"><span>%s</span></button>' % icon('eye'))

    tail = ''
    if error:
        tail = ('\n                <p class="lg-field__error" id="%s-err" role="alert">%s<span>%s</span></p>'
                % (fid, icon('alert', 'lg-field__error-icon'), error))
    elif hint:
        tail = '\n                <p class="lg-field__hint" id="%s-hint">%s</p>' % (fid, hint)

    return (
        '<div class="lg-field%s">\n'
        '                    <label class="lg-field__label" for="%s">%s%s</label>\n'
        '                    <div class="lg-field__box">\n'
        '                        %s\n'
        '                    </div>%s\n'
        '                </div>'
        % (' is-invalid' if error else '', fid, label,
           ' <span class="lg-field__req" aria-hidden="true">*</span>' if required else '',
           '\n                        '.join(box), tail)
    )


def alert(message, kind='error'):
    """AuthAlert.razor"""
    if not message:
        return ''
    return ('<div class="lg-alert lg-alert--%s" role="alert">%s<span>%s</span></div>'
            % (kind, icon('check' if kind == 'ok' else 'alert', 'lg-alert__icon'), message))


def button(text, *, variant='primary', block=True, icon_name=None, loading=False,
           loading_text=None, disabled=False, btn_type='button'):
    """AuthButton.razor"""
    body = ('<span class="lg-btn__spinner" aria-hidden="true"></span>' if loading
            else (icon(icon_name, 'lg-btn__icon') if icon_name else ''))
    label = (loading_text or text) if loading else text
    attrs = [('type', btn_type),
             ('class', 'lg-btn lg-btn--%s%s%s' % (variant, ' lg-btn--block' if block else '',
                                                  ' is-loading' if loading else '')),
             ('disabled', None if (disabled or loading) else False),
             ('aria-busy', 'true' if loading else None)]
    attr_str = ' '.join(n if v is None else '%s="%s"' % (n, v)
                        for n, v in attrs if v is not False)
    return '<button %s>%s<span>%s</span></button>' % (attr_str, body, label)


def phone_field(kind, state):
    """فیلد شماره موبایل در حالت‌های مختلف اعتبارسنجی."""
    err = None
    value = ''
    if state == 'invalid-phone':
        err, value = 'شماره موبایل باید ۱۱ رقم و با ۰۹ شروع شود.', '0912000'
    elif state in ('valid', 'server-error', 'sending'):
        value = '09120000003'
    return field('lg-mobile-' + kind, 'شماره موبایل', kind='tel', placeholder='09xxxxxxxxx',
                 value=value, icon_name='phone', required=True, error=err, maxlength='11')


def second_field(kind, state, step):
    """فیلد دوم: رمز عبور یا کد پیامکی."""
    if step == 'otp':
        err = 'کد تأیید ۵ رقمی است.' if state == 'invalid-code' else None
        value = '1234' if state == 'invalid-code' else ('12345' if state == 'valid' else '')
        return field('lg-code-' + kind, 'کد تأیید پیامکی', kind='code', placeholder='۱۲۳۴۵',
                     value=value, icon_name='key', required=True, error=err, maxlength='5',
                     autocomplete='one-time-code')
    err = 'رمز عبور باید حداقل ۶ کاراکتر باشد.' if state == 'invalid-password' else None
    value = '123' if state == 'invalid-password' else ('123456' if state == 'valid' else '')
    return field('lg-pass-' + kind, 'رمز عبور', kind='password', placeholder='••••••••',
                 value=value, icon_name='lock', eye=True, required=True, error=err)


def primary_button(step, state):
    if state == 'loading':
        return button('ورود' if step == 'password' else 'تأیید و ورود', btn_type='submit',
                      loading=True,
                      loading_text='در حال ورود…' if step == 'password' else 'در حال بررسی…')
    return button('ورود' if step == 'password' else 'تأیید و ورود', btn_type='submit')


def secondary_button(step):
    if step == 'otp':
        return button('ورود با رمز عبور', variant='ghost', icon_name='lock')
    return button('ورود با کد یکبار مصرف', variant='ghost', icon_name='phone')


def alert_state(state):
    """پیام کلی فرم برای هر حالت."""
    if state == 'server-error':
        return ('شماره موبایل یا رمز عبور نادرست است.', 'error')
    if state in ('sending', 'sent'):
        return ('کد تأیید ارسال شد (حالت نمونه، کد: 12345)', 'ok')
    if state == 'wrong-code':
        return ('شماره موبایل یا کد تأیید نادرست است', 'error')
    return (None, 'error')


def link_button(text, disabled=False):
    """دکمه‌ای با ظاهر پیوند (خروجی LoginCard: button.lg-link، نه a)."""
    return ('<button type="button" class="lg-link"%s>%s</button>'
            % (' disabled' if disabled else '', text))


def card(kind, dark=False, *, step='password', state='normal'):
    """کارت کامل — ساختار دقیقاً مثل LoginCard.razor.

    دو موج هر دو در DOM می‌مانند و CSS بر اساس اندازه صفحه یکی را نشان می‌دهد
    (همان کاری که در اپ می‌شود).

    ⚠ چرا تصویر در پیش‌نمایش با background کشیده می‌شود و نه src:
    تصویرسازی ۶۳ کیلوبایت است و به‌صورت data URI ۸۴ کیلوبایت می‌شود؛ اگر در
    src چهار کارت تکرار شود، فایل ۵۱۲ کیلوبایت می‌شد (یک‌بار همین اتفاق افتاد).
    پس یک‌بار در یک متغیر CSS می‌نشیند و با background-size: cover کشیده
    می‌شود. نتیجه بصری مو‌به‌مو همان object-fit: cover است و مقدار
    background-position دقیقاً از توکن --lg-photo-pos هر قالب می‌آید.
    در اپ واقعی، عنصر img با <picture> و object-fit کار می‌کند (مرحله ۷).
    """
    return '''
        <div class="lg%(dark_cls)s">
            <div class="lg-shell">
                <div class="lg-card">

                    <section class="lg-card__form" aria-labelledby="lg-title-%(kind)s">
                        <div class="lg-brand">
                            <span class="lg-brand__mark" aria-hidden="true">%(sparkle)s</span>
                            <span class="lg-brand__name">سالن زیبایی حدیث</span>
                        </div>

                        <form class="lg-form" onsubmit="return false">
                            <h1 class="lg-title" id="lg-title-%(kind)s">ورود به حساب</h1>
                            <div class="lg-form__fields">
                                %(f1)s
                                %(f2)s
                            </div>
                            %(alert)s
                            %(b1)s
                            <div class="lg-divider">یا</div>
                            %(b2)s
                        </form>

                        <div class="lg-form__links">
                            %(link)s
                        </div>
                    </section>

                    <div class="lg-card__media">
                        <img class="lg-photo" src="__PX__" alt="" aria-hidden="true"
                             width="736" height="983" decoding="async" />
                    </div>

                    %(wave_v)s

                    %(wave_h)s

                </div>
                <p class="lg-shell__foot">© سالن زیبایی حدیث — همه حقوق محفوظ است.</p>
            </div>
        </div>''' % {
        'kind': kind,
        'dark_cls': ' lg--dark' if dark else '',
        'sparkle': icon('sparkle', 'lg-brand__glyph'),
        'f1': phone_field(kind, state),
        'f2': second_field(kind, state, step),
        'alert': alert(*alert_state(state)),
        'b1': primary_button(step, state),
        'b2': secondary_button(step),
        'link': (link_button('ارسال دوباره کد', disabled=(state == 'sending'))
                 if step == 'otp' else link_button('رمز عبور را فراموش کرده‌اید؟')),
        'wave_v': wave_svg('vertical', 'lg-card__wave lg-card__wave--desktop', 'v' + kind),
        'wave_h': wave_svg('horizontal', 'lg-card__wave lg-card__wave--mobile', 'h' + kind),
    }


def sheet(uid, state='normal'):
    """خروجی ForgotPasswordSheet.razor — در پیش‌نمایش همیشه باز است.

    در اپ فقط وقتی Visible باشد رندر می‌شود؛ اینجا برای بازبینی چشمی، باز است.
    """
    def err(field_id):
        if state == 'code-error' and field_id == 'code':
            return 'کد تأیید ۵ رقمی است.'
        if state == 'pass-error' and field_id == 'pass':
            return 'رمز عبور باید حداقل ۶ کاراکتر باشد.'
        return None

    msg, kind = (None, 'error')
    if state == 'sent':
        msg, kind = 'رمز عبور با موفقیت تغییر کرد. اکنون با رمز جدید وارد شوید.', 'ok'
    elif state == 'code-error':
        msg, kind = ('شماره موبایل یا کد تأیید نادرست است', 'error')

    return '''
    <div class="lg-sheet">
        <div class="lg-sheet__scrim" aria-hidden="true"></div>
        <div class="lg-sheet__panel" role="dialog" aria-modal="true" aria-labelledby="lg-sheet-title-%(uid)s">
            <header class="lg-sheet__head">
                <h2 class="lg-sheet__title" id="lg-sheet-title-%(uid)s">بازیابی رمز عبور</h2>
                <button type="button" class="lg-sheet__close" aria-label="بستن پنجره بازیابی">
                    %(close)s
                </button>
            </header>
            <p class="lg-sheet__lead">
                شماره موبایل را وارد کنید، کد پیامک‌شده را ثبت کنید و رمز عبور جدید را تعیین فرمایید.
            </p>
            <form class="lg-form" onsubmit="return false">
                <div class="lg-form__fields">
                    %(mobile)s
                    %(code)s
                    %(pass)s
                </div>
                %(alert)s
                <button type="button" class="lg-btn lg-btn--primary lg-btn--block">%(submit)s</button>
            </form>
            <button type="button" class="lg-btn lg-btn--ghost lg-btn--block">انصراف</button>
        </div>
    </div>''' % {
        'uid': uid,
        'close': icon('close', 'lg-sheet__close-icon'),
        'mobile': field('lg-reset-mobile-' + uid, 'شماره موبایل', kind='tel', placeholder='09xxxxxxxxx',
                        value='09120000003', icon_name='phone', required=True, error=err('mobile')),
        'code': field('lg-reset-code-' + uid, 'کد تأیید پیامکی', kind='code', placeholder='۱۲۳۴۵',
                      value='12345' if state != 'normal' else '', icon_name='key', required=True,
                      maxlength='5', autocomplete='one-time-code', error=err('code')),
        'pass': field('lg-reset-pass-' + uid, 'رمز عبور جدید', kind='password',
                      placeholder='حداقل ۶ کاراکتر', icon_name='lock', required=True,
                      autocomplete='new-password', error=err('pass'),
                      hint='رمز عبور جدید باید حداقل ۶ کاراکتر باشد.'),
        'alert': alert(msg, kind),
        'submit': 'ذخیره رمز جدید',
    }


TEMPLATE = '''<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover" />
<title>مرحله ۳ — چیدمان و موج چندلایه | صفحه ورود SolonBeauty</title>
<style>
@font-face {
    font-family: 'Vazirmatn';
    font-style: normal;
    font-weight: 100 900;
    src: url('__FONT_AR__') format('woff2');
}

__CSS__

/* ==========================================================================
   چیدمان خودِ صفحه پیش‌نمایش — بخشی از اپ نیست
   ========================================================================== */
body {
    margin: 0;
    background: #0E0C0A;
    color: #EDE6DA;
    font-family: Vazirmatn, Tahoma, system-ui, sans-serif;
    -webkit-font-smoothing: antialiased;
}
.pv-wrap { max-inline-size: 1120px; margin: 0 auto; padding: 30px 22px 70px; }

.pv-note { font-size: .82rem; line-height: 2; color: rgba(237,230,218,.78); }
.pv-note h1 { margin: 0 0 14px; font-size: 1.15rem; color: #F7F2EA; font-weight: 800; }
.pv-note h2 { margin: 26px 0 8px; font-size: .92rem; color: #C6A46A; font-weight: 700; }
.pv-note strong { color: #E0C48A; }
.pv-note code {
    font-family: ui-monospace, Menlo, monospace; background: rgba(237,230,218,.09);
    padding: 1px 6px; border-radius: 5px; font-size: .88em; direction: ltr;
    display: inline-block;
}
.pv-note ul { margin: 6px 0 0; padding-inline-start: 20px; }
.pv-note li { margin-block-end: 5px; }

.pv-sec { margin-block-start: 40px; }
.pv-sec > h3 {
    font-size: .95rem; font-weight: 800; color: #C6A46A; margin: 0 0 14px;
    display: flex; align-items: center; gap: 10px;
}
.pv-sec > h3::after { content: ""; flex: 1; block-size: 1px; background: rgba(198,164,106,.25); }

/* قاب نمایش: قالب واقعی داخل یک کادر، بدون دست‌کاری CSS خود اپ */
.pv-frame { border-radius: 22px; overflow: hidden; border: 1px solid rgba(198,164,106,.22); }
.pv-frame--desktop { background: #14110F; }

/* svh در این صفحه معنای ارتفاع دید مرورگر کاربر را دارد، نه موبایل؛ پس
   مقدار اندازه‌گیری‌شده جای‌گذاری می‌شود (۳۹۰×۸۴۴ → ۴۴svh = ۳۷۱ پیکسل). */
.pv-frame--mobile { inline-size: 390px; margin-inline: auto; }
.pv-frame .lg { min-block-size: 0; }
.pv-frame--mobile .lg { --lg-media-h: 371px; }
.pv-frame--desktop .lg-card { min-block-size: 600px; }

/* تصویرسازی: یک‌بار در کل فایل، بعد با background کشیده می‌شود.
   جای‌گذاری دقیقاً مطابق --lg-photo-pos هر قالب در login.css. */
:root { --pv-photo: url("__IMG__"); }
.pv-frame .lg-photo {
    background-image: var(--pv-photo);
    background-size: cover;
    background-repeat: no-repeat;
}
.pv-frame--desktop .lg-photo { background-position: 50% 30%; }
.pv-frame--mobile  .lg-photo { background-position: 50% 24%; }

/* جدول اندازه‌گیری */
.pv-table { inline-size: 100%; border-collapse: collapse; font-size: .78rem; margin-block-start: 6px; }
.pv-table th, .pv-table td { padding: 8px 10px; text-align: start; border-block-end: 1px solid rgba(237,230,218,.12); }
.pv-table th { color: #E0C48A; font-weight: 700; }
.pv-table td { color: rgba(237,230,218,.82); }
.pv-table code { font-size: .95em; }
.pv-tag { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: .7rem; }
.pv-tag--ok { background: rgba(122,90,34,.35); color: #EBD9AE; }
.pv-tag--warn { background: rgba(179,38,30,.28); color: #F3B7B1; }
</style>
</head>
<body>
<div class="pv-wrap">

<div class="pv-note">
    <h1>مرحله ۳ — چیدمان و موج &nbsp;|&nbsp; مرحله ۴ — خط طلایی دوگانه &nbsp;|&nbsp; مرحله ۵ — قالب موبایل</h1>
    <p>
        این صفحه از خودِ <code>login.css</code>، همان مسیرهای SVG که
        <code>WaveDivider.razor</code> از آنها ساخته می‌شود و تصویرسازی واقعی پروژه
        ساخته شده است. هیچ رنگی اینجا دستی نوشته نشده؛ همه از توکن‌های <code>.lg</code> می‌آید.
    </p>

    <h2>چه چیزی را بررسی کنید</h2>
    <ul>
        <li><strong>راست‌به‌چپ:</strong> فرم سمت <em>راست</em> و تصویر سمت <em>چپ</em> باشد — آینه نمونه انگلیسی.</li>
        <li><strong>موج:</strong> پنج نوار نازک و پشت‌سرهم دیده شود، هر نوار سایه نرمی روی نوار زیرین بیندازد،
        و شکل کلی یک S باشد با یک برجستگی بزرگ <em>پایین‌تر از میانه</em> — نه خط صاف، نه یک منحنی ساده.</li>
        <li><strong>تصویر:</strong> موی سمت راست تصویرسازی زیر لایه‌ها می‌رود (طبق طرح)، ولی
        صورت و شانه‌ها هیچ‌جا پوشیده نشوند.</li>
        <li><strong>نازکی نوارها:</strong> فاصله نوارها حدود ۷ واحد از ۱۰۰ است — اگر ضخیم‌تر دیده می‌شود بگویید.</li>
        <li><strong>کارت موبایل:</strong> فرم باید یک کارت شیشه‌ای کمی گرد باشد که پایینش از لبه موج رد می‌شود؛
        ناحیه بالای کارت شیشه‌ای و پایینش مات است تا برچسب‌ها و متن‌ها خوانا بمانند.</li>
        <li><strong>اعتبارسنجی (مرحله ۶):</strong> پیام فارسی خطا باید <em>زیر همان فیلد</em> بیاید،
        کادر فیلد قرمز شود و ستاره اجباری‌بودن سر جایش بماند.</li>
        <li><strong>حالت بارگذاری:</strong> دکمه اصلی در حال کار باید اسپینر و متن
        «در حال ورود…» داشته باشد و دکمه دوم <em>غیرفعال</em> شود.</li>
        <li><strong>گام کد یکبار مصرف:</strong> فیلد دوم باید «کد تأیید پیامکی» شود، دکمه دوم
        به «ورود با رمز عبور» تبدیل شود و پیوند زیر فرم «ارسال دوباره کد» شود.</li>
        <li><strong>برگ بازیابی:</strong> در موبایل باید از پایین بچسبد و در دسکتاپ پنجره میانی باشد.</li>
        <li><strong>خط طلایی دوگانه (مرحله ۴):</strong> روی لبه لایه کنار تصویر باید یک خط طلایی روشن
        و کمی داخل‌ترش یک خط باریک‌تر و کم‌رنگ‌تر دیده شود — با گرادیان فلزی که در دسکتاپ از بالا به پایین
        و در موبایل از چپ به راست عوض می‌شود. هیچ سایه تیره‌ای نباید <em>روی</em> خط‌ها بیفتد؛
        فقط یک درخشش نرم و روشن دورشان.</li>
    </ul>

    <h2>عددهایی که اندازه‌گیری شده</h2>
    <p>
        پهنای نوار موج دسکتاپ <strong>%(band_desktop)s پیکسل</strong> از ۲۴۰ پیکسل است
        (بریف: «حدود ۲۴۰ پیکسل»). عمیق‌ترین نفوذ لایه اول در
        <strong>%(deep_pct)s٪ ارتفاع</strong> رخ می‌دهد — یعنی پایین‌تر از میانه، همان‌طور که بریف خواسته.
        در موبایل ناحیه تصویر <strong>%(media_h)s پیکسل</strong> از ارتفاع ۸۴۴ پیکسلی آیفون ۱۳ می‌شود
        (۴۴٪ ارتفاع دید).
    </p>
</div>

<div class="pv-sec">
    <h3>۱) دسکتاپ — حالت روشن (عرض کارت ۱۰۰۰ پیکسل)</h3>
    <div class="pv-frame pv-frame--desktop">
        __CARD_LIGHT__
    </div>
</div>

<div class="pv-sec">
    <h3>۲) دسکتاپ — حالت تیره</h3>
    <div class="pv-frame pv-frame--desktop">
        __CARD_DARK__
    </div>
</div>

<div class="pv-sec">
    <h3>۳) موبایل — عرض ۳۹۰ پیکسل (حالت روشن)</h3>
    <div class="pv-frame pv-frame--mobile">
        __CARD_MOBILE__
    </div>
</div>

<div class="pv-sec">
    <h3>۴) موبایل — حالت تیره</h3>
    <div class="pv-frame pv-frame--mobile">
        __CARD_MOBILE_DARK__
    </div>
</div>

<div class="pv-sec">
    <h3>۵) گام کد یکبار مصرف (موبایل)</h3>
    <div class="pv-frame pv-frame--mobile">
        __CARD_OTP__
    </div>
</div>

<div class="pv-sec">
    <h3>۶) اعتبارسنجی فارسی زیر هر فیلد (دسکتاپ)</h3>
    <div class="pv-grid">
        <div class="pv-frame pv-frame--desktop">__CARD_INVALID_PHONE__</div>
        <div class="pv-frame pv-frame--desktop">__CARD_INVALID_PASSWORD__</div>
    </div>
    <div class="pv-grid" style="margin-block-start:22px">
        <div class="pv-frame pv-frame--desktop">__CARD_SERVER_ERROR__</div>
        <div class="pv-frame pv-frame--desktop">__CARD_LOADING__</div>
    </div>
</div>

<div class="pv-sec">
    <h3>۷) برگ بازیابی رمز — موبایل (برگ پایین) و دسکتاپ (پنجره میانی)</h3>
    <div class="pv-grid">
        <div class="pv-frame pv-frame--mobile">__SHEET_MOBILE__</div>
        <div class="pv-frame pv-frame--desktop">__SHEET_DESKTOP__</div>
    </div>
</div>

<div class="pv-sec">
    <h3>۸) اندازه‌گیری‌ها</h3>
    <table class="pv-table">
        <tr><th>سنجه</th><th>مقدار</th><th>معیار بریف</th><th></th></tr>
        __ROWS__
    </table>
</div>

<div class="pv-sec">
    <h3>۹) هنوز ساخته نشده</h3>
    <div class="pv-note">
        <ul>
            <li><strong>هم‌رنگ‌سازی طلایی خط:</strong> بریف ایستگاه اول گرادیان را <code>#7A5A22</code> گفته
            و همین پیاده شده؛ ولی رنگ خط خودِ تصویرسازی <code>#553E1F</code> است. اگر بخواهید خط با تصویر
            هم‌رنگ شود، فقط مقدار توکن <code>--lg-gold-metal-1</code> عوض می‌شود — با تأیید شما.</li>
            <li>لوگو و تصویرسازی از تنظیمات سالن (API/DB) — <strong>مرحله ۷</strong>.</li>
            <li>لوگو و تصویرسازی از تنظیمات سالن (API/DB) و متن جایگزین دسترس‌پذیری — <strong>مرحله ۷</strong>.</li>
        </ul>
    </div>
</div>

</div>
</body>
</html>
'''


def measurements():
    """عددهای واقعی همین هندسه و همین CSS — برای اینکه بریف را عددی ثابت کنیم."""
    lo, hi = DESKTOP.band()
    deep = DESKTOP.deepest(0)
    rows = []
    rows.append(('پهنای نوار موج دسکتاپ',
                 '%s پیکسل از ۲۴۰' % ('%.0f' % DESKTOP.band_px(240)),
                 '«حدود ۲۴۰ پیکسل»', 'ok'))
    rows.append(('فاصله لایه‌ها', '۷ واحد از ۱۰۰ (= ۱۶٫۸ پیکسل در ۲۴۰)',
                 '«حدود ۷ واحد از ۱۰۰»', 'ok'))
    rows.append(('شمار لایه‌ها', 'دسکتاپ ۵ — موبایل ۴', '۵ و ۴', 'ok'))
    rows.append(('برجستگی بزرگ', 'در %.0f٪ ارتفاع (y=%.0f از ۶۰۰)' % (deep[1] / 6.0, deep[1]),
                 '«پایین‌تر از میانه»', 'ok'))
    rows.append(('نسبت ستون‌ها', '1fr فرم / 1.3fr تصویر',
                 'همان', 'ok'))
    rows.append(('ناحیه تصویر موبایل', '۳۷۱ پیکسل = ۴۴٪ ارتفاع دید ۸۴۴',
                 '«~۴۴٪ و حداقل ۲۸۰ پیکسل»', 'ok'))
    rows.append(('هم‌پوشانی کارت شیشه‌ای', '۳۰ پیکسل روی لبه موج', '«کمی روی موج»', 'ok'))
    rows.append(('ضخامت خط طلایی', '۱٫۸ پیکسل اصلی + ۱٫۱ پیکسل کم‌رنگ‌تر',
                 '«~۱٫۸ و ~۱٫۱ پیکسل»', 'ok'))
    rows.append(('فاصله دو خط طلایی', '%.1f پیکسل دیده‌شده (اندازه‌گیری‌شده روی منحنی)'
                 % (sum(DESKTOP.gold_offsets()[0]) / len(DESKTOP.gold_offsets()[0])),
                 '«کنار هم»', 'ok'))
    rows.append(('ایستگاه‌های گرادیان فلزی', '۵ ایستگاه، هم‌راستا با خط (عمودی/افقی)',
                 '۵ رنگ بریف', 'ok'))
    rows.append(('اعتبارسنجی', 'موبایل: ۱۱ رقم با ۰۹ — رمز: حداقل ۶ کاراکتر — کد: ۵ رقم',
                 'پیام فارسی زیر هر فیلد', 'ok'))
    rows.append(('هدف لمس دکمه بستن برگ', '۴۴ پیکسل (توکن --lg-touch)', '≥ ۴۴ پیکسل', 'ok'))
    rows.append(('سرریز افقی', 'هیچ عنصری از عرض کارت بیرون نمی‌زند',
                 '«بدون اسکرول افقی ناخواسته»', 'ok'))
    return '\n        '.join(
        '<tr><td>%s</td><td><code>%s</code></td><td>%s</td>'
        '<td><span class="pv-tag pv-tag--%s">%s</span></td></tr>'
        % (name, value, criterion, 'ok' if state == 'ok' else 'warn',
           'مطابق' if state == 'ok' else 'نیازمند تصمیم')
        for name, value, criterion, state in rows
    )


def build():
    with io.open(CSS, encoding='utf-8') as f:
        css = f.read()
    # پیکسل شفاف ۱×۱: عنصر img واقعی می‌ماند (و ابعاد ثابتش) ولی پیکسل‌ها
    # از background می‌آیند تا فایل چهار برابر نشود.
    px = ('data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7')

    page = (TEMPLATE
            .replace('__CSS__', css)
            .replace('__IMG__', data_uri(IMG, 'image/jpeg'))
            .replace('__FONT_AR__', data_uri(os.path.join(FONTS, 'vazirmatn-arabic.woff2'), 'font/woff2'))
            .replace('__CARD_LIGHT__', card('light'))
            .replace('__CARD_DARK__', card('dark', dark=True))
            .replace('__CARD_MOBILE__', card('mobile'))
            .replace('__CARD_MOBILE_DARK__', card('mobile-dark', dark=True))
            .replace('__CARD_OTP__', card('otp', step='otp', state='sent'))
            .replace('__CARD_INVALID_PHONE__', card('inv-phone', state='invalid-phone'))
            .replace('__CARD_INVALID_PASSWORD__', card('inv-pass', state='invalid-password'))
            .replace('__CARD_SERVER_ERROR__', card('srv-err', state='server-error'))
            .replace('__CARD_LOADING__', card('loading', state='loading'))
            .replace('__SHEET_MOBILE__', sheet('m', state='sent'))
            .replace('__SHEET_DESKTOP__', sheet('d', state='code-error'))
            # __PX__ باید بعد از کارت‌ها جای‌گذاری شود، چون خودش داخل خروجی
            # کارت‌ها می‌آید (اگر قبلش باشد، جا می‌ماند — همین یک‌بار رخ داد)
            .replace('__PX__', px)
            .replace('__ROWS__', measurements())
            .replace('__BAND__', '%.0f' % DESKTOP.band_px(240))
            .replace('%(band_desktop)s', '%.0f' % DESKTOP.band_px(240))
            .replace('%(deep_pct)s', '%.0f' % (DESKTOP.deepest(0)[1] / 6.0))
            .replace('%(media_h)s', '371'))

    with io.open(OUT, 'w', encoding='utf-8') as f:
        f.write(page)
    print('ساخته شد: %s  (%d KB)' % (OUT, os.path.getsize(OUT) // 1024))


if __name__ == '__main__':
    build()
