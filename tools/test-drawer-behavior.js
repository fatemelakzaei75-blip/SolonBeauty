/* ==========================================================================
   تست رفتار دراور موبایل — Escape، بازگشت، تله فوکوس و بازگشت فوکوس
   --------------------------------------------------------------------------
   چرا این فایل وجود دارد:
   در نسخه پیشین، توضیحات بالای mobile-ux.js ادعا می‌کرد «بستن دراور با
   Escape و دکمه بازگشت اندروید» پیاده شده است، در حالی که هیچ کدی نداشت.
   این تست همان ادعا را به یک بررسی اجراشدنی تبدیل می‌کند تا دیگر تکرار نشود.

   محیط مرورگر در دسترس نیست، پس یک DOM کمینه شبیه‌سازی می‌شود که فقط
   همان چیزی را دارد که این کد لازم دارد.

   اجرا:  node tools/test-drawer-behavior.js
   ========================================================================== */
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const JS_SRC = path.join(__dirname, '..', 'BlazorAppSolon', 'wwwroot', 'js', 'mobile-ux.js');

/* ─────────────── DOM کمینه ─────────────── */

function makeClassList(initial) {
    const set = new Set(initial || []);
    return {
        add: (c) => set.add(c),
        remove: (c) => set.delete(c),
        contains: (c) => set.has(c),
        toggle: (c, on) => (on ? set.add(c) : set.delete(c)),
        _set: set
    };
}

function makeElement(name) {
    const el = {
        name,
        classList: makeClassList(),
        focusCount: 0,
        focus() { el.focusCount++; sandboxRef.document.activeElement = el; },
        querySelector: () => null,
        querySelectorAll: () => [],
        _focusables: [],
        matches: () => false
    };
    return el;
}

let sandboxRef = null;

function buildSandbox(drawerOpen) {
    const closeBtn = makeElement('close-btn');
    const toggleBtn = makeElement('toggle-btn');
    const outside = makeElement('outside');

    const drawer = makeElement('drawer');
    if (drawerOpen) drawer.classList.add('is-open');
    drawer.querySelector = (sel) => (sel === '.pnav-drawer-close' ? closeBtn : null);
    // تله فوکوس از querySelectorAll لیست کنترل‌های دراور را می‌گیرد
    drawer.querySelectorAll = () => [closeBtn];

    const listeners = {};         // document listeners
    const winListeners = {};      // window listeners

    const history = {
        state: null,
        pushed: 0,
        backCalled: 0,
        pushState(state) { history.state = state; history.pushed++; },
        back() { history.backCalled++; history.state = null; }
    };

    const document_ = {
        activeElement: outside,
        addEventListener: (t, fn) => { (listeners[t] = listeners[t] || []).push(fn); },
        removeEventListener: (t, fn) => {
            listeners[t] = (listeners[t] || []).filter((f) => f !== fn);
        },
        dispatch: (t, ev) => { (listeners[t] || []).slice().forEach((f) => f(ev)); },
        listenerCount: (t) => (listeners[t] || []).length,
        querySelector: (sel) => {
            if (sel === '.pnav-drawer') return drawer;
            if (sel.indexOf('.pnav-bnav-item') === 0 || sel === '.panel-mob-toggle') return toggleBtn;
            return null;
        },
        querySelectorAll: () => [],
        contains: (el) => el === outside || el === toggleBtn || el === closeBtn
    };

    const window_ = {
        addEventListener: (t, fn) => { (winListeners[t] = winListeners[t] || []).push(fn); },
        removeEventListener: (t, fn) => {
            winListeners[t] = (winListeners[t] || []).filter((f) => f !== fn);
        },
        dispatch: (t, ev) => { (winListeners[t] || []).slice().forEach((f) => f(ev)); },
        listenerCount: (t) => (winListeners[t] || []).length,
        matchMedia: () => ({ matches: false, addEventListener: () => {}, removeEventListener: () => {} })
    };

    // setTimeout در syncDrawerState برای تأخیر فوکوس استفاده می‌شود؛
    // اینجا صف می‌شود تا تست بتواند آگاهانه اجرایش کند.
    const timers = [];
    const sandbox = {
        document: document_,
        window: window_,
        history,
        console,
        getComputedStyle: () => ({ direction: 'rtl' }),
        requestAnimationFrame: (fn) => { fn(); return 1; },
        cancelAnimationFrame: () => {},
        localStorage: { getItem: () => null, setItem: () => {}, removeItem: () => {} },
        navigator: { maxTouchPoints: 0 },
        setTimeout: (fn) => { timers.push(fn); return timers.length; },
        clearTimeout: () => {},
        CustomEvent: function (type) { this.type = type; }
    };
    sandbox.window.__proto__ = null;
    sandbox.globalThis = sandbox;
    sandbox.window.document = document_;
    sandbox.window.history = history;

    sandboxRef = sandbox;
    vm.createContext(sandbox);
    vm.runInContext(fs.readFileSync(JS_SRC, 'utf8'), sandbox);

    return {
        sandbox, drawer, closeBtn, toggleBtn, outside, history,
        document: document_, window: window_,
        flushTimers: () => { timers.splice(0).forEach((fn) => fn()); }
    };
}

