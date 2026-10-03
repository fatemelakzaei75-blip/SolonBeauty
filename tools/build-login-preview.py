#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سازنده پیش‌نمایش کامپوننت‌های پایه صفحه ورود (مرحله ۲).

CSS را از فایل واقعی پروژه می‌خواند (login.css) و آیکون‌ها را از منبع مشترک
(login_markup.py) — پس پیش‌نمایش هرگز از کد اصلی جدا نمی‌افتد.
tools/test-login.js هر دو پیوند را بررسی می‌کند.

بازبینی چشمی: چون در محیط توسعه ما مرورگر نیست، تنها راه تأیید ظاهر همین
فایل است.

اجرا:  python3 tools/build-login-preview.py
خروجی: login-preview.html
"""
import io
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from login_markup import icon   # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CSS_SRC = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "css", "login.css")
OUT = os.path.join(ROOT, "login-preview.html")


# ───────────────────────── اجزای سازنده ─────────────────────────
# هر تابع، خروجی HTML کامپوننت رِیزور متناظر را بازتولید می‌کند.

def capsule(fid, label, *, kind="text", value="", placeholder="", icon_name=None,
            hint=None, error=None, disabled=False, required=False, dir_attr=None,
            toggle=False, cls=""):
    """CapsuleField.razor / PasswordField.razor

    ویژگی‌ها با یک فهرست ساخته می‌شوند نه با شرط‌های تودرتو در رشته. دلیل: نسخه
    قبلی همین تابع یک کوتیشن جاافتاده تولید می‌کرد (dir="ltr />) و HTML را
    می‌شکست. ساختن ویژگی‌ها در یک جا، این دسته خطا را ریشه‌کن می‌کند.
    """
    classes = ["lg-field"]
    if error:
        classes.append("is-invalid")
    if disabled:
        classes.append("is-disabled")
    if cls:
        classes.append(cls)

    # ---------- ویژگی‌های ورودی ----------
    attrs = [("id", fid), ("class", "lg-field__input"), ("type", kind), ("value", value)]

    if placeholder:
        attrs.append(("placeholder", placeholder))
    if dir_attr:
        attrs.append(("dir", dir_attr))
    if error:
        attrs.append(("aria-invalid", "true"))
        attrs.append(("aria-describedby", fid + "-err"))
    elif hint:
        attrs.append(("aria-describedby", fid + "-hint"))
    if required:
        attrs.append(("aria-required", "true"))
    if disabled:
        attrs.append(("disabled", None))

    attr_str = " ".join(
        name if val is None else '%s="%s"' % (name, val)
        for name, val in attrs
    )
    input_html = "<input %s />" % attr_str

    box_parts = []
    if icon_name:
        box_parts.append(icon(icon_name, "lg-field__icon"))
    box_parts.append(input_html)

    if toggle:
        # هر دو آیکون رندر می‌شوند و با CSS جابه‌جا می‌شوند — همان کاری که
        # PasswordField در Blazor با شرط انجام می‌دهد.
        box_parts.append(
            '<button type="button" class="lg-field__eye"'
            ' aria-label="نمایش رمز عبور" aria-pressed="false"'
            ' aria-controls="%s" data-eye>'
            '<span data-icon-show>%s</span><span data-icon-hide hidden>%s</span>'
            "</button>" % (fid, icon("eye"), icon("eye-off"))
        )

    tail = ""
    if error:
        tail = ('\n    <p class="lg-field__error" id="%s-err" role="alert">%s'
                "<span>%s</span></p>" % (fid, icon("alert", "lg-field__error-icon"), error))
    elif hint:
        tail = '\n    <p class="lg-field__hint" id="%s-hint">%s</p>' % (fid, hint)

    req = ' <span class="lg-field__req" aria-hidden="true">*</span>' if required else ""

    return (
        '<div class="%s">\n'
        '    <label class="lg-field__label" for="%s">%s%s</label>\n'
        '    <div class="lg-field__box">\n'
        '      %s\n'
        '    </div>%s\n'
        '  </div>' % (" ".join(classes), fid, label, req, "\n      ".join(box_parts), tail)
    )


def button(text, *, variant="primary", block=False, loading=False,
           disabled=False, icon_name=None, loading_text=None, btn_type="button",
           cls=""):
    """AuthButton.razor"""
    kls = ["lg-btn", f"lg-btn--{variant}"]
    if block:
        kls.append("lg-btn--block")
    if loading:
        kls.append("is-loading")
    if cls:
        kls.append(cls)

    body = []
    if loading:
        body.append('<span class="lg-btn__spinner" aria-hidden="true"></span>')
    elif icon_name:
        body.append(icon(icon_name, "lg-btn__icon"))

    label = (loading_text or text) if loading else text
    body.append(f"<span>{label}</span>")

    dis = " disabled" if (disabled or loading) else ""
    busy = ' aria-busy="true"' if loading else ""

    return (f'<button type="{btn_type}" class="{" ".join(kls)}"{dis}{busy}>'
            f'{"".join(body)}</button>')


def divider(text="یا"):
    return f'<div class="lg-divider">{text}</div>'


def alert(text, kind="error"):
    ic = icon("alert" if kind == "error" else "check", "lg-alert__icon")
    return f'<div class="lg-alert lg-alert--{kind}">{ic}<span>{text}</span></div>'


def link(text):
    return f'<a class="lg-link" href="#" onclick="return false">{text}</a>'


# ───────────────────────── صفحه ─────────────────────────

TEMPLATE = """<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover" />
<title>مرحله ۲ — کامپوننت‌های پایه صفحه ورود | SolonBeauty</title>
<style>
__CSS__

/* ==========================================================================
   چیدمان خودِ صفحه پیش‌نمایش — بخشی از اپ نیست
   ========================================================================== */
body {
    margin: 0;
    background: radial-gradient(120% 90% at 50% 0%, #2A221B 0%, #14110F 70%);
    min-block-size: 100vh;
    color: #EDE6DA;
    font-family: Vazirmatn, Tahoma, system-ui, sans-serif;
    -webkit-font-smoothing: antialiased;
}

.pv-wrap { max-inline-size: 1120px; margin: 0 auto; padding: 28px 24px 64px; }

.pv-note {
    font-size: .82rem; line-height: 2; color: rgba(237,230,218,.7);
    margin-block-end: 30px;
}
.pv-note h1 { margin: 0 0 12px; font-size: 1.1rem; color: #F7F2EA; font-weight: 800; }
.pv-note h2 { margin: 22px 0 7px; font-size: .9rem; color: #C6A46A; font-weight: 700; }
.pv-note strong { color: #C6A46A; }
.pv-note code {
    font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
    background: rgba(237,230,218,.09); padding: 1px 6px; border-radius: 5px; font-size: .9em;
}
.pv-note ul { margin: 6px 0 0; padding-inline-start: 20px; }
.pv-note li { margin-block-end: 4px; }

/* دو ستون: روشن و تیره */
.pv-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
    gap: 22px;
    align-items: start;
}

.pv-panel {
    border-radius: 26px;
    padding: 26px 22px 30px;
    border: 1px solid rgba(198,164,106,.18);
}
.pv-panel--light { background: #F7F2EA; }
.pv-panel--dark  { background: #1D1916; }
.pv-panel--tokens { background: rgba(237,230,218,.04); }

.pv-panel__title {
    font-size: .78rem; font-weight: 800; letter-spacing: .02em;
    margin: 0 0 20px; display: flex; align-items: center; gap: 8px;
}
.pv-panel--light .pv-panel__title { color: #6F655B; }
.pv-panel--dark  .pv-panel__title { color: #A89F93; }
.pv-panel--tokens .pv-panel__title { color: #A89F93; }

.pv-stack { display: flex; flex-direction: column; gap: 18px; }
.pv-row { display: flex; flex-direction: column; gap: 10px; }
.pv-label {
    font-size: .72rem; font-weight: 700; opacity: .6;
}
.pv-panel--light .pv-label { color: #2A2420; }
.pv-panel--dark .pv-label, .pv-panel--tokens .pv-label { color: #EDE6DA; }

/* جدول رنگ‌ها */
.pv-swatches { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 12px; }
.pv-swatch {
    border-radius: 14px; overflow: hidden; border: 1px solid rgba(237,230,218,.12);
}
.pv-swatch__chip { block-size: 54px; }
.pv-swatch__meta {
    padding: 8px 11px; font-size: .68rem; line-height: 1.7;
    background: rgba(237,230,218,.05);
}
.pv-swatch__meta b { display: block; font-family: ui-monospace, monospace; font-size: .72rem; }
.pv-swatch__meta span { color: rgba(237,230,218,.6); }
</style>
</head>
<body>
<div class="pv-wrap">

<div class="pv-note">
    <h1>مرحله ۲ — توکن‌های رنگ و کامپوننت‌های پایه</h1>
    <p>
        این فایل از <strong>CSS واقعی پروژه</strong> ساخته شده (<code>login.css</code>) و
        آیکون‌هایش از منبع مشترک <code>tools/login_markup.py</code> می‌آیند — همان مسیرهایی
        که در <code>AuthIcons.razor</code> هستند. پس هر چه اینجا می‌بینید، همان چیزی است
        که در اپ رندر می‌شود.
    </p>

    <h2>چه چیزی را بررسی کنید</h2>
    <ul>
        <li><strong>کپسولی بودن:</strong> هر دو انتهای فیلد و دکمه کاملاً گرد باشند.</li>
        <li><strong>حلقه فوکوس:</strong> داخل هر فیلد کلیک کنید — حلقه طلایی باید دور
        <em>کل کپسول</em> بیفتد، نه فقط دور متن.</li>
        <li><strong>دکمه چشم:</strong> روی آیکون چشم در فیلد رمز کلیک کنید؛ باید بین
        نمایش و پنهان کردن سوئیچ شود و آیکون هم عوض شود.</li>
        <li><strong>دکمه ثانویه:</strong> متنش باید طلاییِ <em>عمیق</em> باشد نه طلایی روشن
        برند — چون طلایی روشن روی سطح عاجی حد کنتراست ۴٫۵ را رد نمی‌کند.</li>
        <li><strong>حالت تیره:</strong> دکمه اصلی در حالت تیره معکوس شده
        (پس‌زمینه طلایی، متن تیره). اگر این را نمی‌خواهید بگویید.</li>
        <li><strong>پر شدن خودکار:</strong> اگر مرورگر فرم را پر کند، فیلد نباید زرد شود.</li>
    </ul>

    <h2>نکته‌ای که هنوز ساخته نشده</h2>
    <p>
        چیدمان کارت، موج SVG چندلایه و خط طلایی دوگانه در مرحله‌های ۳ تا ۵ می‌آیند.
        اینجا فقط مصالح پایه است: رنگ‌ها، فیلد و دکمه.
    </p>
</div>

<div class="pv-panel pv-panel--tokens">
    <p class="pv-panel__title">پالت — همه از توکن‌های <code>.lg</code> می‌آیند</p>
    <div class="pv-swatches">__SWATCHES__</div>
</div>

<div class="pv-grid">

    <div class="pv-panel pv-panel--light">
        <p class="pv-panel__title">حالت روشن — سطح عاجی</p>
        <div class="lg">
            <div class="pv-stack">
                <div class="pv-row">
                    <span class="pv-label">فیلد خالی با آیکون</span>
                    __F1__
                </div>
                <div class="pv-row">
                    <span class="pv-label">فیلد پر شده</span>
                    __F2__
                </div>
                <div class="pv-row">
                    <span class="pv-label">فیلد با راهنما</span>
                    __F3__
                </div>
                <div class="pv-row">
                    <span class="pv-label">فیلد نامعتبر با پیام خطا</span>
                    __F4__
                </div>
                <div class="pv-row">
                    <span class="pv-label">فیلد غیرفعال</span>
                    __F5__
                </div>
                <div class="pv-row">
                    <span class="pv-label">رمز عبور با دکمه نمایش (روی چشم کلیک کنید)</span>
                    __F6__
                </div>
                <div class="pv-row">
                    <span class="pv-label">دکمه اصلی</span>
                    __B1__
                </div>
                <div class="pv-row">
                    <span class="pv-label">دکمه اصلی در حالت بارگذاری</span>
                    __B2__
                </div>
                <div class="pv-row">
                    <span class="pv-label">دکمه ثانویه و غیرفعال</span>
                    __B3__
                </div>
                <div class="pv-row">
                    <span class="pv-label">جداکننده، پیوند و پیام‌ها</span>
                    __M1__
                </div>
            </div>
        </div>
    </div>

    <div class="pv-panel pv-panel--dark">
        <p class="pv-panel__title">حالت تیره — سطح #1D1916</p>
        <div class="lg lg--dark">
            <div class="pv-stack">
                <div class="pv-row">
                    <span class="pv-label">فیلد خالی با آیکون</span>
                    __D1__
                </div>
                <div class="pv-row">
                    <span class="pv-label">فیلد نامعتبر</span>
                    __D2__
                </div>
                <div class="pv-row">
                    <span class="pv-label">رمز عبور</span>
                    __D3__
                </div>
                <div class="pv-row">
                    <span class="pv-label">دکمه اصلی (معکوس‌شده)</span>
                    __D4__
                </div>
                <div class="pv-row">
                    <span class="pv-label">دکمه ثانویه</span>
                    __D5__
                </div>
                <div class="pv-row">
                    <span class="pv-label">جداکننده و پیام</span>
                    __D6__
                </div>
            </div>
        </div>
    </div>

</div>
</div>

<script>
/* فقط برای پیش‌نمایش: سوئیچ نمایش رمز. در اپ، PasswordField.razor همین کار را
   با حالت داخلی خودش انجام می‌دهد. */
document.querySelectorAll('[data-eye]').forEach(function (btn) {
    btn.addEventListener('click', function () {
        var input = document.getElementById(btn.getAttribute('aria-controls'));
        if (!input) return;

        var nowShown = input.type === 'password';
        input.type = nowShown ? 'text' : 'password';

        btn.setAttribute('aria-pressed', nowShown ? 'true' : 'false');
        btn.setAttribute('aria-label', nowShown ? 'پنهان کردن رمز عبور' : 'نمایش رمز عبور');

        var show = btn.querySelector('[data-icon-show]');
        var hide = btn.querySelector('[data-icon-hide]');
        if (show) show.hidden = nowShown;
        if (hide) hide.hidden = !nowShown;
    });
});
</script>
</body>
</html>
"""


def build():
    with io.open(CSS_SRC, encoding="utf-8") as f:
        css = f.read()

    swatches = [
        ("--lg-page-1", "#2A221B", "مرکز گرادیان صفحه"),
        ("--lg-page-2", "#14110F", "لبه گرادیان صفحه"),
        ("--lg-surface", "#F7F2EA", "سطح فرم (عاجی)"),
        ("--lg-photo-bg", "#D8CCBE", "زمینه ناحیه تصویر"),
        ("--lg-wave-1", "#B49560", "لایه موج — طلایی تیره"),
        ("--lg-wave-2", "#CDB88F", "لایه موج — شامپاینی"),
        ("--lg-wave-3", "#DBCBAF", "لایه موج — حد وسط"),
        ("--lg-wave-4", "#E9DFD0", "لایه موج — کرم"),
        ("--lg-wave-5", "#F7F2EA", "لایه موج — عاجی"),
        ("--lg-gold", "#C6A46A", "طلایی برند"),
        ("--lg-gold-dark", "#A8854B", "طلایی تیره برند"),
        ("--lg-gold-deep", "#7A5A22", "طلایی عمیق (برای متن روی عاجی)"),
        ("--lg-text", "#2A2420", "متن اصلی"),
        ("--lg-text-dim", "#6F655B", "متن کم‌رنگ"),
        ("--lg-field-bg", "#FFFDFA", "زمینه فیلد"),
        ("--lg-danger", "#B3261E", "قرمز خطا"),
        ("--lg-btn-bg", "#14110F", "دکمه اصلی — زمینه"),
    ]
    swatch_html = "".join(
        f'<div class="pv-swatch"><div class="pv-swatch__chip" style="background:{hexv}"></div>'
        f'<div class="pv-swatch__meta"><b>{hexv}</b><span>{desc}</span>'
        f'<span style="opacity:.55">{token}</span></div></div>'
        for token, hexv, desc in swatches
    )

    page = (TEMPLATE
            .replace("__CSS__", css)
            .replace("__SWATCHES__", swatch_html)

            # حالت روشن
            .replace("__F1__", capsule("pv-mobile", "شماره موبایل", kind="tel",
                                       placeholder="09xxxxxxxxx", icon_name="phone",
                                       dir_attr="ltr", required=True))
            .replace("__F2__", capsule("pv-mobile-2", "شماره موبایل", kind="tel",
                                       value="09120000003", icon_name="phone", dir_attr="ltr"))
            .replace("__F3__", capsule("pv-mobile-3", "شماره موبایل", kind="tel",
                                       value="09120000003", icon_name="phone", dir_attr="ltr",
                                       hint="کد تأیید به همین شماره پیامک می‌شود."))
            .replace("__F4__", capsule("pv-mobile-4", "شماره موبایل", kind="tel",
                                       value="0912000", icon_name="phone", dir_attr="ltr",
                                       error="شماره موبایل باید ۱۱ رقم و با ۰۹ شروع شود."))
            .replace("__F5__", capsule("pv-mobile-5", "شماره موبایل", kind="tel",
                                       value="09120000003", icon_name="phone", dir_attr="ltr",
                                       disabled=True))
            .replace("__F6__", capsule("pv-pass", "رمز عبور", kind="password",
                                       value="123456", icon_name="lock", dir_attr="ltr",
                                       toggle=True))
            .replace("__B1__", button("ورود", block=True))
            .replace("__B2__", button("ورود", block=True, loading=True,
                                      loading_text="در حال ورود…"))
            .replace("__B3__", '<div class="pv-stack">'
                     + button("ورود با کد یکبار مصرف", variant="ghost", block=True,
                              icon_name="phone")
                     + button("ورود", block=True, disabled=True) + "</div>")
            .replace("__M1__", '<div class="pv-stack">' + divider()
                     + '<div style="text-align:center">' + link("رمز عبور را فراموش کرده‌اید؟") + "</div>"
                     + alert("شماره موبایل یا رمز عبور نادرست است.")
                     + alert("کد تأیید ارسال شد.", "ok") + "</div>")

            # حالت تیره
            .replace("__D1__", capsule("pv-d-mobile", "شماره موبایل", kind="tel",
                                       placeholder="09xxxxxxxxx", icon_name="phone",
                                       dir_attr="ltr", required=True))
            .replace("__D2__", capsule("pv-d-mobile-2", "شماره موبایل", kind="tel",
                                       value="0912000", icon_name="phone", dir_attr="ltr",
                                       error="شماره موبایل باید ۱۱ رقم و با ۰۹ شروع شود."))
            .replace("__D3__", capsule("pv-d-pass", "رمز عبور", kind="password",
                                       value="123456", icon_name="lock", dir_attr="ltr",
                                       toggle=True))
            .replace("__D4__", button("ورود", block=True))
            .replace("__D5__", button("ورود با کد یکبار مصرف", variant="ghost", block=True,
                                      icon_name="phone"))
            .replace("__D6__", '<div class="pv-stack">' + divider()
                     + alert("شماره موبایل یا رمز عبور نادرست است.") + "</div>"))

    with io.open(OUT, "w", encoding="utf-8") as f:
        f.write(page)
    print("ساخته شد: %s  (%d KB)" % (OUT, os.path.getsize(OUT) // 1024))


if __name__ == "__main__":
    build()
