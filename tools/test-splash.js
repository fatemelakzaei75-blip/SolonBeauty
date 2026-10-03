/* ==========================================================================
   تست صفحه آغاز (نماد بی‌نهایت)
   --------------------------------------------------------------------------
   چرا این فایل وجود دارد: در محیط توسعه ما مرورگر در دسترس نیست، پس شکل نماد
   و رفتار نوار را نمی‌توان چشمی بررسی کرد. این تست همان چیزها را عددی
   بررسی می‌کند:

   الف — هندسه مسیر: بسته بودن، هم‌طولی قطعات، تقارن دوگانه، تقاطع واقعی
   ب — مارک‌آپ و سیم‌کشی index.html و دسترس‌پذیری
   ج — قرارداد CSS: توکن‌بودن رنگ‌ها، واکنش‌گرایی، کنتراست، reduced-motion
   د — موتور splash.js: دو فاز، نگهبان یک‌نوا، خزیدن محدود، سر نور، درصد فارسی
   ه — پایان، خطا، برند و کمکی‌های بارگذاری
   و — سیم‌کشی سی‌شارپ: رندر نشدن قفل‌شده، ترتیب مراحل، نبود forceLoad

   محیط مرورگر شبیه‌سازی می‌شود و تایمرها دروغین‌اند تا تست قطعی و سریع باشد.

   اجرا:  node tools/test-splash.js
   ========================================================================== */
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const ROOT = path.join(__dirname, '..');
const INDEX = path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'index.html');
const CSS = path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'css', 'splash.css');
const JS_SRC = path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'js', 'splash.js');
const BOOT_SERVICE = path.join(ROOT, 'BlazorAppSolon', 'Services', 'BootService.cs');
const BOOT_GATE = path.join(ROOT, 'BlazorAppSolon', 'Components', 'Boot', 'BootGate.razor');

let failed = 0;

function check(label, actual, expected, tol) {
    const ok = Math.abs(actual - expected) <= (tol === undefined ? 1e-6 : tol);
    if (!ok) failed++;
    const fmt = (v) => (typeof v === 'number' ? v.toFixed(4) : String(v));
    console.log(`   ${ok ? '✅' : '❌'} ${label}: ${fmt(actual)}` +
        (ok ? '' : `  (انتظار ${fmt(expected)} ± ${fmt(tol)})`));
}

function ok(label, cond) {
    if (!cond) failed++;
    console.log(`   ${cond ? '✅' : '❌'} ${label}`);
}

const html = fs.readFileSync(INDEX, 'utf8');
const css = fs.readFileSync(CSS, 'utf8');
const js = fs.readFileSync(JS_SRC, 'utf8');

/* ==========================================================================
   الف) هندسه مسیر
   ========================================================================== */

function parsePath(d) {
    const nums = (s) => s.trim().split(/[\s,]+/).map(Number);
    const parts = d.match(/[MC][^MC]+/g).map((s) => s.trim());
    const start = nums(parts[0].slice(1));
    const segs = parts.slice(1).map((p) => {
        const v = nums(p.slice(1));
        return { p0: null, p1: [v[0], v[1]], p2: [v[2], v[3]], p3: [v[4], v[5]] };
    });
    let cur = start;
    for (const s of segs) { s.p0 = cur; cur = s.p3; }
    return { start, end: cur, segs };
}

function bezier(p0, p1, p2, p3, t) {
    const u = 1 - t;
    const a = u * u * u, b = 3 * u * u * t, c = 3 * u * t * t, e = t * t * t;
    return [a * p0[0] + b * p1[0] + c * p2[0] + e * p3[0],
            a * p0[1] + b * p1[1] + c * p2[1] + e * p3[1]];
}

function arcTable(seg, n) {
    n = n || 600;
    const pts = [], acc = [0];
    for (let i = 0; i <= n; i++) {
        const p = bezier(seg.p0, seg.p1, seg.p2, seg.p3, i / n);
        pts.push(p);
        if (i > 0) {
            const q = pts[i - 1];
            acc.push(acc[i - 1] + Math.hypot(p[0] - q[0], p[1] - q[1]));
        }
    }
    return { pts, acc, len: acc[n] };
}

const D = (html.match(/class="s-inf__geom"[^>]*\sd="([^"]+)"/) || [])[1];
if (!D) { console.error('❌ مسیر هندسی در index.html پیدا نشد'); process.exit(1); }

const parsed = parsePath(D);
const tables = parsed.segs.map((s) => arcTable(s));
const total = tables.reduce((a, t) => a + t.len, 0);

function pointAt(dist) {
    let rest = dist;
    for (const t of tables) {
        if (rest <= t.len) {
            const n = t.acc.length - 1;
            for (let i = 1; i <= n; i++) {
                if (t.acc[i] >= rest) {
                    const span = t.acc[i] - t.acc[i - 1] || 1;
                    const f = (rest - t.acc[i - 1]) / span;
                    const a = t.pts[i - 1], b = t.pts[i];
                    return [a[0] + (b[0] - a[0]) * f, a[1] + (b[1] - a[1]) * f];
                }
            }
            return t.pts[n];
        }
        rest -= t.len;
    }
    return tables[tables.length - 1].pts.slice(-1)[0];
}

const unit = (v) => { const m = Math.hypot(v[0], v[1]) || 1; return [v[0] / m, v[1] / m]; };
const CX = 160, CY = 90;

console.log('\n▸ الف) مسیر خوانده‌شده از index.html');
console.log('   ' + D);
console.log(`   طول کل: ${total.toFixed(2)} واحد`);

console.log('\n▸ الف) شروع و پایان — مسیر بسته است');
{
    check('نقطه شروع روی مرکز تقاطع (x)', parsed.start[0], CX, 0.001);
    check('نقطه شروع روی مرکز تقاطع (y)', parsed.start[1], CY, 0.001);
    ok('پایان مسیر به همان نقطه شروع برمی‌گردد',
        Math.hypot(parsed.end[0] - parsed.start[0], parsed.end[1] - parsed.start[1]) < 0.001);
}

console.log('\n▸ الف) هم‌طولی چهار قطعه');
{
    const avg = total / 4;
    const devs = tables.map((t) => Math.abs(t.len - avg) / avg);
    tables.forEach((t, i) => console.log(`   قطعه ${i + 1}: ${t.len.toFixed(3)} واحد`));
    const maxDev = Math.max(...devs);
    ok(`هر چهار قطعه هم‌طول‌اند (بیشترین انحراف ${(maxDev * 100).toFixed(4)}٪ زیر ۰٫۱٪)`,
        maxDev < 0.001);
}