/* ─────────────── اجرای سناریو ─────────────── */

let failed = 0;

function check(label, actual, expected) {
    const ok = actual === expected;
    if (!ok) failed++;
    console.log(`   ${ok ? '✅' : '❌'} ${label}: ${actual}${ok ? '' : `  (انتظار: ${expected})`}`);
}

console.log('\n▸ دراور باز می‌شود');
{
    const env = buildSandbox(true);
    const calls = [];
    const dotNetRef = { invokeMethodAsync: (name) => { calls.push(name); } };

    // همان ترتیبی که PanelLayout طی می‌کند: اول watchViewport، بعد syncDrawerState
    env.sandbox.window.solonUx.watchViewport(dotNetRef);
    env.sandbox.window.solonUx.syncDrawerState();
    env.flushTimers();

    check('شنونده کلید ثبت شد', env.document.listenerCount('keydown'), 1);
    check('ورودی تاریخچه اضافه شد', env.history.pushed, 1);
    check('فوکوس به دکمه بستن رفت', env.closeBtn.focusCount, 1);
    check('فوکوس از عنصر قبلی برداشته شد', env.sandbox.document.activeElement === env.outside, false);
    check('هیچ درخواست بستنی صادر نشده', calls.length, 0);

    console.log('\n▸ کلید Escape');
    env.document.dispatch('keydown', { key: 'Escape', preventDefault() {}, shiftKey: false });
    check('به Blazor خبر داده شد', calls.join(','), 'OnDrawerCloseRequested');
}

console.log('\n▸ تله فوکوس (Tab در آخرین کنترل)');
{
    const env = buildSandbox(true);
    env.sandbox.window.solonUx.watchViewport({ invokeMethodAsync: () => {} });
    env.sandbox.window.solonUx.syncDrawerState();
    env.flushTimers();

    // آخرین کنترل دراور دکمه بستن است (تنها کنترل شبیه‌سازی‌شده)
    env.sandbox.document.activeElement = env.closeBtn;
    let prevented = false;
    env.document.dispatch('keydown', {
        key: 'Tab', shiftKey: false,
        preventDefault() { prevented = true; }
    });
    check(' چرخش فوکوس جلوگیری شد', prevented, true);
}

console.log('\n▸ دکمه بازگشت اندروید (popstate)');
{
    const env = buildSandbox(true);
    const calls = [];
    env.sandbox.window.solonUx.watchViewport({ invokeMethodAsync: (n) => calls.push(n) });
    env.sandbox.window.solonUx.syncDrawerState();
    env.flushTimers();

    env.window.dispatch('popstate', {});
    check('popstate درخواست بستن داد', calls.join(','), 'OnDrawerCloseRequested');
}

