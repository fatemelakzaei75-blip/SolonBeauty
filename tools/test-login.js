/* ==========================================================================
   تست کامپوننت‌های پایه صفحه ورود (مرحله ۲)
   --------------------------------------------------------------------------
   چرا این فایل وجود دارد: در محیط توسعه ما مرورگر نیست، پس ظاهر و کنتراست
   را نمی‌توان چشمی سنجید. این تست همان چیزها را عددی و ماشینی بررسی می‌کند:

   الف — توکن‌های رنگ: مقدارهای دقیق، نبود کدرنگ پراکنده، نبود !important
   ب — کنتراست متن (WCAG AA) برای هر جفت رنگ واقعی در هر دو حالت
   ج — ویژگی‌های منطقی RTL، اندازه اهداف لمس، کپسولی بودن، فوکوس
   د — کامپوننت‌های رِیزور: برچسب، aria، حالت بارگذاری
   ه — نگهبان رانش: آیکون‌ها، نام کلاس‌ها و متن CSS بین سه فایل هم‌گام باشند

   اجرا:  node tools/test-login.js
   ========================================================================== */
'use strict';

const fs = require('fs');
const path = require('path');

const ROOT = path.join(__dirname, '..');
const CSS_PATH = path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'css', 'login.css');
const PREVIEW = path.join(ROOT, 'login-preview.html');
const MARKUP_PY = path.join(ROOT, 'tools', 'login_markup.py');
const AUTH = path.join(ROOT, 'BlazorAppSolon', 'Components', 'Auth');

let failed = 0;

function ok(label, cond) {
    if (!cond) failed++;
    console.log(`   ${cond ? '✅' : '❌'} ${label}`);
}

function check(label, actual, expected, tol) {
    const good = Math.abs(actual - expected) <= (tol === undefined ? 1e-6 : tol);
    if (!good) failed++;
    console.log(`   ${good ? '✅' : '❌'} ${label}: ${actual.toFixed(2)}` +
        (good ? '' : `  (انتظار ${expected})`));
}

const css = fs.readFileSync(CSS_PATH, 'utf8');
const preview = fs.readFileSync(PREVIEW, 'utf8');
const markupPy = fs.readFileSync(MARKUP_PY, 'utf8');

const LAYOUT_PREVIEW = path.join(ROOT, 'login-layout-preview.html');
const WAVE_SRC = path.join(ROOT, 'tools', 'wave_geometry.py');

const readRazor = (name) => fs.readFileSync(path.join(AUTH, name), 'utf8');
const capsuleRazor = readRazor('CapsuleField.razor');
const passwordRazor = readRazor('PasswordField.razor');
const buttonRazor = readRazor('AuthButton.razor');
const iconsRazor = readRazor('AuthIcons.razor');
const cardRazor = readRazor('LoginCard.razor');
const waveRazor = readRazor('WaveDivider.razor');
const sheetRazor = readRazor('ForgotPasswordSheet.razor');
const alertRazor = readRazor('AuthAlert.razor');
const loginPage = fs.readFileSync(path.join(ROOT, 'BlazorAppSolon', 'Pages', 'Login.razor'), 'utf8');
const authLayout = fs.readFileSync(path.join(ROOT, 'BlazorAppSolon', 'Layout', 'AuthLayout.razor'), 'utf8');

const preview3 = fs.existsSync(LAYOUT_PREVIEW) ? fs.readFileSync(LAYOUT_PREVIEW, 'utf8') : '';
const waveSrc = fs.readFileSync(WAVE_SRC, 'utf8');