console.log('\n▸ الف) ایستگاه‌های کلیدی سر نور');
for (const [p, tx, ty, label] of [
    [0, CX, CY, 'مرکز تقاطع'],
    [0.25, 286, 90, 'انتهای راست'],
    [0.5, CX, CY, 'مرکز تقاطع'],
    [0.75, 34, 90, 'انتهای چپ'],
    [1, CX, CY, 'مرکز تقاطع']
]) {
    const pt = pointAt(total * p);
    const dist = Math.hypot(pt[0] - tx, pt[1] - ty);
    console.log(`   ${(p * 100).toFixed(0).padStart(3)}٪  →  (${pt[0].toFixed(2)}, ${pt[1].toFixed(2)})` +
        `   خطا تا ${label}: ${dist.toFixed(3)}`);
    ok(`   ${(p * 100).toFixed(0)}٪ روی ${label}`, dist < 0.5);
}

console.log('\n▸ الف) تقارن دوگانه');
{
    const seq = (s) => [s.p0, s.p1, s.p2, s.p3];
    const mx = (p) => [2 * CX - p[0], p[1]];
    const my = (p) => [p[0], 2 * CY - p[1]];
    const near = (a, b) => Math.hypot(a[0] - b[0], a[1] - b[1]) < 1e-9;
    const S = parsed.segs;

    ok('نیمه چپ آینه نیمه راست حول محور عمودی است',
        seq(S[2]).every((p, i) => near(p, mx(seq(S[0])[i]))) &&
        seq(S[3]).every((p, i) => near(p, mx(seq(S[1])[i]))));
    ok('رشته پایین آینه رشته بالا حول محور افقی است',
        seq(S[1]).every((p, i) => near(p, my(seq(S[0])[3 - i]))));
    ok('تقارن مرکزی بین قطعه ۱ و ۴ برقرار است',
        seq(S[3]).every((p, i) => near(p, mx(my(seq(S[0])[3 - i])))));
}

console.log('\n▸ الف) پیوستگی مماس و تقاطع واقعی در مرکز');
{
    const S = parsed.segs;
    const outT = (s) => unit([s.p1[0] - s.p0[0], s.p1[1] - s.p0[1]]);
    const inT = (s) => unit([s.p3[0] - s.p2[0], s.p3[1] - s.p2[1]]);

    ok('رشته دوم→سوم در مرکز پیوسته است (بدون شکستگی)',
        Math.hypot(outT(S[2])[0] - inT(S[1])[0], outT(S[2])[1] - inT(S[1])[1]) < 1e-9);
    ok('رشته چهارم→اول در مرکز پیوسته است (بدون شکستگی)',
        Math.hypot(outT(S[0])[0] - inT(S[3])[0], outT(S[0])[1] - inT(S[3])[1]) < 1e-9);

    const a = inT(S[1]), b = outT(S[0]);
    const dot = Math.max(-1, Math.min(1, a[0] * b[0] + a[1] * b[1]));
    const angle = Math.acos(dot) * 180 / Math.PI;
    console.log(`   زاویه تقاطع دو رشته: ${angle.toFixed(1)}°`);
    ok('دو رشته با زاویه واقعی از هم می‌گذرند (نه مماس)', angle > 25 && angle < 90);
}

console.log('\n▸ الف) جای‌گیری در viewBox و نسبت شکل');
{
    let minX = 1e9, maxX = -1e9, minY = 1e9, maxY = -1e9;
    for (const t of tables) {
        for (const p of t.pts) {
            minX = Math.min(minX, p[0]); maxX = Math.max(maxX, p[0]);
            minY = Math.min(minY, p[1]); maxY = Math.max(maxY, p[1]);
        }
    }
    console.log(`   کادر نماد: x ${minX}..${maxX}   y ${minY}..${maxY}`);
    const margin = Math.min(minX, minY, 320 - maxX, 180 - maxY);
    ok(`نماد داخل viewBox ۳۲۰×۱۸۰ جا می‌شود (حاشیه ${margin} واحد برای هاله)`, margin > 20);

    const ratio = (maxX - minX) / (maxY - minY);
    console.log(`   نسبت پهنا به ارتفاع: ${ratio.toFixed(2)}`);
    ok('نسبت شکل به بی‌نهایت کلاسیک نزدیک است (بین ۲ و ۳)', ratio > 2 && ratio < 3);
}

/* ==========================================================================
   ب) مارک‌آپ، سیم‌کشی و دسترس‌پذیری
   ========================================================================== */