console.log('\n▸ دراور بسته می‌شود');
{
    const env = buildSandbox(true);
    env.sandbox.window.solonUx.watchViewport({ invokeMethodAsync: () => {} });
    env.sandbox.window.solonUx.syncDrawerState();
    env.flushTimers();

    // Blazor کلاس is-open را برداشته؛ JS باید دنبالش بیاید
    env.drawer.classList.remove('is-open');
    env.sandbox.window.solonUx.syncDrawerState();

    check('شنونده کلید برداشته شد', env.document.listenerCount('keydown'), 0);
    check('شنونده popstate برداشته شد', env.window.listenerCount('popstate'), 0);
    check('از ورودی تاریخچه خودمان برگشتیم', env.history.backCalled, 1);
    check('فوکوس به عنصر قبلی برگشت', env.outside.focusCount, 1);
}

console.log('\n▸ بدون dotNetRef (اتصال قطع یا پیش‌نمایش مستقل)');
{
    const env = buildSandbox(true);
    env.sandbox.window.solonUx.syncDrawerState();
    env.flushTimers();

    env.document.dispatch('keydown', { key: 'Escape', preventDefault() {}, shiftKey: false });
    check('دراور در DOM بسته شد', env.drawer.classList.contains('is-open'), false);
}

console.log('\n▸ دراور بسته: هیچ کاری نمی‌شود');
{
    const env = buildSandbox(false);
    env.sandbox.window.solonUx.syncDrawerState();
    check('شنونده‌ای ثبت نشد', env.document.listenerCount('keydown'), 0);
    check('تاریخچه دست نخورد', env.history.pushed, 0);
    check('فوکوس جابه‌جا نشد', env.closeBtn.focusCount, 0);
}

/* ─────────────── بررسی نهایی: قواعد CSS مرحله ۵ ───────────────
   این‌ها ادعاهایی هستند که به‌سادگی باگ می‌شوند و در مرورگر هم بی‌صدا
   از کار می‌افتند (مثلاً جهت اشتباه دراور یا خاموش‌نشدن نوار در دسکتاپ).
   پس در همان تست بررسی می‌شوند. */
console.log('\n▸ قواعد CSS مرحله ۵');
{
    const cssPath = path.join(__dirname, '..', 'BlazorAppSolon', 'wwwroot', 'css', 'panel-sidebar.css');
    const css = fs.readFileSync(cssPath, 'utf8');

    const drawerBlock = css.slice(css.indexOf('.pnav-drawer {'), css.indexOf('.pnav-drawer.is-open'));
    const ltrBlock = css.slice(css.indexOf('[dir="ltr"] {'), css.indexOf('}', css.indexOf('[dir="ltr"] {')));

    check('دراور با ویژگی منطقی موقعیت می‌گیرد (نه چپ/راست فیزیکی)',
        /inset-inline-start:\s*0/.test(drawerBlock), true);
    check('جابه‌جایی پنهان‌شدن از توکن می‌آید',
        /translateX\(var\(--sb-drawer-off\)\)/.test(drawerBlock), true);
    check('توکن در RTL برابر ۱۰۰٪ است', /--sb-drawer-off:\s*100%/.test(css), true);
    check('توکن در LTR آینه می‌شود', /-100%/.test(ltrBlock), true);
    check('دراور بسته فوکوس‌پذیر نیست', /visibility:\s*hidden/.test(drawerBlock), true);
    check('پدینگ محتوا از توکن نوار می‌آید',
        /--sb-bnav-space/.test(css) && /padding-block-end:\s*calc\(var\(--sb-bnav-space/.test(css), true);
    check('نوار پایین در دسکتاپ خاموش می‌شود',
        /min-width:\s*1024px[\s\S]{0,300}\.pnav-bnav/.test(css), true);
    check('آکولادهای CSS متوازن', css.split('{').length === css.split('}').length, true);
    check('هیچ ارجاعی به مارک‌آپ حذف‌شده نمانده',
        /panel-bottom-nav|panel-bnav-item|panel-drawer-backdrop|\.panel-side\s*\{/.test(css), false);
}

console.log('\n' + (failed === 0 ? '✅ همه بررسی‌ها موفق' : `❌ ${failed} بررسی ناموفق`));
process.exit(failed === 0 ? 0 : 1);