/** حذف توضیح‌ها: توضیح، کد نیست و نباید آزمون را گمراه کند. */
const stripComments = (t) => t.replace(/\/\*[\s\S]*?\*\//g, '');

/** بدنه فایل: هر چیز بیرون از بلوک‌های توکن. */
function bodyOf(text) {
    const clean = stripComments(text);
    // بلوک‌های توکن با شروعشان در ابتدای خط شناسایی می‌شوند
    const removed = clean.replace(/^\.lg \{[\s\S]*?\n\}/gm, '')
                          .replace(/^\[data-theme="dark"\] \.lg,\n\.lg--dark \{[\s\S]*?\n\}/gm, '');
    return removed;
}

/* ==========================================================================
   الف) توکن‌های رنگ
   ========================================================================== */

console.log('\n▸ الف) ساختار فایل CSS');
{
    ok('فایل login.css وجود دارد', css.length > 1000);
    const open = (css.match(/\{/g) || []).length;
    const close = (css.match(/\}/g) || []).length;
    check('آکولادهای باز و بسته برابرند', open, close, 0);

    const clean = stripComments(css);
    const bangs = clean.match(/!important/g) || [];
    ok(`هیچ !important وجود ندارد (پیدا شد: ${bangs.length || 'هیچ'})`, bangs.length === 0);

    ok('توکن‌ها روی .lg دامنه دارند، نه :root', /^\.lg \{/m.test(clean) && !/:root\s*\{/.test(clean));
}

console.log('\n▸ الف) رنگ‌های بریف — مقدار دقیق');
{
    const required = [
        ['--lg-page-1', '#2A221B', 'مرکز گرادیان صفحه'],
        ['--lg-page-2', '#14110F', 'لبه گرادیان صفحه'],
        ['--lg-surface', '#F7F2EA', 'سطح فرم عاجی'],
        ['--lg-wave-1', '#B49560', 'لایه موج طلایی تیره'],
        ['--lg-wave-2', '#CDB88F', 'لایه موج شامپاینی'],
        ['--lg-wave-4', '#E9DFD0', 'لایه موج کرم'],
        ['--lg-photo-bg', '#D8CCBE', 'زمینه ناحیه تصویر'],
        ['--lg-gold', '#C6A46A', 'طلایی برند'],
        ['--lg-gold-dark', '#A8854B', 'طلایی تیره برند'],
        ['--lg-text', '#2A2420', 'متن اصلی'],
        ['--lg-text-dim', '#6F655B', 'متن کم‌رنگ'],
        ['--lg-btn-bg', '#14110F', 'زمینه دکمه اصلی'],
        ['--lg-btn-text', '#C6A46A', 'متن دکمه اصلی'],
    ];
    for (const [token, hex, desc] of required) {
        const re = new RegExp(token.replace(/-/g, '\\-') + ':\\s*' + hex, 'i');
        ok(`${token} = ${hex}  (${desc})`, re.test(css));
    }

    // لایه میانه: بریف «حد وسط» گفته ولی عدد نداده؛ باید بین شامپاینی و کرم باشد
    const mid = /--lg-wave-3:\s*(#[0-9A-Fa-f]{6})/.exec(css);
    ok('لایه موج «حد وسط» تعریف شده', !!mid);
    if (mid) {
        const v = mid[1];
        const rgb = (h) => [1, 3, 5].map((i) => parseInt(h.substr(i, 2), 16));
        const [r, g, b] = rgb(v);
        const [r2, g2, b2] = rgb('#CDB88F');   // شامپاینی
        const [r4, g4, b4] = rgb('#E9DFD0');   // کرم
        const between = r > r2 && r < r4 && g > g2 && g < g4 && b > b2 && b < b4;
        ok(`لایه میانه ${v} بین شامپاینی و کرم است (نه بیرون از طیف)`, between);
    }
}

console.log('\n▸ الف) حالت تیره');
{
    ok('بلوک حالت تیره از قرارداد پروژه استفاده می‌کند ([data-theme="dark"])',
        /\[data-theme="dark"\] \.lg/.test(css));
    ok('کلاس اجباری .lg--dark هم هست (برای تست و پیش‌نمایش)', /\.lg--dark\s*\{/.test(css));
    ok('سطح فرم در حالت تیره #1D1916 است', /--lg-surface:\s*#1D1916/i.test(css));
    ok('متن در حالت تیره #EDE6DA است', /--lg-text:\s*#EDE6DA/i.test(css));
    ok('دکمه اصلی در حالت تیره معکوس می‌شود (زمینه طلایی، متن تیره)',
        /--lg-btn-bg:\s*#C6A46A/.test(css) && /--lg-btn-text:\s*#14110F/.test(css));
    ok('حالت تیره به تم سیستم وصل نشده (قرارداد پروژه دستی است)',
        !/prefers-color-scheme/.test(css));
}

console.log('\n▸ الف) هیچ کدرنگ پراکنده‌ای نباشد');
{
    const body = bodyOf(css);

    const hexes = body.match(/#[0-9A-Fa-f]{6}\b/g) || [];
    ok(`هیچ کدرنگ شش‌رقمی در بدنه فایل نیست (پیدا شد: ${hexes.length || 'هیچ'})`,
        hexes.length === 0);
    if (hexes.length) console.log('      →', hexes.slice(0, 8).join(', '));

    const rgbas = body.match(/rgba?\(/g) || [];
    ok(`هیچ rgba خامی در بدنه فایل نیست (پیدا شد: ${rgbas.length || 'هیچ'})`,
        rgbas.length === 0);

    // هر توکنی که تعریف شده باید جایی هم استفاده شود — مگر آن‌هایی که عمداً
    // برای مرحله‌های بعد رزرو شده‌اند. این فهرست صریح است تا اگر روزی کسی
    // توکن تازه‌ای بی‌استفاده اضافه کند، آزمون بگیردش.
    // در پایان مرحله ۳ همه ۵۰ توکن مصرف شده‌اند — فهرست رزرو خالی است.
    // سازوکارش می‌ماند: اگر مرحله‌های بعد توکن تازه‌ای اضافه کردند، تا وقتی
    // مصرفش نکنند، باید صریح اینجا رزرو شود.
    const deferredTokens = new Set([]);

    const defined = [...css.matchAll(/^\s*(--lg-[a-z0-9-]+)\s*:/gm)].map((m) => m[1]);
    const unique = [...new Set(defined)];
    const unused = unique.filter((t) => {
        const uses = (css.match(new RegExp('var\\(\\s*' + t + '[,\\s)]', 'g')) || []).length;
        return uses === 0;
    });
    const unexpected = unused.filter((t) => !deferredTokens.has(t));
    ok(`هیچ توکن بی‌استفاده ناخواسته‌ای نیست (${unused.length} توکن رزرو‌شده برای مرحله‌های بعد)` +
        (unexpected.length ? ` — ناخواسته: ${unexpected.join(', ')}` : ''),
        unexpected.length === 0);

    // هر توکن رزرو‌شده باید واقعاً تعریف شده باشد؛ وگرنه فهرست کهنه شده است
    const staleReserved = [...deferredTokens].filter((t) => !unique.includes(t));
    ok('فهرست توکن‌های رزرو‌شده به‌روز است (هیچ نام کهنه‌ای ندارد)',
        staleReserved.length === 0);
}

/* ==========================================================================
   ب) کنتراست
   ========================================================================== */

console.log('\n▸ ب) کنتراست متن — WCAG AA (حد ۴٫۵:۱)');

const luminance = (hex) => {
    const v = [1, 3, 5].map((i) => parseInt(hex.substr(i, 2), 16) / 255)
        .map((c) => (c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4)));
    return 0.2126 * v[0] + 0.7152 * v[1] + 0.0722 * v[2];
};
const ratio = (a, b) => {
    const la = luminance(a), lb = luminance(b);
    return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05);
};

{
    const pairs = [
        // ---------- حالت روشن ----------
        ['#C6A46A', '#14110F', 'متن دکمه اصلی روی زمینه تیره', 'light'],
        ['#7A5A22', '#F7F2EA', 'متن دکمه ثانویه روی سطح عاجی', 'light'],
        ['#7A5A22', '#F7F2EA', 'پیوند «فراموشی رمز» روی سطح عاجی', 'light'],
        ['#2A2420', '#F7F2EA', 'متن اصلی روی سطح عاجی', 'light'],
        ['#2A2420', '#FFFDFA', 'متن داخل فیلد', 'light'],
        ['#6F655B', '#F7F2EA', 'برچسب فیلد روی سطح عاجی', 'light'],
        ['#6F655B', '#FFFDFA', 'متن جانشین (placeholder) داخل فیلد', 'light'],
        ['#B3261E', '#F7F2EA', 'پیام خطا روی سطح عاجی', 'light'],
        ['#7A5A22', '#EEE6D9', 'متن پیام موفق (روی زمینه طلایی نیمه‌شفاف)', 'light'],

        // ---------- حالت تیره ----------
        ['#14110F', '#C6A46A', 'متن دکمه اصلی معکوس‌شده در حالت تیره', 'dark'],
        ['#E0C48A', '#1D1916', 'متن دکمه ثانویه در حالت تیره', 'dark'],
        ['#EDE6DA', '#1D1916', 'متن اصلی روی سطح تیره', 'dark'],
        ['#EDE6DA', '#262220', 'متن داخل فیلد در حالت تیره', 'dark'],
        ['#A89F93', '#1D1916', 'برچسب فیلد در حالت تیره', 'dark'],
        ['#F0A9A2', '#1D1916', 'پیام خطا در حالت تیره', 'dark'],
    ];

    let worst = { r: 99, label: '' };
    for (const [fg, bg, label, mode] of pairs) {
        const r = ratio(fg, bg);
        if (r < worst.r) worst = { r, label };
        ok(`[${mode}] ${label}: ${r.toFixed(2)}:۱`, r >= 4.5);
    }
    console.log(`\n   پایین‌ترین کنتراست در کل پالت: ${worst.r.toFixed(2)}:۱  (${worst.label})`);
}

/* ==========================================================================
   ج) RTL، اهداف لمس، فوکوس
   ========================================================================== */

console.log('\n▸ ج) ویژگی‌های منطقی RTL');
{
    const body = bodyOf(css);

    const physical = [];
    for (const prop of ['padding-left', 'padding-right', 'margin-left', 'margin-right',
                        'border-left', 'border-right', 'border-left-width', 'border-right-width']) {
        const re = new RegExp('(^|[;{\\s])' + prop + '\\s*:', 'm');
        if (re.test(body)) physical.push(prop);
    }
    ok(`هیچ ویژگی فیزیکی چپ/راست وجود ندارد (پیدا شد: ${physical.join(', ') || 'هیچ'})`,
        physical.length === 0);

    // text-align: left/right هم فیزیکی است؛ باید start/end باشد
    const alignSide = body.match(/text-align:\s*(left|right)/g) || [];
    ok(`text-align فقط منطقی است، نه چپ/راست (پیدا شد: ${alignSide.join(', ') || 'هیچ'})`,
        alignSide.length === 0);

    for (const prop of ['padding-inline', 'margin-inline', 'padding-inline-start',
                        'margin-inline-end', 'border-block-start-color']) {
        ok(`ویژگی منطقی ${prop} استفاده شده`, body.includes(prop));
    }

    // آینه‌شوندگی: در RTL نباید جایی به چپ/راست مطلق ارجاع شود
    ok('هیچ ارجاع مطلق به left/right در چیدمان نیست',
        !/(^|[;{\s])(left|right)\s*:/m.test(body));
}

console.log('\n▸ ج) اهداف لمس ≥ ۴۴ پیکسل');
{
    ok('توکن --lg-touch برابر ۴۴px است', /--lg-touch:\s*44px/.test(css));
    ok('ارتفاع فیلد کپسولی ≥ ۴۴px است', /--lg-field-h:\s*(4[4-9]|[5-9]\d)px/.test(css));
    ok('ارتفاع دکمه ≥ ۴۴px است', /--lg-btn-h:\s*(4[4-9]|[5-9]\d)px/.test(css));
    ok('دکمه چشم از توکن --lg-touch استفاده می‌کند',
        /\.lg-field__eye\s*\{[\s\S]*?inline-size:\s*var\(--lg-touch\)[\s\S]*?block-size:\s*var\(--lg-touch\)/.test(css));
    ok('پیوند متنی حداقل ارتفاع لمس را دارد',
        /\.lg-link\s*\{[\s\S]*?min-block-size:\s*var\(--lg-touch\)/.test(css));
    ok('دکمه‌ها min-block-size دارند',
        /\.lg-btn\s*\{[\s\S]*?min-block-size:\s*var\(--lg-btn-h\)/.test(css));

    // بازبینی عددی: هر مقدار ۴۴ یا بیشتر؟ اگر کسی بعداً ۳۰px بگذارد، این می‌گیرد
    const touchVars = [...css.matchAll(/--lg-(touch|field-h|btn-h):\s*(\d+)px/g)];
    const tooSmall = touchVars.filter((m) => Number(m[2]) < 44);
    ok(`هیچ توکن لمسی کوچک‌تر از ۴۴px نیست (پیدا شد: ${tooSmall.length || 'هیچ'})`,
        tooSmall.length === 0);
}

console.log('\n▸ ج) کپسولی بودن و فوکوس');
{
    ok('شعاع کپسول ۹۹۹px است (خمیردندان)', /--lg-radius:\s*999px/.test(css));
    ok('فیلد از توکن شعاع استفاده می‌کند', /border-radius:\s*var\(--lg-radius\)/.test(css));
    ok('دکمه از توکن شعاع استفاده می‌کند', /\.lg-btn\s*\{[\s\S]*?border-radius:\s*var\(--lg-radius\)/.test(css));

    ok('حلقه فوکوس دور کل کپسول می‌آید (focus-within روی کادر)',
        /\.lg-field__box:focus-within\s*\{/.test(css));
    ok('فوکوس با رنگ طلایی مشخص می‌شود',
        /\.lg-field__box:focus-within\s*\{[\s\S]*?border-color:\s*var\(--lg-gold\)/.test(css));
    ok('دکمه‌ها :focus-visible دارند', /\.lg-btn:focus-visible\s*\{/.test(css));
    ok('دکمه چشم :focus-visible دارد', /\.lg-field__eye:focus-visible\s*\{/.test(css));
    ok('پیوند :focus-visible دارد', /\.lg-link:focus-visible\s*\{/.test(css));
    ok('حلقه فوکوس توکن دارد (ضخامت)', /--lg-ring-w:/.test(css));
}

console.log('\n▸ ج) انیمیشن، دسترس‌پذیری حرکتی و جزئیات');
{
    const rmStart = css.indexOf('@media (prefers-reduced-motion');
    ok('بلوک prefers-reduced-motion وجود دارد', rmStart > 0);
    const rm = css.substring(rmStart);
    ok('در reduced-motion گذارها خاموش می‌شوند', /transition:\s*none/.test(rm));
    ok('در reduced-motion چرخش اسپینر خاموش می‌شود', /\.lg-btn__spinner\s*\{[\s\S]*?animation:\s*none/.test(rm));
    ok('در reduced-motion اثر فشردن دکمه حذف می‌شود',
        /\.lg-btn:active:not\(:disabled\)\s*\{\s*transform:\s*none/.test(rm));

    ok('قاعده پر شدن خودکار مرورگر وجود دارد (فیلد زرد نمی‌شود)',
        /-webkit-autofill/.test(css) && /-webkit-box-shadow:\s*0 0 0 60px/.test(css));

    ok('رنگ متن جانشین (placeholder) از توکن است', /::placeholder[\s\S]{0,120}?color:\s*var\(--lg-text-dim\)/.test(css));
    ok('متن فقط برای صفحه‌خوان (.lg-sr) تعریف شده', /\.lg-sr\s*\{/.test(css));
    ok('safe-area رعایت شده',
        /padding-block:\s*max\(16px,\s*env\(safe-area-inset-top\)\)/.test(css) &&
        /padding-inline:\s*max\(16px,\s*env\(safe-area-inset-right\)\)/.test(css));
    ok('حاشیه پیش‌فرض فقط داخل کامپوننت صفر می‌شود، نه سراسری',
        /\.lg \*,[\s\S]{0,80}?box-sizing:\s*border-box/.test(css));

    // هیچ قاعده‌ای نباید بیرون از .lg دامنه بگیرد و به سایت درز کند
    const selectors = stripComments(css).match(/^[^@\/\s][^{]*\{/gm) || [];
    const leaks = selectors
        .map((s) => s.replace('{', '').trim())
        .filter((s) => !s.includes('.lg') && !s.includes('@'));
    ok(`هیچ انتخابگری بی‌دامنه به بیرون درز نمی‌کند (پیدا شد: ${leaks.join(' | ') || 'هیچ'})`,
        leaks.length === 0);
}

/* ==========================================================================
   د) کامپوننت‌های رِیزور
   ========================================================================== */

console.log('\n▸ د) CapsuleField.razor');
{
    ok('برچسب با for به ورودی وصل شده', /<label[^>]*for="@Id"/.test(capsuleRazor));
    ok('ورودی شناسه دارد (id="@Id")', /id="@Id"/.test(capsuleRazor));
    ok('aria-describedby فقط به عنصر موجود اشاره می‌کند',
        /DescribedBy/.test(capsuleRazor) && /Id\}-err/.test(capsuleRazor) && /Id\}-hint/.test(capsuleRazor));
    ok('aria-invalid فقط در حالت خطا ست می‌شود', /aria-invalid="@\(Error is not null/.test(capsuleRazor));
    ok('aria-required ست می‌شود', /aria-required="@\(Required/.test(capsuleRazor));
    ok('ویژگی required مرورگر عمداً داده نشده (تعارض با اعتبارسنجی فارسی)',
        !/\srequired[="]/.test(capsuleRazor.replace(/aria-required/g, '').replace(/Required/g, '')));
    ok('پیام خطا role="alert" دارد', /role="alert"/.test(capsuleRazor));
    ok('خطا جای راهنما را می‌گیرد (زیر فیلد شلوغ نمی‌شود)', /else if \(!string\.IsNullOrEmpty\(Hint\)\)/.test(capsuleRazor));
    ok('پیوند دوطرفه مقدار تعریف شده', /ValueChanged/.test(capsuleRazor) && /EventCallback<string\?>/.test(capsuleRazor));
    ok('Id و Label اجباری علامت خورده‌اند', /EditorRequired[^\n]*Id/.test(capsuleRazor) && /EditorRequired[^\n]*Label/.test(capsuleRazor));
    ok('نوع ورودی قابل تنظیم است', /public string Type/.test(capsuleRazor));
    ok('جهت متن قابل تنظیم است (برای موبایل و رمز ltr)', /public string\? Dir/.test(capsuleRazor));
    ok('صفحه‌کلید موبایل قابل تنظیم است (inputmode)', /public string\? InputMode/.test(capsuleRazor));
    ok('enterkeyhint قابل تنظیم است', /public string\? EnterKeyHint/.test(capsuleRazor));
}

console.log('\n▸ د) PasswordField.razor');
{
    ok('از CapsuleField استفاده می‌کند، چیدمان را تکرار نمی‌کند',
        /<CapsuleField/.test(passwordRazor));
    ok('نوع ورودی با حالت دیده‌شدن عوض می‌شود',
        /Type="@\(_visible \? "text" : "password"\)"/.test(passwordRazor));
    ok('دکمه نوع button دارد (نقش submit نمی‌گیرد)', /type="button"/.test(passwordRazor));
    ok('aria-label وضعیت فعلی را می‌گوید',
        /aria-label="@\(_visible \? "پنهان کردن رمز عبور" : "نمایش رمز عبور"\)"/.test(passwordRazor));
    ok('aria-pressed حالت دکمه فشاری را می‌دهد', /aria-pressed="@\(_visible/.test(passwordRazor));
    ok('aria-controls به خود فیلد اشاره می‌کند', /aria-controls="@Id"/.test(passwordRazor));
    ok('autocomplete پیش‌فرض current-password است', /"current-password"/.test(passwordRazor));
    ok('آیکون قفل روی فیلد هست', /IconName="lock"/.test(passwordRazor));
    ok('دکمه در حالت غیرفعال هم غیرفعال می‌شود', /disabled="@Disabled"/.test(passwordRazor));
}

console.log('\n▸ د) AuthButton.razor');
{
    ok('در حالت بارگذاری غیرفعال می‌شود (دوباره زده نمی‌شود)',
        /disabled="@\(Disabled \|\| Loading\)"/.test(buttonRazor));
    ok('aria-busy در حالت بارگذاری ست می‌شود', /aria-busy="@\(Loading/.test(buttonRazor));
    ok('دو گونه دارد: primary و ghost', /primary \| ghost/.test(buttonRazor));
    ok('اسپینر از دید صفحه‌خوان پنهان است', /class="lg-btn__spinner" aria-hidden="true"/.test(buttonRazor));
    ok('متن در حالت بارگذاری عوض می‌شود', /LoadingText/.test(buttonRazor));
    ok('نوع دکمه قابل تنظیم است (submit در فرم)', /public string Type \{ get; set; \} = "button"/.test(buttonRazor));
}

console.log('\n▸ د) AuthIcons.razor');
{
    for (const name of ['phone', 'lock', 'eye', 'eye-off', 'alert', 'check', 'sparkle']) {
        ok(`آیکون ${name} تعریف شده`, iconsRazor.includes(`case "${name}"`));
    }
    ok('آیکون‌ها از دید صفحه‌خوان پنهان‌اند', /aria-hidden="true" focusable="false"/.test(iconsRazor));
    ok('رنگ آیکون از currentColor می‌آید (با متن هم‌رنگ می‌شود)',
        /stroke="currentColor"/.test(iconsRazor));
    ok('کلاس از بیرون قابل تنظیم است', /public string\? Class/.test(iconsRazor));
}

/* ==========================================================================
   ه) نگهبان رانش بین فایل‌ها
   ========================================================================== */

console.log('\n▸ ه) نگهبان رانش — آیکون‌ها');
{
    // هر مسیر رسم که در AuthIcons.razor هست باید عیناً در login_markup.py هم باشد
    const razorShapes = new Set(
        [...iconsRazor.matchAll(/\sd="([^"]+)"/g)].map((m) => m[1]));
    const razorRects = new Set(
        [...iconsRazor.matchAll(/<rect [^>]+\/>/g)].map((m) => m[0].trim()));
    const razorCircles = new Set(
        [...iconsRazor.matchAll(/<circle [^>]+\/>/g)].map((m) => m[0].trim()));

    const pyShapes = new Set([...markupPy.matchAll(/d="([^"]+)"/g)].map((m) => m[1]));
    const pyRects = new Set([...markupPy.matchAll(/<rect [^>]+\/>/g)].map((m) => m[0].trim()));
    const pyCircles = new Set([...markupPy.matchAll(/<circle [^>]+\/>/g)].map((m) => m[0].trim()));

    const missing = [...razorShapes].filter((d) => !pyShapes.has(d));
    ok(`همه ${razorShapes.size} مسیر رسم بین رِیزور و پایتون یکی است` +
        (missing.length ? ` (غایب: ${missing.length})` : ''), missing.length === 0);

    const missRect = [...razorRects].filter((r) => !pyRects.has(r));
    ok(`مستطیل‌های آیکون هم‌گام‌اند` + (missRect.length ? ` (${missRect.join(', ')})` : ''), missRect.length === 0);

    const missCircle = [...razorCircles].filter((c) => !pyCircles.has(c));
    ok(`دایره‌های آیکون هم‌گام‌اند` + (missCircle.length ? ` (${missCircle.join(', ')})` : ''), missCircle.length === 0);

    const extraPy = [...pyShapes].filter((d) => !razorShapes.has(d));
    ok(`مسیر اضافه‌ای در پایتون نیست` + (extraPy.length ? ` (${extraPy.length})` : ''), extraPy.length === 0);
}

console.log('\n▸ ه) نگهبان رانش — نام کلاس‌ها');
{
    // ⚠ درس گرفته‌شده: الگوی نام کلاس باید زیرخط را هم بپذیرد، وگرنه
    // کلاس‌های BEM (lg-field__label) به lg-field بریده می‌شوند و هر دو
    // بررسی (تعریف‌شده/بی‌استفاده) بی‌صدا بی‌اثر می‌شوند.
    const defined = new Set(
        [...css.matchAll(/\.(lg-[a-z0-9_-]+)/g)].map((m) => m[1]));

    // کلاس‌هایی که برای مرحله‌های بعد رزرو شده‌اند و هنوز مصرفی ندارند
    const deferred = new Set(['lg-sr']);
    const usedInRazor = [capsuleRazor, passwordRazor, buttonRazor, iconsRazor,
                         cardRazor, waveRazor, sheetRazor, alertRazor, loginPage].join('\n');
    const usedAnywhere = usedInRazor + '\n' + preview + '\n' + preview3;

    const unused = [...defined].filter(
        (c) => !usedAnywhere.includes(c) && !deferred.has(c));
    ok(`همه ${defined.size} کلاس تعریف‌شده جایی استفاده شده‌اند` +
        (unused.length ? ` (بی‌استفاده: ${unused.join(', ')})` : ''), unused.length === 0);

    // و برعکس: کلاسی که در رِیزور استفاده شده ولی در CSS نیست.
    //
    // ⚠ درس گرفته‌شده: اگر کل متن رِیزور را با الگوی lg-… بگردیم، شناسه‌ها هم
    // با کلاس‌ها قاطی می‌شوند (Id="lg-mobile" شناسه است نه کلاس) و رشته‌های
    // پویا مثل lg-wave--@Orientation هم مثبت کاذب می‌سازند. پس فقط مقدار
    // ویژگی class خوانده می‌شود و بخش پویا جدا می‌شود.
    // ⚠ درس گرفته‌شده: عبارت‌های رِیزوری می‌توانند کوتیشن داشته باشند
    // (مثل class="lg-alert--@(Kind == "ok" ? "ok" : "error")")؛ اگر با الگوی
    // ساده کلاس خوانده شود، از کوتیشن داخلی بریده می‌شود و تکه‌ای مثل
    // «lg-alert--@(Kind» به‌عنوان کلاس بی‌تعریف گزارش می‌شود. پس اول همه
    // عبارت‌های @(...) — با پشتیبانی از کوتیشن داخلشان — پاک می‌شوند.
    const exprFree = usedInRazor
        .replace(/@\({(?:"[^"]*"|[^()])*}\)/g, ' ')
        .replace(/@\((?:"[^"]*"|[^()])*\)/g, ' ')
        .replace(/\$?"[^"]*"/g, ' ')
        .replace(/@\w+/g, ' ')
        .replace(/[{}]/g, ' ');

    const usedClassTokens = new Set();
    for (const m of exprFree.matchAll(/class="([^"]*)"/g)) {
        for (const t of m[1].split(/\s+/)) if (t.startsWith('lg-')) usedClassTokens.add(t);
    }
    const undefinedClasses = [...usedClassTokens].filter((c) => {
        if (defined.has(c)) return false;
        // کلاس پویا: ته‌اش بریده شده (lg-btn-- از lg-btn--@Variant)
        if (c.endsWith('--')) return ![...defined].some((d) => d.startsWith(c));
        return true;
    });
    ok(`همه ${usedClassTokens.size} کلاس به‌کاررفته در رِیزور در CSS تعریف شده‌اند` +
        (undefinedClasses.length ? ` (بی‌تعریف: ${undefinedClasses.join(', ')})` : ''),
        undefinedClasses.length === 0);
}

console.log('\n▸ ه) نگهبان رانش — CSS و پیش‌نمایش');
{
    ok('CSS کامل در پیش‌نمایش جاسازی شده', preview.includes(css.trim().slice(0, 400)));
    ok('پیش‌نمایش هیچ برچسب جاگذاری‌نشده ندارد',
        !/__[A-Z0-9_]+__/.test(preview));

    // هیچ تگ input شکسته‌ای نباشد (کوتیشن فرد) — باگی که یک بار رخ داد
    const inputs = preview.match(/<input [^>]*>/g) || [];
    const brokenInputs = inputs.filter((t) => (t.match(/"/g) || []).length % 2 !== 0);
    ok(`هیچ تگ input شکسته‌ای در پیش‌نمایش نیست (${inputs.length} ورودی بررسی شد)`,
        brokenInputs.length === 0);
}

console.log('\n▸ ه) قرارداد کامنت رِیزور');
{
    // درس گرفته‌شده: کامنت رِیزور داخل لیست ویژگی‌های یک تگ حذف نمی‌شود و
    // به‌عنوان ویژگی واقعی صادر می‌شود. باید بالای تگ باشد.
    const files = { 'CapsuleField': capsuleRazor, 'PasswordField': passwordRazor,
                    'AuthButton': buttonRazor, 'AuthIcons': iconsRazor };
    for (const [name, src] of Object.entries(files)) {
        let insideTag = false, leak = false;
        for (let i = 0; i < src.length - 1; i++) {
            if (src[i] === '<') insideTag = true;
            else if (src[i] === '>' && insideTag) insideTag = false;
            else if (insideTag && src.startsWith('@*', i)) { leak = true; break; }
        }
        ok(`${name}: هیچ کامنت رِیزوری داخل لیست ویژگی‌ها نیست`, !leak);
    }

    for (const [name, src] of Object.entries(files)) {
        const comments = src.match(/@\*[\s\S]*?\*@/g) || [];
        const broken = comments.filter((c) => c.slice(2, -2).includes('*@'));
        ok(`${name}: هیچ کامنت تودرتویی وجود ندارد`, broken.length === 0);
    }
}

/* ==========================================================================
   و) مرحله ۳ — هندسه موج، کامپوننت، چیدمان
   --------------------------------------------------------------------------
   اینجا هندسه موج مستقل از پایتون و مستقل از رِیزور بازسازی می‌شود:
   عددهای نقاط راه از wave_geometry.py خوانده می‌شوند، اسپلاین در JS ساخته
   می‌شود و رشته مسیرها با آنچه در WaveDivider.razor نوشته شده مقایسه می‌شود.
   پس اگر کسی فایل رِیزور را دستی عوض کند — یا منحنی شکلش را از دست بدهد —
   همین آزمون می‌گیردش. (بررسی هم‌گامی با پایتون هم جداگانه در پایان می‌آید.)
   ========================================================================== */

const fmtNum = (v) => {
    let s = v.toFixed(2);
    if (s.includes('.')) s = s.replace(/0+$/, '').replace(/\.$/, '');
    return s === '-0' ? '0' : s;
};

const catmullSegs = (pts) => {
    const ext = [pts[0], ...pts, pts[pts.length - 1]];
    const segs = [];
    for (let i = 1; i < ext.length - 2; i++) {
        const [p0, p1, p2, p3] = [ext[i - 1], ext[i], ext[i + 1], ext[i + 2]];
        segs.push([
            p1,
            [p1[0] + (p2[0] - p0[0]) / 6, p1[1] + (p2[1] - p0[1]) / 6],
            [p2[0] - (p3[0] - p1[0]) / 6, p2[1] - (p3[1] - p1[1]) / 6],
            p2,
        ]);
    }
    return segs;
};

const sample = (pts, per = 40) => {
    const out = [];
    for (const [p0, c1, c2, p1] of catmullSegs(pts)) {
        for (let i = out.length ? 0 : 1; i <= per; i++) {
            const t = i / per, mt = 1 - t;
            out.push([
                mt ** 3 * p0[0] + 3 * mt ** 2 * t * c1[0] + 3 * mt * t ** 2 * c2[0] + t ** 3 * p1[0],
                mt ** 3 * p0[1] + 3 * mt ** 2 * t * c1[1] + 3 * mt * t ** 2 * c2[1] + t ** 3 * p1[1],
            ]);
        }
    }
    return out;
};

const edgeD = (pts) => {
    const segs = catmullSegs(pts);
    let d = `M ${fmtNum(segs[0][0][0])} ${fmtNum(segs[0][0][1])}`;
    for (const s of segs) {
        d += ` C ${fmtNum(s[1][0])} ${fmtNum(s[1][1])} ${fmtNum(s[2][0])} ` +
             `${fmtNum(s[2][1])} ${fmtNum(s[3][0])} ${fmtNum(s[3][1])}`;
    }
    return d;
};

const fillD = (pts, side, view) => {
    const segs = catmullSegs(pts);
    const first = segs[0][0], last = segs[segs.length - 1][3];
    return side === 'right'
        ? `${edgeD(pts)} L ${fmtNum(view[0])} ${fmtNum(last[1])} L ${fmtNum(view[0])} ${fmtNum(first[1])} Z`
        : `${edgeD(pts)} L ${fmtNum(last[0])} ${fmtNum(view[1])} L ${fmtNum(first[0])} ${fmtNum(view[1])} Z`;
};

/** خواندن نقاط راه از منبع هندسه */
const readWaypoints = (name) => {
    const m = new RegExp(name + '\\s*=\\s*\\[([\\s\\S]*?)\\]').exec(waveSrc);
    if (!m) return null;
    return [...m[1].matchAll(/\(\s*(-?[\d.]+)\s*,\s*(-?[\d.]+)\s*\)/g)]
        .map((p) => [Number(p[1]), Number(p[2])]);
};

const GAP = Number((/^GAP\s*=\s*(\d+)/m.exec(waveSrc) || [])[1]);
const GOLD_GAP_PX = Number((/^GOLD_GAP_PX\s*=\s*([\d.]+)/m.exec(waveSrc) || [])[1]);

const VIEWS = { vertical: [100, 600], horizontal: [600, 120] };
const AXIS = { vertical: 0, horizontal: 1 };          // محور جابه‌جایی لایه‌ها
const SIDE = { vertical: 'right', horizontal: 'bottom' };

const WAY = {
    vertical: readWaypoints('DESKTOP_WAYPOINTS'),
    horizontal: readWaypoints('MOBILE_WAYPOINTS'),
};

const LAYERS = { vertical: 5, horizontal: 4 };

const buildPaths = (orientation) => {
    const base = WAY[orientation];
    const axis = AXIS[orientation];
    const view = VIEWS[orientation];
    return Array.from({ length: LAYERS[orientation] }, (_, i) => {
        const pts = base.map((p) => axis === 0 ? [p[0] + GAP * i, p[1]] : [p[0], p[1] + GAP * i]);
        return fillD(pts, SIDE[orientation], view);
    });
};

console.log('\n▸ و) هندسه موج — بازسازی مستقل');
{
    ok('نقاط راه دسکتاپ از منبع هندسه خوانده شد',
        Array.isArray(WAY.vertical) && WAY.vertical.length >= 6);
    ok('نقاط راه موبایل از منبع هندسه خوانده شد',
        Array.isArray(WAY.horizontal) && WAY.horizontal.length >= 6);
    ok('فاصله لایه‌ها ۷ واحد است (بریف: «حدود ۷ واحد از ۱۰۰»)', GAP === 7);
    ok('دسکتاپ ۵ لایه و موبایل ۴ لایه', LAYERS.vertical === 5 && LAYERS.horizontal === 4);

    for (const o of ['vertical', 'horizontal']) {
        const view = VIEWS[o], axis = AXIS[o], longAxis = 1 - axis;
        // ۱) داخل viewBox بماند
        let outside = 0;
        for (let i = 0; i < LAYERS[o]; i++) {
            const pts = sample(WAY[o].map((p) => axis === 0
                ? [p[0] + GAP * i, p[1]] : [p[0], p[1] + GAP * i]));
            for (const [x, y] of pts) {
                if (x < -0.01 || x > view[0] + 0.01 || y < -0.01 || y > view[1] + 0.01) outside++;
            }
        }
        ok(`${o}: همه نقاط هر ${LAYERS[o]} لایه داخل viewBox ${view[0]}×${view[1]} می‌ماند`, outside === 0);

        // ۲) لایه‌ها هم‌شکل و فقط جابه‌جا: فاصله ثابت
        const base = sample(WAY[o]);
        let drift = 0;
        for (let i = 1; i < LAYERS[o]; i++) {
            const pts = sample(WAY[o].map((p) => axis === 0
                ? [p[0] + GAP * i, p[1]] : [p[0], p[1] + GAP * i]));
            for (let k = 0; k < base.length; k++) {
                if (Math.abs(pts[k][axis] - base[k][axis] - GAP * i) > 1e-6) drift++;
                if (Math.abs(pts[k][longAxis] - base[k][longAxis]) > 1e-6) drift++;
            }
        }
        ok(`${o}: لایه‌ها دقیقاً هم‌شکل‌اند و فقط ${GAP} واحد جابه‌جا شده‌اند`, drift === 0);

        // ۳) برجستگی بزرگ در نیمه دوم (بریف: «پایین‌تر از میانه»)
        const deep = sample(WAY[o]).reduce((a, b) => (b[axis] < a[axis] ? b : a));
        ok(`${o}: برجستگی بزرگ در نیمه دوم محور طول است ` +
           `(${deep[longAxis].toFixed(0)} از ${view[longAxis]})`,
           deep[longAxis] > view[longAxis] * 0.5);

        // ۴) منحنی در محور طول عقب نمی‌رود (بدون حلقه)
        const pts = sample(WAY[o]);
        const back = pts.filter((p, k) => k && p[longAxis] < pts[k - 1][longAxis]).length;
        ok(`${o}: منحنی در محور طول عقب نمی‌رود (بدون حلقه و برگشت)`, back === 0);
    }
}

console.log('\n▸ و) کامپوننت WaveDivider.razor — مسیرها یکی‌اند');
{
    const arrays = {};
    for (const [name, key] of [['Vertical', 'vertical'], ['Horizontal', 'horizontal']]) {
        const m = new RegExp('string\\[\\] ' + name + '\\s*=\\s*\\{([\\s\\S]*?)\\};').exec(waveRazor);
        arrays[key] = m ? [...m[1].matchAll(/"([^"]+)"/g)].map((x) => x[1]) : [];
    }

    for (const o of ['vertical', 'horizontal']) {
        const expected = buildPaths(o);
        ok(`${o}: شمار مسیرها ${LAYERS[o]} است`, arrays[o].length === LAYERS[o]);
        const bad = expected.filter((d, i) => arrays[o][i] !== d);
        ok(`${o}: هر ${LAYERS[o]} مسیر مو‌به‌مو با هندسه منبع یکی است` +
           (bad.length ? ` (ناهمخوان: ${bad.length})` : ''), bad.length === 0);
        if (bad.length) console.log('      →', bad[0].slice(0, 90), '≠', (arrays[o][0] || '').slice(0, 90));
    }

    ok('viewBox دسکتاپ "0 0 100 600" است', /"0 0 100 600"/.test(waveRazor));
    ok('viewBox موبایل "0 0 600 120" است', /"0 0 600 120"/.test(waveRazor));
    ok('preserveAspectRatio="none" هست (لایه‌ها نازک می‌مانند)', /preserveAspectRatio="none"/.test(waveRazor));
    ok('موج از دید صفحه‌خوان پنهان است (aria-hidden + role)',
        /aria-hidden="true"/.test(waveRazor) && /role="presentation"/.test(waveRazor));
    // شناسه‌ها: فقط شناسه گرادیان مجاز است و باید پویا (یکتا در هر نمونه) باشد؛
    // شناسه ثابت در موجی که دو بار رندر می‌شود، url(#…) را به نمونه اول می‌فرستد.
    const svgIds = [...waveRazor.matchAll(/\sid="([^"]+)"/g)].map((m) => m[1]);
    ok(`تنها شناسه داخل SVG، گرادیان پویاست (پیدا شد: ${svgIds.join(', ') || 'هیچ'})`,
        svgIds.length === 1 && svgIds[0].includes('GoldGradientId'));
    ok('هیچ رنگ خامی در کامپوننت موج نیست (رنگ از CSS یا گرادیان می‌آید)',
        !/(fill|stroke)\s*=\s*"(?!(none|currentColor|url\())/.test(waveRazor)
        && !/#[0-9A-Fa-f]{3,6}/.test(waveRazor));
    ok('کلاس لایه‌ها با شماره ساخته می‌شود (lg-wave__layer--N)',
        /lg-wave__layer--\{i \+ 1\}/.test(waveRazor));
    ok('هدر فایل، ماشینی بودن و منبع هندسه را می‌گوید',
        /ماشینی ساخته می‌شود/.test(waveRazor) && /wave_geometry\.py/.test(waveRazor));
}

console.log('\n▸ و) چیدمان کارت در CSS');
{
    const css3 = stripComments(css);

    ok('کارت: شبکه، بیشینه عرض ۱۰۰۰ پیکسل', /\.lg-card\s*\{[\s\S]*?max-inline-size:\s*var\(--lg-card-max\)/.test(css3));
    ok('کارت: گوشه گرد و برش سرریز (موج بیرون نزند)',
        /\.lg-card\s*\{[\s\S]*?overflow:\s*hidden/.test(css3));
    ok('کارت: لایه‌بندی جداشده (isolation) تا موج به بیرون درز نکند',
        /\.lg-card\s*\{[\s\S]*?isolation:\s*isolate/.test(css3));

    // نسبت ستون‌ها و سهم فرم باید هم‌خوان باشند، وگرنه موج روی مرز نمی‌نشیند
    ok('دسکتاپ: ستون‌ها 1fr / 1.3fr هستند',
        /grid-template-columns:\s*minmax\(0,\s*1fr\)\s*minmax\(0,\s*1\.3fr\)/.test(css3));
    const share = Number(/--lg-form-share:\s*([\d.]+)%/.exec(css3)[1]);
    check('سهم ستون فرم با نسبت ۱ به ۱٫۳ هم‌خوان است', share, 100 / 2.3, 0.01);
    ok('موج دسکتاپ روی مرز ستون‌ها می‌نشیند (calc از سهم فرم)',
        /\.lg-card__wave--desktop\s*\{[\s\S]*?inset-inline-start:\s*calc\(var\(--lg-form-share\)/.test(css3));
    ok('موج دسکتاپ عرضش از توکن می‌آید (۲۴۰ پیکسل)',
        /\.lg-card__wave--desktop\s*\{[\s\S]*?inline-size:\s*var\(--lg-wave-w\)/.test(css3));

    // در هر اندازه دقیقاً یکی از دو موج دیده می‌شود
    ok('موج عمودی در موبایل پنهان است',
        /\.lg-card__wave--desktop\s*\{\s*display:\s*none/.test(css3));
    const desktopBlock = css3.substring(css3.indexOf('@media (min-width: 900px)'));
    ok('موج عمودی در دسکتاپ دیده می‌شود و موج افقی پنهان',
        /\.lg-card__wave--desktop\s*\{\s*display:\s*block/.test(desktopBlock)
        && /\.lg-card__wave--mobile\s*\{\s*display:\s*none/.test(desktopBlock));

    ok('موج هرگز جلوی لمس را نمی‌گیرد', /\.lg-card__wave\s*\{[\s\S]*?pointer-events:\s*none/.test(css3));

    // ترتیب لایه‌ها: فرم روی موج، موج روی تصویر
    const zForm = Number(/\.lg-card__form\s*\{[\s\S]*?z-index:\s*(\d)/.exec(css3)[1]);
    const zWave = Number(/\.lg-card__wave\s*\{[\s\S]*?z-index:\s*(\d)/.exec(css3)[1]);
    const zMedia = Number(/\.lg-card__media\s*\{[\s\S]*?z-index:\s*(\d)/.exec(css3)[1]);
    ok(`ترتیب لایه‌ها درست است (فرم ${zForm} > موج ${zWave} > تصویر ${zMedia})`,
        zForm > zWave && zWave > zMedia);

    // سایه‌ها: هر لایه + سایه بیرونی کل موج
    ok('هر لایه موج سایه نرم دارد (دسکتاپ به سمت تصویر)',
        /\.lg-wave--vertical \.lg-wave__layer\s*\{\s*filter:\s*drop-shadow\(-4px/.test(css3));
    ok('هر لایه موج سایه نرم دارد (موبایل به سمت بالا)',
        /\.lg-wave--horizontal \.lg-wave__layer\s*\{\s*filter:\s*drop-shadow\(2px -4px/.test(css3));
    ok('سایه بیرونی کل SVG هم هست (جانشین سافاری)', /\.lg-card__wave\s*\{[\s\S]*?drop-shadow\(0 16px 26px/.test(css3));

    // پنج رنگ لایه از توکن‌ها
    for (let i = 1; i <= 5; i++) {
        ok(`رنگ لایه ${i} از توکن می‌آید`,
            new RegExp(`\\.lg-wave__layer--${i}\\s*\\{\\s*fill:\\s*var\\(--lg-wave-${i}\\)`).test(css3));
    }

    ok('ناحیه تصویر زمینه هم‌رنگ تصویرسازی دارد (بدون درز)',
        /\.lg-card__media\s*\{[\s\S]*?background:\s*var\(--lg-photo-bg\)/.test(css3));
    ok('تصویر با object-fit: cover پوشانده می‌شود', /object-fit:\s*cover/.test(css3));
    // اگر تصویر در جریان عادی باشد، ارتفاع ذاتی‌اش ارتفاع کارت را تعیین می‌کند
    ok('ارتفاع کارت از تصویر تأثیر نمی‌گیرد (تصویر مطلق در کادر خودش)',
        /\.lg-photo\s*\{[\s\S]*?position:\s*absolute[\s\S]*?inset:\s*0/.test(css3)
        && /\.lg-card__media\s*\{[\s\S]*?position:\s*relative/.test(css3));
    ok('کادر تصویر سرریز را می‌بُرد (بدون بیرون‌زدگی)',
        /\.lg-card__media\s*\{[\s\S]*?overflow:\s*hidden/.test(css3));
    ok('جای تصویر از توکن می‌آید (صورت در کادر)', /object-position:\s*var\(--lg-photo-pos\)/.test(css3));
    ok('دسکتاپ صورت را روی ۳۰٪ عمودی می‌گیرد', /--lg-photo-pos:\s*50% 30%/.test(css3));
    ok('موبایل صورت را روی ۲۴٪ عمودی می‌گیرد', /--lg-photo-pos:\s*50% 24%/.test(css3));

    // موبایل: کارت شیشه‌ای روی موج
    ok('موبایل: کارت فرم با حاشیه منفی روی موج می‌افتد',
        /\.lg-card__form\s*\{[\s\S]*?margin-block-start:\s*calc\(-1 \* var\(--lg-card-overlap\)\)/.test(css3));
    ok('موبایل: موج به اندازه هم‌پوشانی پایین‌تر از لبه تصویر می‌نشیند',
        /\.lg-card__wave--mobile\s*\{[\s\S]*?inset-block-start:\s*calc\(var\(--lg-media-h\) - var\(--lg-wave-h\) \+ var\(--lg-card-overlap\)\)/.test(css3));
    ok('کارت شیشه‌ای بلور دارد', /backdrop-filter:\s*blur\(var\(--lg-glass-blur\)\)/.test(css3));
    ok('برای بلور جانشین بدون backdrop-filter وجود دارد',
        /@supports not \(\(backdrop-filter:/ .test(css3) && /\.lg-card__form\s*\{\s*background:\s*var\(--lg-surface\)/.test(css3));
    ok('کارت شیشه‌ای پایینش مات می‌شود (خوانایی متن و برچسب)',
        /background:\s*linear-gradient\(180deg,[\s\S]*?var\(--lg-glass-solid-at\)\)/.test(css3));

    // ارتفاع ناحیه تصویر موبایل: ~۴۴٪ و کف ۲۸۰ پیکسل
    const media = /--lg-media-h:\s*clamp\((\d+)px,\s*44svh,\s*(\d+)px\)/.exec(css3);
    ok('ارتفاع ناحیه تصویر موبایل clamp(280px, 44svh, 400px) است',
        !!media && Number(media[1]) === 280 && Number(media[2]) === 400);
    ok('ارتفاع دید با svh می‌آید (نوار آدرس موبایل حساب می‌شود)', /--lg-media-h:\s*clamp\(280px,\s*44svh/.test(css3));

    // آینه‌شوندگی برای اسناد چپ‌به‌راست
    ok('اگر جهت سند چپ‌به‌راست شود، موج آینه می‌شود',
        /\[dir="ltr"\] \.lg-wave\s*\{\s*transform:\s*scaleX\(-1\)/.test(css3));
}

console.log('\n▸ و) کامپوننت LoginCard.razor');
{
    // ترتیب DOM تعیین می‌کند فرم راست بنشیند
    const iForm = cardRazor.indexOf('lg-card__form');
    const iMedia = cardRazor.indexOf('lg-card__media');
    ok('ستون فرم در DOM اول است (در راست‌به‌چپ یعنی سمت راست)', iForm > 0 && iForm < iMedia);
    ok('ناحیه تصویر بعد از فرم می‌آید (سمت چپ)', iMedia > iForm);

    ok('عنوان صفحه با id به بخش فرم وصل شده (aria-labelledby + h1)',
        /aria-labelledby="lg-title"/.test(cardRazor) && /<h1 class="lg-title" id="lg-title"/.test(cardRazor));
    ok('تصویر تزئینی است: alt خالی و aria-hidden',
        /alt=""\s+aria-hidden="true"/.test(cardRazor) || /alt=""[\s\S]{0,60}aria-hidden="true"/.test(cardRazor));
    ok('تصویر ابعاد ثابت دارد (بدون جابه‌جایی چیدمان)',
        /width="736"\s+height="983"/.test(cardRazor));
    ok('نسخه‌های بهینه AVIF و WebP با جانشین JPEG هست',
        /login-illustration\.avif/.test(cardRazor) && /login-illustration\.webp/.test(cardRazor)
        && /login-illustration\.jpg/.test(cardRazor));
    ok('تصویر از تنظیمات سالن می‌آید و جانشین دارد',
        /IllustrationUrl/.test(cardRazor) && /<picture>/.test(cardRazor));
    ok('لوگو از تنظیمات می‌آید و نشان گرادیانی جانشینش است',
        /LogoUrl/.test(cardRazor) && /lg-brand__mark/.test(cardRazor));

    ok('هر دو جهت موج در کارت هستند (CSS یکی را نشان می‌دهد)',
        /Orientation="vertical"/.test(cardRazor) && /Orientation="horizontal"/.test(cardRazor));

    // فرم بی‌حالت: داده و رویداد از بیرون
    ok('ارسال فرم با preventDefault مهار شده (صفحه بارگذاری نمی‌شود)',
        /@onsubmit:preventDefault/.test(cardRazor));
    ok('رویدادها به بیرون واگذار شده‌اند (OnSubmit/OnOtp)',
        /EventCallback OnSubmit/.test(cardRazor) && /EventCallback OnOtp/.test(cardRazor));
    ok('دکمه ورود نوع submit دارد (Enter در فیلد کار می‌کند)', /Type="submit"/.test(cardRazor));
    ok('پیوند فراموشی رمز بیرون از فرم است (submit ناخواسته نمی‌شود)', (() => {
        const close = cardRazor.indexOf('</form>');
        return cardRazor.indexOf('lg-link') > close;
    })());
    ok('فیلدها با @bind دوطرفه وصل شده‌اند',
        /@bind-Value="Mobile"/.test(cardRazor) && /@bind-Value="Password"/.test(cardRazor));
    ok('حالت بارگذاری دکمه ورود به بیرون وصل است', /Loading="@Loading"/.test(cardRazor));
    // هر فیلدی که کارت رندر می‌کند باید اجباری باشد (در هر دو گام)
    const fieldCount = (cardRazor.match(/<(CapsuleField|PasswordField)/g) || []).length;
    ok(`همه ${fieldCount} فیلد کارت اجباری‌اند (ستاره از کامپوننت پایه)`,
        (cardRazor.match(/Required="true"/g) || []).length === fieldCount && fieldCount >= 4);
    ok('شماره موبایل با صفحه‌کلید tel و autocomplete=username',
        /Type="tel"/.test(cardRazor) && /Autocomplete="username"/.test(cardRazor));
    ok('نام سالن پارامتر است و مقدار پیش‌فرض دارد', /SalonName/.test(cardRazor));
}

console.log('\n▸ و) پیش‌نمایش چیدمان (login-layout-preview.html)');
{
    ok('فایل پیش‌نمایش چیدمان وجود دارد', preview3.length > 50000);
    ok('هیچ برچسب جاگذاری‌نشده‌ای ندارد', !/__[A-Z0-9_]+__/.test(preview3));
    ok('CSS کامل در پیش‌نمایش جاسازی شده (خود فایل، نه بازنویسی)',
        preview3.includes(css.trim().slice(0, 400)));

    // مسیرهای موج در پیش‌نمایش باید همان مسیرهای رِیزور باشند
    for (const [o, cls] of [['vertical', 'vertical'], ['horizontal', 'horizontal']]) {
        const svg = new RegExp('<svg class="lg-wave lg-wave--' + cls + '[\\s\\S]*?</svg>').exec(preview3);
        ok(`${o}: SVG موج در پیش‌نمایش هست`, !!svg);
        if (svg) {
            // فقط مسیرهای لایه، نه خطوط طلایی که آنها هم ویژگی d دارند
            const ds = [...svg[0].matchAll(/class="lg-wave__layer lg-wave__layer--\d" d="([^"]+)"/g)]
                .map((m) => m[1]);
            const expected = buildPaths(o);
            const bad = expected.filter((d, i) => ds[i] !== d);
            ok(`${o}: ${LAYERS[o]} مسیر پیش‌نمایش با هندسه منبع یکی است`,
                ds.length === LAYERS[o] && bad.length === 0);
            ok(`${o}: SVG پیش‌نمایش aria-hidden و preserveAspectRatio دارد`,
                /aria-hidden="true"/.test(svg[0]) && /preserveAspectRatio="none"/.test(svg[0]));
        }
    }

    ok('چهار قالب پایه در پیش‌نمایش هست (دسکتاپ و موبایل، روشن و تیره)',
        (preview3.match(/class="lg-card"/g) || []).length >= 4);
    ok('حالت تیره در پیش‌نمایش با کلاس lg--dark است (نه رنگ دستی)',
        (preview3.match(/class="lg lg--dark"/g) || []).length === 2);
    ok('تصویر فقط یک‌بار در فایل نشسته (data URI تکراری نیست)',
        (preview3.match(/data:image\/jpeg;base64/g) || []).length === 1);
    ok('هیچ تگ شکسته‌ای نیست: تعداد کوتیشن‌ها در هر تگ img/track زوج است',
        (preview3.match(/<img [^>]*>/g) || []).every((t) => (t.match(/"/g) || []).length % 2 === 0));
}

console.log('\n▸ و) هم‌گامی کامپوننت موج با منبع هندسه');
{
    // چرا فراخوانی پایتون: منبع هندسه پایتون است؛ اگر کسی عددها را در
    // wave_geometry.py عوض کند و فراموش کند کامپوننت را بازسازی کند، فقط
    // همین بررسی (و مقایسه مستقل بالا) می‌تواند بگیردش.
    const { execFileSync } = require('child_process');
    let out = '', code = 0;
    try {
        out = execFileSync('python3', [path.join(ROOT, 'tools', 'build-wave-component.py'), '--check'],
            { encoding: 'utf8' });
    } catch (e) {
        code = 1;
        out = (e.stdout || '') + (e.stderr || '');
    }
    ok('WaveDivider.razor با tools/wave_geometry.py هم‌گام است', code === 0);
    if (code !== 0) console.log('      →', out.trim().split('\n').join('\n      → '));
}


/* ==========================================================================
   ز) مرحله ۴ — خط طلایی دوگانه
   --------------------------------------------------------------------------
   مسیر خط دوم در پایتون ساخته می‌شود (جابه‌جایی عمود بر منحنی + نمونه‌برداری
   یکنواخت). اینجا آن هندسه مستقل بازبینی می‌شود: هر نقطه خط دوم باید سمت
   داخل نوار باشد و فاصله‌اش از خط اصلی نزدیک مقدار توکن‌شده بماند.
   ========================================================================== */

console.log('\n▸ ز) خط طلایی — توکن‌ها و CSS');
{
    const css4 = stripComments(css);

    const metals = ['#7A5A22', '#FBE9B4', '#A8854B', '#FFF3CF', '#8A6A2F'];
    metals.forEach((hex, i) => {
        ok(`ایستگاه ${i + 1} گرادیان فلزی = ${hex} (بریف)`,
            new RegExp('--lg-gold-metal-' + (i + 1) + ':\\s*' + hex, 'i').test(css4));
    });
    for (let i = 1; i <= 5; i++) {
        ok(`رنگ ایستگاه ${i} از توکن به گرادیان وصل است (stop-color)`,
            new RegExp('\\.lg-gold__stop-' + i + '\\s*\\{\\s*stop-color:\\s*var\\(--lg-gold-metal-' + i + '\\)').test(css4));
    }

    ok('ضخامت خط اصلی ۱٫۸ پیکسل است', /--lg-gold-line-w1:\s*1\.8px/.test(css4));
    ok('ضخامت خط دوم ۱٫۱ پیکسل است', /--lg-gold-line-w2:\s*1\.1px/.test(css4));
    ok('خط دوم کم‌رنگ‌تر است (stroke-opacity از توکن)',
        /--lg-gold-line-op2:\s*0\.\d+/.test(css4)
        && /\.lg-wave__line--thin\s*\{[\s\S]*?stroke-opacity:\s*var\(--lg-gold-line-op2\)/.test(css4));
    ok('ضخامت هر دو خط از توکن می‌آید',
        /\.lg-wave__line--main\s*\{[\s\S]*?stroke-width:\s*var\(--lg-gold-line-w1\)/.test(css4)
        && /\.lg-wave__line--thin\s*\{[\s\S]*?stroke-width:\s*var\(--lg-gold-line-w2\)/.test(css4));

    // بریف: «هیچ سایه تیره‌ای روی خط‌ها». سایه تیره روی لایه یا گروه خط‌ها ممنوع است.
    ok('خط‌ها هیچ سایه تیره‌ای نمی‌گیرند (بریف)',
        !/\.lg-wave__line[^{]*\{[^}]*filter/.test(css4)
        && !/\.lg-wave__gold[^{]*\{[^}]*filter/.test(css4));
    ok('درخشش روشن روی موج هست (هاله طلایی، نه سایه تیره)',
        /drop-shadow\(0 0 7px var\(--lg-gold-glow\)\)/.test(css4));

    ok('لایه کنار تصویر سایه لایه‌ای ندارد (سایه‌اش روی خط می‌افتاد)',
        /\.lg-wave--vertical \.lg-wave__layer--1,\s*\.lg-wave--horizontal \.lg-wave__layer--1\s*\{\s*filter:\s*none/.test(css4));
    ok('سایه لایه‌های ۲ تا ۵ سرجایش هست',
        /\.lg-wave--vertical \.lg-wave__layer\s*\{[\s\S]*?drop-shadow\(-4px/.test(css4));
}

console.log('\n▸ ز) خط طلایی — هندسه مسیرها');
{
    // مسیر خط اصلی باید مو‌به‌مو همان لبه لایه ۱ باشد، منهای بخش پرشدنی
    const goldMain = {
        vertical: (/GoldMainV = "([^"]+)"/.exec(waveRazor) || [])[1],
        horizontal: (/GoldMainH = "([^"]+)"/.exec(waveRazor) || [])[1],
    };
    const goldThin = {
        vertical: (/GoldThinV = "([^"]+)"/.exec(waveRazor) || [])[1],
        horizontal: (/GoldThinH = "([^"]+)"/.exec(waveRazor) || [])[1],
    };

    ok('هر چهار مسیر طلایی در کامپوننت هستند',
        [goldMain.vertical, goldMain.horizontal, goldThin.vertical, goldThin.horizontal]
            .every((v) => typeof v === 'string' && v.length > 50));

    for (const o of ['vertical', 'horizontal']) {
        const close = o === 'vertical' ? ' L 100 600 L 100 0 Z' : ' L 600 120 L 0 120 Z';
        ok(`${o}: خط اصلی دقیقاً روی لبه لایه ۱ است (همان مسیر، بدون بخش پرشدنی)`,
            goldMain[o] === buildPaths(o)[0].replace(close, ''));

        const pts = [...(goldThin[o] || '').matchAll(/(-?[\d.]+) (-?[\d.]+)/g)]
            .map((m) => [Number(m[1]), Number(m[2])]);
        ok(`${o}: خط دوم مسیر نقطه‌ای دارد (${pts.length} نقطه)`, pts.length > 40);

        const view = VIEWS[o], axis = AXIS[o];
        const out = pts.filter(([x, y]) =>
            x < -0.01 || x > view[0] + 0.01 || y < -0.01 || y > view[1] + 0.01);
        ok(`${o}: همه نقاط خط دوم داخل viewBox می‌مانند`, out.length === 0);

        // مستقل: هر نقطه خط دوم باید سمت داخل نوار و در فاصله معقول باشد
        const mainPts = sample(WAY[o]);
        const ref = o === 'vertical' ? [240, 600] : [390, 96];
        const sx = ref[0] / view[0], sy = ref[1] / view[1];
        let minGap = 1e9, maxGap = 0, wrongSide = 0;
        for (const [x, y] of pts) {
            let best = 1e9, bestIdx = 0;
            for (let k = 0; k < mainPts.length; k++) {
                const d = Math.hypot((mainPts[k][0] - x) * sx, (mainPts[k][1] - y) * sy);
                if (d < best) { best = d; bestIdx = k; }
            }
            minGap = Math.min(minGap, best);
            maxGap = Math.max(maxGap, best);
            const inside = axis === 0 ? (x - mainPts[bestIdx][0] > 0) : (y - mainPts[bestIdx][1] > 0);
            if (!inside) wrongSide++;
        }
        // کران‌ها کمی سخاوتمندانه‌اند چون دو مسیر با تراکم متفاوت نمونه‌برداری شده‌اند
        ok(`${o}: خط دوم همه‌جا سمت داخل نوار است (نه روی تصویر)`, wrongSide === 0);
        ok(`${o}: فاصله خط دوم از خط اصلی ${minGap.toFixed(1)} .. ${maxGap.toFixed(1)} پیکسل است ` +
           `(هدف ${GOLD_GAP_PX})`, minGap > 2.5 && maxGap < 9);
    }
}

console.log('\n▸ ز) خط طلایی — قالب SVG در کامپوننت');
{
    ok('گرادیان فلزی با gradientUnits="userSpaceOnUse" تعریف شده',
        /<linearGradient id="@GoldGradientId"[\s\S]*?gradientUnits="userSpaceOnUse"/.test(waveRazor));
    ok('پنج ایستگاه گرادیان با کلاس توکن‌دار ساخته می‌شوند',
        /for \(var i = 0; i < 5; i\+\+\)/.test(waveRazor)
        && /lg-gold__stop-\{i \+ 1\}/.test(waveRazor));
    ok('گرادیان دسکتاپ عمودی و موبایل افقی است',
        /GradX2 => IsVertical \? "0" : "600"/.test(waveRazor)
        && /GradY2 => IsVertical \? "600" : "0"/.test(waveRazor));
    ok('شناسه گرادیان در هر نمونه یکتاست (موج دو بار در یک صفحه رندر می‌شود)',
        /Guid\.NewGuid\(\)/.test(waveRazor) && /lg-gold-\{_uid\}/.test(waveRazor));
    ok('هر دو خط به همان گرادیان وصل‌اند',
        (waveRazor.match(/stroke="url\(#@GoldGradientId\)"/g) || []).length === 2);
    ok('non-scaling-stroke روی خود مسیر است (نه فقط در CSS)',
        (waveRazor.match(/<path[^>]*vector-effect="non-scaling-stroke"/g) || []).length === 2);
    ok('خطوط هیچ رنگ خامی ندارند (فقط url(#…))',
        !/#[0-9A-Fa-f]{3,6}/.test(waveRazor) && !/stroke="(?!url|none)/.test(waveRazor));
    ok('گروه طلایی از دید صفحه‌خوان و لمس بی‌اثر است',
        /class="lg-wave__gold"/.test(waveRazor)
        && /\.lg-wave__gold\s*\{\s*pointer-events:\s*none/.test(css));
}

console.log('\n▸ ز) خط طلایی در پیش‌نمایش چیدمان');
{
    ok('پیش‌نمایش خط طلایی دارد', preview3.includes('lg-wave__line--main'));
    const grads = [...preview3.matchAll(/linearGradient id="([^"]+)"/g)].map((m) => m[1]);
    ok(`شناسه گرادیان‌ها یکتا هستند (${grads.length} نمونه، ${new Set(grads).size} یکتا)`,
        grads.length >= 8 && new Set(grads).size === grads.length);
    ok('هر دو خط در پیش‌نمایش به گرادیان نمونه خودشان وصل‌اند (نه به اولی)',
        [...preview3.matchAll(/stroke="url\(#([^)]+)\)"/g)].every((m) => grads.includes(m[1])));
    // هر کارت دو موج دارد (عمودی + افقی) و هر موج دو خط طلایی
    const cardCount = (preview3.match(/class="lg-card"/g) || []).length;
    ok(`پیش‌نمایش ${cardCount} کارت × ۲ موج × ۲ خط طلایی دارد`,
        (preview3.match(/lg-wave__line--main/g) || []).length === cardCount * 2 + 1);
}

/* ==========================================================================
   ح) مرحله ۶ — اعتبارسنجی، حالت بارگذاری، API ورود/OTP و برگ بازیابی
   --------------------------------------------------------------------------
   اینجا «رفتار» بررسی می‌شود، نه ظاهر: الگوی اعتبارسنجی، اینکه پیام خطا زیر
   خودِ فیلد می‌نشیند، اینکه در حین کار دکمه‌ها غیرفعال می‌شوند، اینکه هر
   دکمه به کدام متد وصل است، و اینکه مسیر بازگشت (returnUrl) نمی‌تواند کاربر
   را به دامنه دیگری بفرستد.
   ========================================================================== */

console.log('\n▸ ح) اعتبارسنجی فارسی — الگوها و پیام‌ها');
{
    ok('الگوی شماره موبایل ^09\\d{9}$ است', /\^09\\d\{9\}\$/.test(loginPage));
    ok('الگوی کد تأیید ^\\d{5}$ است', /\^\\d\{5\}\$/.test(loginPage));
    ok('حداقل طول رمز عبور ۶ کاراکتر است', /Length < 6/.test(loginPage));
    ok('ورودی پیش از بررسی با DigitHelper.Normalize یکدست می‌شود (ارقام فارسی)',
        (loginPage.match(/DigitHelper\.Normalize/g) || []).length >= 5);

    for (const msg of ['شماره موبایل را وارد کنید.', 'شماره موبایل باید ۱۱ رقم و با ۰۹ شروع شود.',
                       'رمز عبور را وارد کنید.', 'رمز عبور باید حداقل ۶ کاراکتر باشد.',
                       'کد تأیید را وارد کنید.', 'کد تأیید ۵ رقمی است.']) {
        ok(`پیام فارسی هست: «${msg}»`, loginPage.includes(msg));
    }

    ok('اعتبارسنجی پیش از فراخوانی API انجام می‌شود (پیام بدون رفت‌وبرگشت شبکه)',
        /_mobileError = ValidateMobile\(_mobile\);\s*\n\s*_passwordError = ValidatePassword\(_password\);\s*\n\s*if \(_mobileError is not null \|\| _passwordError is not null\) return;/
            .test(loginPage));
    // ⚠ درس گرفته‌شده: نام متد در توضیح بالای فایل هم آمده و indexOf زودتر
    // به آن می‌رسد. پس «فراخوانی» با پرانتز جست‌وجو می‌شود.
    ok('در اعتبارسنجی ناموفق، API صدا زده نمی‌شود',
        loginPage.indexOf('if (_mobileError is not null || _passwordError is not null) return;')
        < loginPage.indexOf('Auth.LoginWithPasswordAsync('));
}

console.log('\n▸ ح) پیام خطا زیر فیلد — نه فقط بالای فرم');
{
    ok('خطای هر فیلد به همان فیلد پاس داده می‌شود',
        /MobileError="@_mobileError"/.test(loginPage)
        && /PasswordError="@_passwordError"/.test(loginPage)
        && /CodeError="@_codeError"/.test(loginPage));
    ok('کارت خطای هر فیلد را به کامپوننت پایه می‌دهد',
        (cardRazor.match(/Error="@\w+Error"/g) || []).length >= 4);
    ok('پیام خطای فیلد role="alert" دارد (اعلام در لحظه ظاهر شدن)',
        /role="alert"/.test(capsuleRazor));
    ok('پیام کلی فرم هم ناحیه زنده است', /role="alert"/.test(alertRazor));
    ok('AuthAlert برای پیام موفق آیکون check و برای خطا alert می‌گذارد',
        /Kind == "ok" \? "check" : "alert"/.test(alertRazor));
    ok('فیلد نامعتبر aria-invalid می‌گیرد', /aria-invalid="\$?\{?@?\(Error is not null/.test(capsuleRazor)
        || /aria-invalid="@\(Error is not null/.test(capsuleRazor));
}

console.log('\n▸ ح) حالت بارگذاری و جلوگیری از ارسال دوباره');
{
    ok('ارسال کد و ورود دو پرچم جدا دارند (هم‌زمان قفل نمی‌شوند)',
        /private bool _busy;/.test(loginPage) && /private bool _sending;/.test(loginPage));
    ok('هر متد ورود با محافظ _busy شروع می‌شود',
        /if \(_busy\) return;/.test(loginPage) && /if \(_sending\) return;/.test(loginPage));
    ok('دکمه اصلی حالت بارگذاری خود را از صفحه می‌گیرد', /Loading="@_busy"/.test(loginPage));
    ok('دکمه کد یکبار مصرف، حالت ارسال را نشان می‌دهد',
        /SendingCode="@_sending"/.test(loginPage) && /Loading="@SendingCode"/.test(cardRazor));
    ok('در حین ورود، دکمه کد یکبار مصرف غیرفعال می‌شود',
        /Disabled="@\(Loading \|\| SendingCode\)"/.test(cardRazor));
    ok('دکمه تأیید کد در حین بررسی غیرفعال و اسپینر‌دار است',
        /Loading="@Loading"\s*\n\s*LoadingText="در حال بررسی…"/.test(cardRazor));
    ok('AuthButton در حالت بارگذاری aria-busy می‌گیرد',
        /aria-busy="@\(Loading \? "true" : null\)"/.test(buttonRazor));
    ok('پیوند «ارسال دوباره کد» در حین کار غیرفعال است',
        /disabled="@\(Loading \|\| SendingCode\)"/.test(cardRazor)
        && /\.lg-link:disabled/.test(css));
}

console.log('\n▸ ح) سیم‌کشی API — هر دکمه به کدام متد');
{
    ok('ورود با رمز عبور → Auth.LoginWithPasswordAsync',
        /Auth\.LoginWithPasswordAsync\(/.test(loginPage));
    ok('ارسال کد → Auth.SendCodeAsync', /Auth\.SendCodeAsync\(_mobile\)/.test(loginPage));
    ok('ورود با کد → Auth.LoginAsync با LoginRequest',
        /Auth\.LoginAsync\(new LoginRequest/.test(loginPage)
        && /MobileNumber = DigitHelper\.Normalize\(_mobile\)/.test(loginPage)
        && /Code = DigitHelper\.Normalize\(_code\)/.test(loginPage));
    ok('بازیابی رمز → Auth.ResetPasswordAsync', /Auth\.ResetPasswordAsync\(/.test(loginPage));

    ok('دکمه اصلی گام تعیین می‌کند کدام کار انجام شود',
        /if \(_step == "otp"\) \{ await SubmitOtpAsync\(\); return; \}/.test(loginPage));
    ok('رفتن به گام کد، اول شماره را بررسی می‌کند (کد بی‌مقصد فرستاده نمی‌شود)',
        /_mobileError = ValidateMobile\(_mobile\);\s*\n\s*if \(_mobileError is not null\) return;/.test(loginPage));
    ok('پس از ورود موفق، به پنل نقش کاربر می‌رود',
        /AppRoles\.PanelUrl\(/.test(loginPage) && /Nav\.NavigateTo\(ResolveTarget\(/.test(loginPage));
    // معیار: کارت هیچ تزریق وابستگی و هیچ ارجاعی به لایه شبکه ندارد و
    // همه رویدادهایش از نوع EventCallback است (یعنی منطق از بیرون می‌آید).
    ok('کارت هیچ وابستگی به API ندارد (بدون @inject و بدون AuthService/HttpClient)',
        !/@inject/.test(cardRazor) && !/AuthService|HttpClient/.test(cardRazor));
    // ۳ دوطرفه (ValueChanged) + ۵ رویداد؛ همه باید [Parameter] باشند
    const callbacks = (cardRazor.match(/\[Parameter\] public EventCallback[^\n]*/g) || []);
    const allCallbacks = (cardRazor.match(/public EventCallback[^\n]*/g) || []);
    ok(`همه ${allCallbacks.length} رویداد کارت پارامتر و EventCallback هستند ` +
       `(${callbacks.length} پارامتر، ۳ دوطرفه)`,
        allCallbacks.length === 8 && callbacks.length === 8
        && (cardRazor.match(/public EventCallback<string\?>/g) || []).length === 3);
}

console.log('\n▸ ح) returnUrl — بازگشت به صفحه درخواستی، بدون دام خارجی');
{
    ok('returnUrl از پارامتر آدرس خوانده می‌شود',
        /\[SupplyParameterFromQuery\(Name = "returnUrl"\)\]/.test(loginPage));
    ok('فقط مسیر داخلی پذیرفته می‌شود (نه آدرس کامل با ://)',
        /url\.Contains\(":\/\/"\)/.test(loginPage) && /StartsWith\("\/"\)/.test(loginPage));
    ok('مسیر // (پروتکل‌نسبی) و /\\ رد می‌شود',
        loginPage.includes('!url.StartsWith("//")')
        && loginPage.includes('!url.StartsWith("/\\\\")'));
    ok('بازگشت به خودِ صفحه ورود بی‌معناست و به پنل می‌رود',
        /url\.StartsWith\("\/login"/.test(loginPage));
    ok('کاربر وارد‌شده‌ای که صفحه ورود را باز کند، مستقیم به پنل می‌رود',
        /IsAuthenticated == true\)\s*\n\s*Nav\.NavigateTo\(ResolveTarget/.test(loginPage));
}

console.log('\n▸ ح) طرح بی‌پوسته و برگ بازیابی');
{
    ok('صفحه ورود طرح خودش را دارد', /@layout BlazorAppSolon\.Layout\.AuthLayout/.test(loginPage));
    // ⚠ درس گرفته‌شده: توضیح فارسی خودِ همین فایل از «هدر، فوتر، نوار پایین»
    // حرف می‌زند؛ بدون حذف توضیح‌ها، آزمون به‌جای کد، توضیح را می‌سنجد.
    const authLayoutBody = authLayout.replace(/@\*[\s\S]*?\*@/g, '');
    ok('طرح بی‌پوسته فقط @Body است (بدون هدر، فوتر و نوار پایین)',
        /@Body/.test(authLayoutBody)
        && !/SiteHeader|SiteFooter|MobileBottomNav|main-content-flow/.test(authLayoutBody)
        && authLayoutBody.trim().split('\n').filter((l) => l.trim() && !l.startsWith('@inherits')).length === 1);
    ok('صفحه ورود دیگر هدر/فوتر سایت را رندر نمی‌کند',
        !/SiteHeader|SiteFooter|MobileBottomNav/.test(loginPage));
    ok('کارت داخل پوسته .lg → .lg-shell است',
        /<div class="lg">/.test(loginPage) && /<div class="lg-shell">/.test(loginPage));

    ok('برگ بازیابی role=dialog و aria-modal دارد',
        /role="dialog"/.test(sheetRazor) && /aria-modal="true"/.test(sheetRazor));
    ok('عنوان برگ با aria-labelledby به دیالوگ وصل است',
        /aria-labelledby="lg-sheet-title"/.test(sheetRazor) && /id="lg-sheet-title"/.test(sheetRazor));
    ok('دکمه بستن برچسب متنی دارد', /aria-label="بستن پنجره بازیابی"/.test(sheetRazor));
    ok('کلیک روی پرده و کلید Escape هر دو می‌بندند',
        /class="lg-sheet__scrim" @onclick="CloseAsync"/.test(sheetRazor)
        && /e\.Key == "Escape"/.test(sheetRazor));
    ok('فوکوس اولیه روی فیلد شماره موبایل می‌رود (بدون جاوااسکریپت سفارشی)',
        /FocusFirstFieldAsync/.test(sheetRazor) && /_inputRef\.FocusAsync\(\)/.test(capsuleRazor));
    ok('برگ همان کامپوننت‌های پایه را استفاده می‌کند (نه فیلد دست‌ساز)',
        (sheetRazor.match(/<CapsuleField/g) || []).length === 3
        && /<AuthButton/.test(sheetRazor) && /<AuthAlert/.test(sheetRazor));
    ok('خطاها هم در برگ زیر همان فیلد می‌آیند',
        (sheetRazor.match(/Error="@\w+Error"/g) || []).length === 3);
    ok('فرم برگ هم با preventDefault مهار شده', /@onsubmit:preventDefault/.test(sheetRazor));

    ok('تراشه‌های حساب آزمایشی فقط در بیلد توسعه کامپایل می‌شوند',
        /#if DEBUG/.test(loginPage) && /ShowDevTools/.test(loginPage));
}

console.log('\n▸ ح) CSS برگ بازیابی و تراشه‌ها');
{
    const css6 = stripComments(css);
    ok('برگ در موبایل به پایین می‌چسبد', /\.lg-sheet\s*\{[\s\S]*?align-items:\s*flex-end/.test(css6));
    ok('برگ در دسکتاپ به میانه می‌آید',
        /@media \(min-width: 900px\)\s*\{[\s\S]*?\.lg-sheet\s*\{\s*align-items:\s*center/.test(css6));
    ok('ارتفاع برگ مهار شده تا با کیبورد باز هم جا بماند', /max-block-size:\s*90svh/.test(css6));
    ok('پرده رنگش از توکن می‌آید', /--lg-scrim:\s*rgba\(/.test(css6) && /\.lg-sheet__scrim\s*\{[\s\S]*?var\(--lg-scrim\)/.test(css6));
    ok('دکمه بستن هدف لمس ۴۴ پیکسلی دارد',
        /\.lg-sheet__close\s*\{[\s\S]*?inline-size:\s*var\(--lg-touch\)[\s\S]*?block-size:\s*var\(--lg-touch\)/.test(css6));
    ok('تراشه آزمایشی هم ۴۴ پیکسل است', /\.lg-dev__chip\s*\{[\s\S]*?min-block-size:\s*var\(--lg-touch\)/.test(css6));
    ok('همه رنگ‌ها در بخش تازه توکنی هستند (بدون کدرنگ پراکنده)', (() => {
        const start = css.indexOf('۱۸) برگ بازیابی رمز');
        const block = stripComments(css.substring(start));
        return !/#[0-9A-Fa-f]{6}/.test(block) && !/rgba?\(/.test(block);
    })());
    ok('در reduced-motion گذار برگ و تراشه‌ها هم خاموش می‌شود',
        /\.lg-sheet__close,\s*\n\s*\.lg-dev__chip\s*\{\s*transition:\s*none/.test(css6));
}

console.log('\n▸ ح) پیش‌نمایش مرحله ۶');
{
    const cards = (preview3.match(/class="lg-card"/g) || []).length;
    ok(`پیش‌نمایش ${cards} قالب دارد (پایه + گام OTP + چهار حالت اعتبارسنجی)`, cards === 9);
    ok('گام کد یکبار مصرف در پیش‌نمایش هست',
        preview3.includes('تأیید و ورود') && preview3.includes('lg-mobile-otp'));
    ok('فیلد کد با ورودی‌مُد عددی و autocomplete یکبارمصرف',
        /lg-code-otp[\s\S]{0,400}?one-time-code/.test(preview3));
    ok('چهار حالت اعتبارسنجی و بارگذاری نمایش داده شده‌اند',
        (preview3.match(/lg-field__error/g) || []).length >= 4
        && preview3.includes('در حال ورود…'));
    ok('برگ بازیابی در پیش‌نمایش باز است (موبایل و دسکتاپ)',
        (preview3.match(/class="lg-sheet__panel"/g) || []).length === 2);
    ok('پیام موفق بازیابی در پیش‌نمایش هست', preview3.includes('رمز عبور با موفقیت تغییر کرد'));
    ok('هیچ برچسب جاگذاری‌نشده‌ای نیست', !/__[A-Z0-9_]+__/.test(preview3));
}

/* ==========================================================================
   نتیجه
   ========================================================================== */

console.log('\n' + (failed === 0
    ? '✅ همه بررسی‌ها موفق'
    : `❌ ${failed} بررسی ناموفق`));
process.exit(failed === 0 ? 0 : 1);