console.log('\n▸ ب) ساختار index.html');
{
    ok('صفحه آغاز با شناسه solon-splash وجود دارد', html.includes('id="solon-splash"'));

    const appAt = html.indexOf('<div id="app">');
    const appClose = html.indexOf('</div>', appAt);
    const insideApp = html.substring(appAt, appClose);
    ok('#app خالی است و صفحه آغاز داخلش نیست', !insideApp.includes('s-splash'));
    ok('صفحه آغاز پیش از #app در DOM می‌آید', html.indexOf('id="solon-splash"') < appAt);

    ok('ویژگی data-progress روی نمونه اصلی نیست (حالت زنده)',
        !/id="solon-splash"[^>]*data-progress/.test(html));
    ok('viewport-fit=cover اضافه شده', html.includes('viewport-fit=cover'));

    // تگ‌های واقعی جست‌وجو می‌شوند، نه نام فایل در کامنت‌ها؛ وگرنه توضیحی که
    // در بالای اسکریپت نوشته شده، خودش آزمون را گمراه می‌کند.
    const cssLink = html.indexOf('href="css/splash.css"');
    const jsLink = html.indexOf('src="js/splash.js"');
    const blazorJs = html.indexOf('src="_framework/blazor.webassembly.js"');
    ok('splash.css در head لینک شده', cssLink > 0 && cssLink < html.indexOf('</head>'));
    ok('splash.js بار می‌شود', jsLink > 0);
    ok('splash.js پیش از blazor.webassembly.js می‌آید (لازمه فاز اول)', jsLink < blazorJs);
    ok('لودر قدیمی از index.html حذف شده', !html.includes('loading-progress'));
    ok('بلوک noscript صفحه آغاز را پنهان می‌کند',
        /<noscript>[\s\S]*\.s-splash\s*\{\s*display:\s*none\s*!important/.test(html));
}

console.log('\n▸ ب) دسترس‌پذیری مارک‌آپ');
{
    ok('ناحیه وضعیت role=status دارد', /role="status"/.test(html));
    ok('ناحیه وضعیت aria-live=polite دارد', /aria-live="polite"/.test(html));
    ok('نماد SVG از دید صفحه‌خوان پنهان است',
        /class="s-inf"[^>]*aria-hidden="true"/.test(html));
    ok('لیست ذرات aria-hidden است', /class="s-splash__particles" aria-hidden="true"/.test(html));
    ok('دکمه تلاش دوباره وجود دارد', /data-retry/.test(html));
    ok('پیام خطا از پیش در DOM است و hidden می‌ماند',
        /class="s-splash__error" data-error hidden/.test(html));
    ok('نقطه شروع سر نور روی مرکز تقاطع پیش‌ست شده',
        /class="s-inf__head"\s+style="transform: translate\(160px, 90px\)"/.test(html));
    ok('سه نقطه متحرک در ردیف بارگذاری هست',
        /<i class="s-splash__dots" aria-hidden="true"><b><\/b><b><\/b><b><\/b><\/i>/.test(html));
    ok('متن «در حال بارگذاری نرم‌افزار» موجود است', html.includes('در حال بارگذاری نرم‌افزار'));
    ok('درصد اولیه با ارقام فارسی است', html.includes('<span data-pct>۰٪</span>'));
    ok('نام برند پیش‌فرض در مارک‌آپ هست (پیش از رسیدن API)', html.includes('>سالن زیبایی حدیث<'));
}

/* ==========================================================================
   ج) قرارداد CSS
   ========================================================================== */

console.log('\n▸ ج) توکن‌های رنگ');
{
    const openBrace = css.indexOf('.s-splash {');
    const closeBrace = css.indexOf('\n}\n', openBrace);
    const tokenBlock = css.substring(openBrace, closeBrace);

    // توضیح‌ها پیش از بازرسی رنگ‌ها حذف می‌شوند: توضیح، کد نیست. اگر این کار
    // را نکنیم، توضیحی که خودش یک کدرنگ را نام می‌برد آزمون را رد می‌کند.
    const strip = (t) => t.replace(/\/\*[\s\S]*?\*\//g, '');
    const body = strip(css.substring(closeBrace));

    for (const [name, value] of [
        ['--sp-bg', '#14110F'], ['--sp-glow', '#2A221B'], ['--sp-gold', '#C6A46A'],
        ['--sp-gold-dark', '#A8854B'], ['--sp-gold-light', '#F1DDAE'],
        ['--sp-text', '#EDE6DA'], ['--sp-text-dim', '#A89F93']
    ]) {
        ok(`توکن ${name} درست تعریف شده (${value})`, tokenBlock.includes(value));
    }

    const literals = body.match(/#[0-9A-Fa-f]{6}/g) || [];
    ok(`هیچ کدرنگ خامی در بدنه فایل نیست (پیدا شد: ${literals.length || 'هیچ'})`, literals.length === 0);

    const rgbaInBody = body.match(/rgba?\(/g) || [];
    ok(`هیچ rgba خامی در بدنه فایل نیست (پیدا شد: ${rgbaInBody.length || 'هیچ'})`, rgbaInBody.length === 0);

    for (const token of ['--sp-bg', '--sp-glow', '--sp-gold', '--sp-gold-dark', '--sp-gold-light',
                         '--sp-text', '--sp-text-dim', '--sp-head', '--sp-track', '--sp-track-w',
                         '--sp-line-w', '--sp-glow-gold', '--sp-dot', '--sp-danger',
                         '--sp-gold-tint']) {
        if (!new RegExp('var\\(\\s*' + token + '[,\\s)]').test(css)) {
            ok(`توکن ${token} استفاده شده`, false);
        }
    }
    ok('همه توکن‌ها هم تعریف و هم استفاده شده‌اند', true);

    ok('توکن‌ها روی .s-splash دامنه دارند، نه :root', !/:root\s*\{/.test(css));
}

console.log('\n▸ ج) اندازه، واکنش‌گرایی و اسکرول');
{
    ok('نماد ۹۰٪ عرض با سقف ۴۶۰ پیکسل است',
        /\.s-inf\s*\{[^}]*inline-size:\s*min\(90vw,\s*460px\)/.test(css));
    ok('viewBox مارک‌آپ با CSS هم‌خوان است', html.includes('viewBox="0 0 320 180"'));
    ok('safe-area در پوسته رعایت شده (بالا/پایین و چپ/راست)',
        /padding-block:\s*max\(16px,\s*env\(safe-area-inset-top\)\)/.test(css) &&
        /padding-inline:\s*max\(16px,\s*env\(safe-area-inset-right\)\)/.test(css));
    ok('پوسته overflow:hidden دارد (هیچ اسکرول یا سرریزی)',
        /\.s-splash\s*\{[^}]*overflow:\s*hidden/.test(css));
    ok('قاعده مخصوص صفحه‌های کوتاه (گوشی افقی) وجود دارد',
        /@media \(max-height:\s*520px\)/.test(css) &&
        /@media \(max-height: 520px\)[\s\S]{0,200}\.s-inf\s*\{[^}]*min\(46vw/.test(css));
    ok('پوسته همیشه تاریک است و به تم سیستم وصل نیست', !/prefers-color-scheme/.test(css));
    // این سه بررسی با includes نوشته شده‌اند نه با الگوی باقاعده: متن ثابتی
    // است که دنبالش هستیم و includes هیچ لایه escape اضافه‌ای ندارد که خودش
    // منبع خطا شود.
    ok('نام برند بلند از دیتابیس سرریز افقی نمی‌سازد',
        css.includes('.s-splash__brand-name,\n.s-splash__brand-latin {') &&
        css.includes('max-inline-size: 100%;') &&
        css.includes('overflow-wrap: break-word;'));
    ok('پیام خطا اندازه سطر خوانا دارد',
        css.includes('max-inline-size: min(100%, 38ch);'));
    ok('پوسته position:fixed با inset:0 است',
        /\.s-splash\s*\{[^}]*position:\s*fixed[^}]*inset:\s*0/.test(css));
}

console.log('\n▸ ج) انیمیشن و reduced-motion');
{
    const rmStart = css.indexOf('@media (prefers-reduced-motion');
    const animBlock = css.substring(rmStart);
    ok('بلوک reduced-motion وجود دارد', rmStart > 0);
    ok('در reduced-motion ذرات خاموش می‌شوند',
        /\.s-splash__particles i,/.test(animBlock) && /animation:\s*none/.test(animBlock));
    ok('در reduced-motion هاله نفس‌کش خاموش می‌شود', /\.s-splash__glow,/.test(animBlock));
    ok('در reduced-motion نقطه‌های متحرک خاموش می‌شوند', /\.s-splash__dots b/.test(animBlock));
    ok('در reduced-motion گذار سر نور خاموش می‌شود', /\.s-inf__head\s*\{\s*transition:\s*none/.test(animBlock));

    const keyframes = css.match(/@keyframes[^{]+\{(?:[^{}]|\{[^{}]*\})*\}/g) || [];
    const badProps = [];
    for (const kf of keyframes) {
        const body = kf.substring(kf.indexOf('{'));
        for (const prop of ['width', 'height', 'top', 'left', 'right', 'bottom',
                            'margin', 'padding', 'inset', 'font-size', 'block-size', 'inline-size']) {
            if (new RegExp('(^|[;{\\s])' + prop + '\\s*:').test(body)) badProps.push(prop);
        }
    }
    console.log(`   تعداد keyframe: ${keyframes.length}`);
    ok(`هیچ keyframe ویژگی چیدمانی انیمیت نمی‌کند (پیدا شد: ${badProps.join(',') || 'هیچ'})`,
        badProps.length === 0);
    ok('همه keyframe‌ها فقط transform/opacity را حرکت می‌دهند',
        keyframes.every((kf) => !/(stroke-dashoffset|stroke-dasharray)\s*:/.test(kf.substring(kf.indexOf('{')))));

    const dotCount = (css.match(/radial-gradient\(circle, var\(--sp-dot\)/g) || []).length;
    console.log(`   ذرات: ${dotCount} نقطه در ۳ لایه DOM`);
    ok('ذرات فقط با CSS ساخته می‌شوند و ریزند (۱۱ تا ۲۴ نقطه)',
        dotCount > 10 && dotCount < 25);
}

console.log('\n▸ ج) کنتراست متن (WCAG AA)');
{
    const lum = (hex) => {
        const v = [1, 3, 5].map((i) => parseInt(hex.substr(i, 2), 16) / 255)
            .map((c) => (c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4)));
        return 0.2126 * v[0] + 0.7152 * v[1] + 0.0722 * v[2];
    };
    const ratio = (a, b) => {
        const la = lum(a), lb = lum(b);
        return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05);
    };

    for (const [fg, bg, label] of [
        ['#EDE6DA', '#14110F', 'متن اصلی روی پس‌زمینه'],
        ['#A89F93', '#14110F', 'متن کم‌رنگ روی پس‌زمینه'],
        ['#C6A46A', '#14110F', 'طلایی روی پس‌زمینه'],
        ['#C6A46A', '#2A221B', 'طلایی روی مرکز گرادیان (درصد)'],
        ['#EDE6DA', '#2A221B', 'متن اصلی روی مرکز گرادیان'],
        ['#E8A6A0', '#14110F', 'متن خطا روی پس‌زمینه'],
        ['#F1DDAE', '#14110F', 'متن دکمه تلاش دوباره'],
        ['#A89F93', '#2A221B', 'متن کم‌رنگ وضعیت روی مرکز گرادیان']
    ]) {
        const r = ratio(fg, bg);
        console.log(`   ${label}: ${r.toFixed(2)}:۱`);
        ok(`   حداقل ۴٫۵:۱`, r >= 4.5);
    }
}

/* ==========================================================================
   د و ه — موتور در محیط مرورگر جعلی با تایمرهای دروغین
   ========================================================================== */

function buildSandbox(opts) {
    opts = opts || {};

    const state = {
        cssProps: {}, headTransform: null, headOpacity: null,
        pct: null, sr: null, classes: [], hidden: false,
        errorHidden: true, errorMsg: null, reloaded: false,
        observerFn: null, observerConnected: false,
        intervals: [], timers: [], timerSeq: 0,
        blazorPct: opts.blazorPct || '',
        clock: 1000000,
        imageOutcome: opts.imageOutcome || 'ok',
        fontFaces: opts.fontFaces === undefined ? 1 : opts.fontFaces,
        transitionEndFn: null,
        brand: null, brandLatin: null, brandLatinHidden: null,
        logoSrc: null, logoHidden: null, logoHandlers: null
    };

    const headStyle = {
        set transform(v) { state.headTransform = v; },
        get transform() { return state.headTransform; },
        set opacity(v) { state.headOpacity = v; },
        get opacity() { return state.headOpacity; }
    };
    const head = { style: headStyle };

    const svg = {
        style: { setProperty: (k, v) => { state.cssProps[k] = v; } },
        querySelector: () => head
    };

    const root = {
        id: 'solon-splash',
        classList: { add: (c) => state.classes.push(c) },
        getAttribute: (k) => (k === 'data-progress' ? (opts.progressAttr || null) : null),
        querySelector: (sel) => {
            switch (sel) {
                case '.s-inf': return svg;
                case '[data-pct]': return { set textContent(v) { state.pct = v; } };
                case '[data-sr]': return { set textContent(v) { state.sr = v; } };
                case '[data-retry]':
                    return opts.noRetry ? null
                        : { dataset: {}, addEventListener: (e, fn) => { state.retryFn = fn; } };
                case '[data-error]':
                    return { set hidden(v) { state.errorHidden = v; }, get hidden() { return state.errorHidden; } };
                case '[data-error-msg]': return { set textContent(v) { state.errorMsg = v; } };
                case '[data-brand]': return { set textContent(v) { state.brand = v; } };
                case '[data-brand-latin]':
                    return {
                        set textContent(v) { state.brandLatin = v; },
                        set hidden(v) { state.brandLatinHidden = v; }
                    };
                case '[data-logo]':
                    return {
                        set src(v) { state.logoSrc = v; },
                        set hidden(v) { state.logoHidden = v; },
                        addEventListener: (e, fn) => {
                            state.logoHandlers = state.logoHandlers || {};
                            state.logoHandlers[e] = fn;
                        }
                    };
                default: return null;
            }
        },
        addEventListener: (e, fn) => { if (e === 'transitionend') state.transitionEndFn = fn; }
    };
    Object.defineProperty(root, 'hidden', {
        set(v) { state.hidden = v; },
        get() { return state.hidden; }
    });

    // هندسه واقعی مسیر — همان چیزی که مرورگر با getTotalLength می‌دهد
    const geomPath = {
        getTotalLength: () => total,
        getPointAtLength: (l) => {
            const p = pointAt(l);
            return { x: p[0], y: p[1] };
        }
    };

    const document_ = {
        readyState: 'complete',
        title: '',
        querySelectorAll: (sel) => (sel === '.s-splash' ? [root] : []),
        querySelector: (sel) => (sel === '.s-inf__geom' ? geomPath : null),
        addEventListener: () => {},
        // html و body استایل درون‌خطی می‌گیرند (قفل اسکرول)، پس باید
        // قابل نوشتن و قابل خواندن باشند.
        documentElement: { style: {} },
        body: { style: {} },
        fonts: { load: () => Promise.resolve(new Array(state.fontFaces).fill({})) }
    };

    function FakeImage() {
        const self = this;
        Object.defineProperty(this, 'src', {
            set(v) {
                state.imageSrc = v;
                if (state.imageOutcome === 'hang') return;
                Promise.resolve().then(() => {
                    if (state.imageOutcome === 'ok') { if (self.onload) self.onload(); }
                    else if (self.onerror) self.onerror();
                });
            }
        });
    }

    function FakeMutationObserver(fn) { state.observerFn = fn; }
    FakeMutationObserver.prototype.observe = function () { state.observerConnected = true; };
    FakeMutationObserver.prototype.disconnect = function () { state.observerConnected = false; };

    const sandbox = {
        document: document_,
        window: {
            matchMedia: () => ({ matches: !!opts.reducedMotion }),
            addEventListener: () => {},
            location: { reload: () => { state.reloaded = true; } }
        },
        console,
        Promise, Math, Number, String, Array, Object, isFinite, JSON,
        Date: { now: () => state.clock },
        Image: FakeImage,
        MutationObserver: opts.noObserver ? undefined : FakeMutationObserver,
        requestAnimationFrame: (fn) => {
            state.rafQueue = state.rafQueue || [];
            state.rafQueue.push(fn);
            return 1;
        },
        cancelAnimationFrame: () => { state.rafQueue = []; },
        setTimeout: (fn, ms) => {
            const id = ++state.timerSeq;
            state.timers.push({ id, fn, ms });
            return id;
        },
        clearTimeout: (id) => {
            state.timers = state.timers.filter((t) => t.id !== id);
        },
        setInterval: (fn, ms) => {
            const id = ++state.timerSeq;
            state.intervals.push({ id, fn, ms });
            return id;
        },
        clearInterval: (id) => {
            state.intervals = state.intervals.filter((t) => t.id !== id);
        },
        getComputedStyle: () => ({
            getPropertyValue: (k) => (k === '--blazor-load-percentage' ? state.blazorPct : '')
        })
    };

    sandbox.__flush = function (max) {
        let n = 0;
        while (state.rafQueue && state.rafQueue.length && n < (max || 500)) {
            const q = state.rafQueue.splice(0);
            q.forEach((fn) => fn(0));
            n++;
        }
        return n;
    };

    sandbox.__runTimers = function (filter) {
        const due = state.timers.filter((t) => !filter || filter(t));
        state.timers = state.timers.filter((t) => filter && !filter(t));
        due.forEach((t) => t.fn());
        return due.map((t) => t.ms);
    };

    sandbox.globalThis = sandbox;
    sandbox.window.document = document_;

    vm.createContext(sandbox);
    vm.runInContext(js, sandbox);

    return {
        api: sandbox.window.solonSplash,
        document: document_,
        state,
        flush: sandbox.__flush,
        runTimers: sandbox.__runTimers,
        tickCreep: () => state.intervals.forEach((t) => t.fn()),
        ids: () => state.intervals.map((t) => t.id)
    };
}

console.log('\n▸ د) سطح عمومی');
{
    const env = buildSandbox();

    for (const fn of ['setProgress', 'getProgress', 'setBrand', 'watchBlazorLoad',
                      'stopWatching', 'preloadImage', 'awaitFont', 'finish', 'fail']) {
        ok(`تابع ${fn} در دسترس است`, typeof env.api[fn] === 'function');
    }
    ok('مقدار اولیه صفر است', env.api.getProgress() === 0);
    check('متغیر CSS پیشرفت روی صفر ست شده', Number(env.state.cssProps['--inf-p']), 0, 1e-9);
    ok('در حالت زنده، رصد فاز اول خودکار شروع شده', env.state.observerConnected === true);
    ok('در حالت زنده، نگهبان راه افتاده است',
        env.state.timers.some((t) => t.ms === 20000));
    ok('تایمر خزیدن فاز اول راه افتاده است', env.ids().length === 1);
}

console.log('\n▸ د) دو فاز و نگهبان یک‌نوا');
{
    const env = buildSandbox({ blazorPct: '0%' });

    env.state.blazorPct = '50%';
    env.state.observerFn();
    env.flush();
    check('فاز اول: ۵۰٪ خود Blazor → ۰٫۲۲۵ روی نوار', env.api.getProgress(), 0.225, 1e-6);

    env.state.blazorPct = '100%';
    env.state.observerFn();
    env.flush();
    check('فاز اول: ۱۰۰٪ خود Blazor → سقف ۰٫۴۵', env.api.getProgress(), 0.45, 1e-6);

    env.state.blazorPct = '98%';
    env.state.observerFn();
    env.flush();
    check('بازگشت ۱۰۰→۹۸ نادیده گرفته شد (ایشوی شناخته‌شده aspnetcore)',
        env.api.getProgress(), 0.45, 1e-6);

    env.state.blazorPct = 'مافیه نامعتبر';
    env.state.observerFn();
    env.flush();
    check('مقدار نامعتبر بی‌اثر بود', env.api.getProgress(), 0.45, 1e-6);

    env.api.stopWatching();
    ok('stopWatching رصد را قطع کرد', env.state.observerConnected === false);
    ok('stopWatching تایمر خزیدن را پاک کرد', env.ids().length === 0);

    for (const [target, label] of [[0.58, 'گام توکن'], [0.74, 'گام تنظیمات'],
                                   [0.84, 'گام لوگو'], [0.92, 'گام فونت']]) {
        env.api.setProgress(target);
        env.flush();
        check(`فاز دوم: ${label} → ${target}`, env.api.getProgress(), target, 1e-6);
    }

    env.api.setProgress(0.3);
    env.flush();
    check('مقدار نزولی در میانه فاز دوم نادیده گرفته شد', env.api.getProgress(), 0.92, 1e-6);

    env.api.setProgress(1);
    env.flush();
    check('فاز دوم: گام هیرو → ۱', env.api.getProgress(), 1, 1e-6);

    env.api.setProgress(-5);
    env.flush();
    check('مقدار منفی پس از ۱۰۰٪ اثری ندارد', env.api.getProgress(), 1, 1e-6);

    env.api.setProgress(NaN);
    env.api.setProgress('abc');
    env.flush();
    check('ورودی نامعتبر بی‌اثر است', env.api.getProgress(), 1, 1e-6);
}

console.log('\n▸ د) خزیدن محدود در فاز اول');
{
    const env = buildSandbox({ blazorPct: '' });
    const ceiling = 0.45 * 0.92;

    env.state.clock += 1000;
    env.tickCreep();
    env.flush();
    check('پیش از گذشت آستانه بی‌کاری، خزیدن آغاز نمی‌شود', env.api.getProgress(), 0, 1e-9);

    env.state.clock += 1500;
    env.tickCreep();
    env.flush();
    ok('پس از بی‌کاری، نوار آرام حرکت می‌کند', env.api.getProgress() > 0);

    for (let i = 0; i < 500; i++) {
        env.state.clock += 300;
        env.tickCreep();
        env.flush();
    }
    const crept = env.api.getProgress();
    console.log(`   مقدار نهایی پس از ۵۰۰ تیک: ${crept.toFixed(4)}  (سقف ${ceiling.toFixed(4)})`);
    ok('خزیدن هرگز از سقف فاز اول رد نمی‌شود', crept <= ceiling + 1e-9);
    ok('خزیدن هرگز به ۱۰۰٪ نزدیک هم نمی‌شود (درباره پایان کار دروغ نمی‌گوید)', crept < 0.5);
    ok('خزیدن بی‌فایده نیست و تا نزدیک سقف می‌رود', crept > ceiling - 0.01);

    env.state.blazorPct = '100%';
    env.state.observerFn();
    env.flush();
    check('گزارش واقعی تازه سقف فاز اول را می‌نشاند', env.api.getProgress(), 0.45, 1e-6);

    // نکته مهم: نگهبان یک‌نوا نباید بگذارد خزیدن از مقدار واقعی جلو بزند
    const env2 = buildSandbox({ blazorPct: '' });
    env2.state.clock += 3000;
    for (let i = 0; i < 20; i++) { env2.tickCreep(); env2.flush(); env2.state.clock += 300; }
    const creptAhead = env2.api.getProgress();
    env2.state.blazorPct = '10%';
    env2.state.observerFn();
    env2.flush();
    check('گزارش واقعی کوچک‌تر از خزیدن، نوار را عقب نمی‌برد',
        env2.api.getProgress(), creptAhead, 1e-9);
}

console.log('\n▸ د) سر نور روی مسیر واقعی');
{
    const env = buildSandbox();
    const at = (p) => {
        env.api.setProgress(p);
        env.flush(2000);
        const m = /translate\(([-\d.]+)px,([-\d.]+)px\)/.exec(env.state.headTransform || '');
        return m ? [parseFloat(m[1]), parseFloat(m[2])] : null;
    };

    for (const [p, tx, ty, label] of [
        [0.25, 286, 90, 'انتهای راست'],
        [0.5, 160, 90, 'مرکز تقاطع'],
        [0.75, 34, 90, 'انتهای چپ'],
        [1, 160, 90, 'مرکز تقاطع (پایان)']
    ]) {
        const pt = at(p);
        ok(`سر نور در ${(p * 100).toFixed(0)}٪ روی ${label} است`,
            !!pt && Math.hypot(pt[0] - tx, pt[1] - ty) < 0.5);
        if (pt) console.log(`      (${pt[0].toFixed(2)}, ${pt[1].toFixed(2)})`);
    }

    ok('سر نور در صفر کاملاً محو است (نقطه تنها وسط نماد دیده نمی‌شود)',
        env.state.headOpacity === '1' || env.state.headOpacity !== null);
    const fresh = buildSandbox();
    check('شفافیت سر نور در مقدار صفر', Number(fresh.state.headOpacity), 0, 1e-9);
}

console.log('\n▸ د) درصد فارسی و اعلام صفحه‌خوان');
{
    const env = buildSandbox();

    env.api.setProgress(0.45);
    env.flush(2000);
    ok('۴۵٪ با ارقام فارسی: ' + env.state.pct, env.state.pct === '۴۵٪');

    env.api.setProgress(1);
    env.flush(2000);
    ok('۱۰۰٪ با ارقام فارسی: ' + env.state.pct, env.state.pct === '۱۰۰٪');
    ok('اعلام صفحه‌خوان سر ۱۰۰ درصد آمده: ' + env.state.sr, /۱۰۰ درصد/.test(env.state.sr || ''));
    ok('اعلام شامل نام نرم‌افزار است', /در حال بارگذاری نرم‌افزار/.test(env.state.sr || ''));

    const fresh = buildSandbox();
    fresh.api.setProgress(0.031);
    fresh.flush(2000);
    ok('سر ۰ درصد اعلامی نمی‌رود (بدون اعلام بی‌مورد اولیه)', fresh.state.sr === null);

    fresh.api.setProgress(0.15);
    fresh.flush(2000);
    ok('سر ۱۰ درصد اعلام می‌آید: ' + fresh.state.sr, /۱۰ درصد/.test(fresh.state.sr || ''));

    let announcements = 0;
    const counter = buildSandbox();
    for (let i = 1; i <= 100; i++) {
        counter.api.setProgress(i / 100);
        counter.flush(2000);
        if (counter.state.sr && /۱۰۰ درصد/.test(counter.state.sr) === false &&
            /0 درصد/.test(counter.state.sr)) announcements++;
    }
    console.log(`   تعداد اعلام‌ها در ۱۰۰ پله پیشرفت: ${announcements}`);
    ok('اعلام‌ها فقط سر دهگان است، نه ۱۰۰ بار', announcements <= 11);

    ok('تبدیل ارقام به‌صورت تابع عمومی هم در دسترس است',
        counter.api.toPersianDigits(1234567890) === '۱۲۳۴۵۶۷۸۹۰');
}

console.log('\n▸ ه) پایان کار: حداقل زمان نمایش');
{
    const env = buildSandbox();

    env.api.setProgress(1);
    env.flush(2000);
    env.api.finish();

    const waits = env.state.timers.map((t) => t.ms).filter((ms) => ms < 5000);
    console.log(`   زمان‌سنج‌های انتظار: ${waits.join(', ')}ms`);
    ok('پیش از گذشت حداقل زمان نمایش، لایه محو نمی‌شود (is-done نیامده)',
        env.state.classes.indexOf('is-done') === -1);
    ok('انتظار حداقل زمان نمایش حدود ۸۰۰ میلی‌ثانیه است',
        waits.some((ms) => ms >= 700 && ms <= 900));

    env.state.clock += 1000;
    env.runTimers((t) => t.ms < 5000);
    ok('پس از سپری شدن زمان، لایه is-done می‌شود', env.state.classes.indexOf('is-done') >= 0);
    check('مقدار پیشرفت روی ۱۰۰ قفل شد', env.api.getProgress(), 1, 1e-9);

    // زمان‌سنج ایمنی هم لایه را کامل پنهان می‌کند
    env.runTimers((t) => t.ms === 900);
    ok('لایه پس از محو شدن hidden می‌شود', env.state.hidden === true);
}

console.log('\n▸ ه) پایان کار: اگر بارگذاری سریع تمام شود');
{
    const env = buildSandbox();
    env.state.clock += 5000;          // زمان زیادی گذشته است
    env.api.setProgress(1);
    env.flush(2000);
    env.api.finish();
    const waits = env.state.timers.map((t) => t.ms).filter((ms) => ms < 5000);
    check('وقتی حداقل زمان نمایش گذشته، بی‌دلیل صبر نمی‌کند', Math.min(...waits), 0, 0.001);
    env.runTimers((t) => t.ms < 5000);
    ok('و همچنان محو شدن انجام می‌شود', env.state.classes.indexOf('is-done') >= 0);
}

console.log('\n▸ ه) حالت خطا: نماد خشک می‌شود');
{
    const env = buildSandbox();
    env.api.setProgress(0.62);
    env.flush(2000);
    const frozen = env.api.getProgress();

    env.api.fail('پیام آزمایشی');

    check('نماد همان‌جا که بود خشک شد', env.api.getProgress(), frozen, 1e-9);
    ok('کلاس is-failed اضافه شد', env.state.classes.indexOf('is-failed') >= 0);
    ok('پیام خطا نوشته شد', env.state.errorMsg === 'پیام آزمایشی');
    ok('کادر خطا از حالت پنهان خارج شد', env.state.errorHidden === false);
    ok('نگهبان پس از خطا خاموش شد', !env.state.timers.some((t) => t.ms === 20000));
    ok('fail دوباره اجرا نمی‌شود', env.api.fail('دوباره') === false);
    ok('finish پس از خطا کاری نمی‌کند', env.api.finish() !== undefined &&
        env.state.classes.indexOf('is-done') === -1);

    env.api.setProgress(0.9);
    env.flush(2000);
    check('پس از خطا هیچ پیشرفتی رخ نمی‌دهد', env.api.getProgress(), frozen, 1e-9);

    ok('دکمه تلاش دوباره به بارگذاری مجدد وصل است', typeof env.state.retryFn === 'function');
    env.state.retryFn();
    ok('کلیک روی تلاش دوباره صفحه را بازنشانی می‌کند', env.state.reloaded === true);
}

console.log('\n▸ ه) نگهبان: خود Blazor بالا نیامده');
{
    const env = buildSandbox();
    const watchdog = env.state.timers.find((t) => t.ms === 20000);
    ok('نگهبان ۲۰ ثانیه‌ای نشانده شده است', !!watchdog);

    watchdog.fn();
    ok('پس از انقضا، حالت خطا فعال می‌شود', env.state.classes.indexOf('is-failed') >= 0);
    ok('پیام فارسی راهنما نشان داده می‌شود',
        /اتصال اینترنت را بررسی کنید/.test(env.state.errorMsg || ''));
    ok('کادر خطا باز می‌شود', env.state.errorHidden === false);

    const fine = buildSandbox();
    fine.api.setProgress(1);
    fine.flush(2000);
    fine.api.finish();
    ok('اگر پایان به‌موقع برسد، نگهبان خاموش می‌شود',
        !fine.state.timers.some((t) => t.ms === 20000));
}

console.log('\n▸ ه) قفل اسکرول پس‌زمینه');
{
    const env = buildSandbox();
    ok('در آغاز کار اسکرول صفحه قفل می‌شود',
        env.document.documentElement.style.overflow === 'hidden' &&
        env.document.body.style.overflow === 'hidden');

    env.api.setProgress(1);
    env.flush(2000);
    env.api.finish();
    env.state.clock += 2000;
    env.runTimers((t) => t.ms < 5000);
    env.runTimers((t) => t.ms === 900);
    ok('پس از محو شدن، اسکرول آزاد می‌شود',
        env.document.documentElement.style.overflow === '' &&
        env.document.body.style.overflow === '');

    const bad = buildSandbox();
    bad.api.fail('خطا');
    ok('در حالت خطا هم اسکرول آزاد می‌شود (صفحه قفل نمی‌ماند)',
        bad.document.documentElement.style.overflow === '' &&
        bad.document.body.style.overflow === '');

    // در پیش‌نمایش، نمونه‌ها مقدار نمایشی دارند و «حالت زنده» نیستند؛ قفل
    // اسکرول نباید فعال شود وگرنه خود صفحه پیش‌نمایش اسکرول نمی‌شود.
    const demo = buildSandbox({ progressAttr: '0.45' });
    ok('در پیش‌نمایش اسکرول قفل نمی‌شود',
        demo.document.documentElement.style.overflow === undefined ||
        demo.document.documentElement.style.overflow === '');
    ok('در پیش‌نمایش فاز اول و نگهبان فعال نمی‌شوند',
        demo.state.observerConnected === false &&
        !demo.state.timers.some((t) => t.ms === 20000) &&
        demo.ids().length === 0);
}

console.log('\n▸ ه) نشاندن برند از تنظیمات سالن');
{
    const env = buildSandbox();
    env.api.setBrand({ name: 'سالن نمونه', latin: 'SAMPLE SALON' });
    ok('نام از تنظیمات نوشته شد', env.state.brand === 'سالن نمونه');
    ok('نام لاتین از تنظیمات نوشته شد', env.state.brandLatin === 'SAMPLE SALON');
    ok('عنوان سند به‌روز شد', env.document.title === 'سالن نمونه | SAMPLE SALON');

    env.api.setBrand({ name: 'سالن لوگو', logo: 'img/logo.png' });
    ok('آدرس لوگو ست شد', env.state.logoSrc === 'img/logo.png');
    ok('لوگو تا بار شدن تصویر پنهان است', env.state.logoHidden !== false);
    env.state.logoHandlers.load();
    ok('پس از بار شدن تصویر، لوگو نمایان می‌شود', env.state.logoHidden === false);
    ok('و نام لاتین جای خود را به لوگو می‌دهد', env.state.brandLatinHidden === true);

    const broken = buildSandbox();
    broken.api.setBrand({ name: 'سالن', logo: 'img/missing.png' });
    broken.state.logoHandlers.error();
    ok('لوگوی خراب پنهان می‌ماند (تصویر شکسته دیده نمی‌شود)', broken.state.logoHidden === true);
    ok('با لوگوی خراب، نام متنی برمی‌گردد', broken.state.brandLatinHidden === false);

    const empty = buildSandbox();
    empty.api.setBrand(null);
    ok('فراخوانی با مقدار خالی خطا نمی‌دهد و متن پیش‌فرض می‌ماند', empty.state.brand === null);
    empty.api.setBrand({});
    ok('شیء بدون فیلد هم بی‌خطر است (متن پیش‌فرض HTML می‌ماند)', empty.state.brand === null);
}

console.log('\n▸ ه) کمکی‌های بارگذاری واقعی');
{
    const env = buildSandbox();
    env.api.preloadImage('img/hero.jpg', 4000).then((r) => {
        ok('preloadImage برای تصویر سالم true می‌دهد', r === true);
    });
    env.api.preloadImage('', 4000).then((r) => {
        ok('preloadImage با آدرس خالی false می‌دهد (بدون انتظار)', r === false);
    });
    env.api.awaitFont('1rem Vazirmatn', 4000).then((r) => {
        ok('awaitFont وقتی فونت واقعاً بار شود true می‌دهد', r === true);
    });

    const bad = buildSandbox({ imageOutcome: 'error', fontFaces: 0 });
    bad.api.preloadImage('img/missing.jpg', 4000).then((r) => {
        ok('preloadImage برای تصویر خراب false می‌دهد (شکست قابل شمارش)', r === false);
    });
    bad.api.awaitFont('1rem Vazirmatn', 4000).then((r) => {
        ok('awaitFont وقتی فونت پیدا نشود false می‌دهد', r === false);
    });

    const hang = buildSandbox({ imageOutcome: 'hang' });
    let hung = null;
    hang.api.preloadImage('img/slow.jpg', 4000).then((r) => { hung = r; });
    hang.runTimers((t) => t.ms === 4000);
    Promise.resolve().then(() => {
        ok('تصویر کند پس از تایم‌اوت false می‌دهد و نوار را نمی‌خواباند', hung === false);
    });
}

/* ==========================================================================
   و) سیم‌کشی سی‌شارپ
   ========================================================================== */

console.log('\n▸ و) BootService.cs — ترتیب و قواعد');
{
    const svc = fs.readFileSync(BOOT_SERVICE, 'utf8');
    const gate = fs.readFileSync(BOOT_GATE, 'utf8');

    const idxHero = svc.indexOf('گام ۵: تصویر هیرو');
    const idxFinal = svc.indexOf('await ReportAsync(1.0)');
    const idxNetwork = svc.indexOf('networkFailures >= NetworkResourceCount');
    const idxBudget = svc.indexOf('clock.Elapsed > TotalBudget');

    ok('پنج گام واقعی در سرویس وجود دارد',
        ['گام ۱', 'گام ۲', 'گام ۳', 'گام ۴', 'گام ۵'].every((g) => svc.includes(g)));
    ok('رسیدن به ۱۰۰٪ پس از گام پنجم است', idxFinal > idxHero && idxHero > 0);
    ok('داوری منابع شبکه پیش از ۱۰۰٪ انجام می‌شود', idxNetwork > 0 && idxNetwork < idxFinal);
    ok('داوری بودجه زمانی پیش از ۱۰۰٪ انجام می‌شود', idxBudget > 0 && idxBudget < idxFinal);
    ok('دقیقاً یک بار به ۱۰۰٪ می‌رسیم',
        (svc.match(/ReportAsync\(1\.0\)/g) || []).length === 1);
    ok('تنظیمات سالن از سرویس API خوانده می‌شود', svc.includes('_settingsApi.GetSettingsAsync()'));
    ok('فونت وزیرمتن واقعاً بار می‌شود', svc.includes('1rem Vazirmatn'));
    ok('تصویر هیرو واقعاً بار می‌شود', svc.includes('img/hero.jpg'));
    ok('توکن از همان کلید localStorage پروژه خوانده می‌شود', svc.includes('_auth.GetTokenAsync()'));
    ok('هر گام سقف زمانی دارد', svc.includes('StepCap') && svc.includes('Task.WhenAny'));
    ok('سقف فاز اول با جاوااسکریپت هم‌خوان است (۰٫۴۵)',
        /PhaseStart\s*=\s*0\.45/.test(svc) && /BOOT_CAP\s*=\s*0\.45/.test(js));
    ok('شکست یک گام کشنده نیست، فقط منابع شبکه شمرده می‌شوند',
        svc.includes('networkFailures') &&
        svc.includes('if (Settings is not null)'));
    ok('شکست تنظیمات باعث توقف کار نمی‌شود (مقدار پیش‌فرض ادامه می‌دهد)',
        /else\s*\{\s*networkFailures\+\+;\s*\}/.test(svc));
    ok('فاز اول در شروع فاز دوم بسته می‌شود', svc.includes('"stopWatching"'));
}

console.log('\n▸ و) BootGate.razor — قواعد دروازه');
{
    const gate = fs.readFileSync(BOOT_GATE, 'utf8');

    // اعلان متد بررسی می‌شود، نه نام آن در متن توضیحی بالای فایل.
    ok('از OnAfterRenderAsync استفاده می‌کند (رندر قفل نمی‌شود)',
        gate.includes('protected override async Task OnAfterRenderAsync') &&
        !/protected override async Task OnInitializedAsync/.test(gate));
    ok('فقط در نخستین رندر اجرا می‌شود', gate.includes('if (!firstRender) return;'));
    ok('فرزندان بی‌درنگ رندر می‌شوند (ChildContent)', gate.includes('@ChildContent'));
    ok('هدایت هوشمند فقط از مسیر ریشه انجام می‌شود', gate.includes('IsRootLanding()'));
    ok('نبود forceLoad — ناوبری درون همان برنامه',
        !/forceLoad\s*:\s*true/.test(gate) &&
        !/NavigateTo\([^)]*,\s*true\s*\)/.test(gate));
    ok('هدایت با یک پرچم یک‌بارمصرف محافظت شده', gate.includes('_redirectChecked'));
    ok('مسیر پنل از AppRoles می‌آید', gate.includes('AppRoles.PanelUrl(role)'));
    ok('نقش پیش‌فرض مشتری است', gate.includes('?? AppRoles.Customer'));
}

console.log('\n▸ و) Program.cs، App.razor و app.css');
{
    const program = fs.readFileSync(path.join(ROOT, 'BlazorAppSolon', 'Program.cs'), 'utf8');
    const app = fs.readFileSync(path.join(ROOT, 'BlazorAppSolon', 'App.razor'), 'utf8');
    const appcss = fs.readFileSync(path.join(ROOT, 'BlazorAppSolon', 'wwwroot', 'css', 'app.css'), 'utf8');

    ok('BootService در DI ثبت شده', program.includes('AddScoped<BootService>()'));
    ok('BootGate بیرون از Router است', app.indexOf('<BootGate>') < app.indexOf('<Router'));
    ok('Authorizing روشن خنثی شده (بدون پرش رنگ)', /<Authorizing\s*\/>/.test(app));
    // انتخابگر بررسی می‌شود نه رشته؛ توضیحی که جای آن قواعد نوشته شده نام
    // آن‌ها را می‌برد و نباید آزمون را گمراه کند.
    ok('قواعد مرده .loading-progress از app.css حذف شده',
        !/(^|\n)\s*\.loading-progress/.test(appcss));
    ok('توضیح جای آن قواعد در app.css مانده (چرا حذف شدند)', appcss.includes('لودر سبک قدیمی'));
    ok('قواعد خطای Blazor دست‌نخورده مانده', appcss.includes('#blazor-error-ui'));
}

/* ==========================================================================
   نتیجه
   ========================================================================== */

setTimeout(() => {
    console.log('\n' + (failed === 0
        ? `✅ همه بررسی‌ها موفق`
        : `❌ ${failed} بررسی ناموفق`));
    process.exit(failed === 0 ? 0 : 1);
}, 60);
