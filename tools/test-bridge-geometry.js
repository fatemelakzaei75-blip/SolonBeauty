/* ==========================================================================
   تست ریاضیات «پل اتصال» قرص فعال به پنل محتوا
   --------------------------------------------------------------------------
   چرا این فایل وجود دارد:
   محاسبه‌ی محل پل با JS انجام می‌شود و در محیط توسعه ما مرورگر در دسترس
   نیست تا نتیجه را چشمی ببینیم. این تست یک DOM شبیه‌سازی‌شده می‌سازد، تابع
   واقعی solonUx.initActiveBridge را روی آن اجرا می‌کند و اعداد خروجی را
   با هندسه‌ی مورد انتظار مقایسه می‌کند — هم برای RTL و هم LTR، و هم برای
   حالت باز و بسته.

   اجرا:  node tools/test-bridge-geometry.js
   ========================================================================== */
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const JS_SRC = path.join(
    __dirname, '..', 'BlazorAppSolon', 'wwwroot', 'js', 'mobile-ux.js'
);

/* ─────────────── DOM شبیه‌سازی‌شده ─────────────── */

function makeClassList() {
    const set = new Set();
    return {
        add: (c) => set.add(c),
        remove: (c) => set.delete(c),
        contains: (c) => set.has(c),
        _set: set
    };
}

function makeEl(rect, opts = {}) {
    const el = {
        rect,
        clientLeft: opts.border || 0,
        clientTop: opts.border || 0,
        offsetParent: opts.hidden ? null : {},
        dataset: {},
        classes: makeClassList(),
        props: {},
        listeners: {},
        getBoundingClientRect: () => rect,
        addEventListener(type, fn) { (el.listeners[type] = el.listeners[type] || []).push(fn); },
        style: { setProperty: (k, v) => { el.props[k] = v; } },
        querySelector: (sel) => (opts.children ? opts.children[sel] || null : null),
        closest: (sel) => (opts.closest ? opts.closest[sel] || null : null)
    };
    Object.defineProperty(el, 'classList', { get: () => el.classes });
    return el;
}

/**
 * یک سناریو می‌سازد: پوسته > [sidebar > capsule > (bridge, list > item), panel]
 * مختصات همه چیز طوری چیده می‌شود که یک چیدمان واقعی را بازتولید کند.
 */
function scenario({ dir, rail, capsuleLeft, gap = 16, panelSide, visible = true }) {
    const wCapsule = rail ? 78 : 236;
    const border = 1;
    const capsuleRect = {
        left: capsuleLeft,
        right: capsuleLeft + wCapsule,
        top: 16,
        bottom: 736,
        width: wCapsule,
        height: 720
    };

    // پنل محتوا در RTL سمت چپ کپسول است، در LTR سمت راست
    const panelRect = dir === 'rtl'
        ? { left: capsuleLeft - gap - 600, right: capsuleLeft - gap, top: 16, bottom: 736 }
        : { left: capsuleLeft + wCapsule + gap, right: capsuleLeft + wCapsule + gap + 600, top: 16, bottom: 736 };

    // آیتم فعال: در حالت باز از لبه‌ی padding شروع می‌شود،
    // در حالت بسته دایره‌ی ۴۴px در وسط کپسول
    const padBoxStart = capsuleLeft + border;
    const padBoxEnd = capsuleLeft + wCapsule - border;
    const itemW = 44;
    const itemLeft = rail
        ? padBoxStart + (padBoxEnd - padBoxStart - itemW) / 2
        : padBoxStart + (dir === 'rtl' ? 0 : 10);
    const itemRight = rail ? itemLeft + itemW : padBoxEnd - (dir === 'rtl' ? 10 : 0);
    const itemRect = { left: itemLeft, right: itemRight, top: 300, bottom: 344, width: itemRight - itemLeft, height: 44 };

    const listRect = { left: capsuleLeft, right: capsuleRect.right, top: 120, bottom: 700 };

    const content = makeEl(panelRect, { border: 0 });
    const bridge = makeEl({}, { hidden: true });
    const list = makeEl(listRect, { children: { '.pnav-item.is-active': null } });

    const capsule = makeEl(capsuleRect, {
        border,
        children: { '.pnav-list': list, '.pnav-item.is-active': makeEl(itemRect, { border: 0 }), '.pnav-bridge': bridge }
    });

    const shell = makeEl({}, { children: { '.panel-content': content } });
    const sidebar = makeEl({}, {
        hidden: !visible,
        children: { '.pnav-capsule': capsule, '.pnav-list': list, '.pnav-item.is-active': capsule.querySelector('.pnav-item.is-active'), '.pnav-bridge': bridge },
        closest: { '.panel-shell--capsule': shell }
    });

    return { sidebar, capsule, bridge };
}

function run({ dir, rail, capsuleLeft, panelSide, visible = true }) {
    const s = scenario({ dir, rail, capsuleLeft, panelSide, visible });
    const document = {
        documentElement: {},
        querySelectorAll: (sel) => (sel === '.panel-sidebar' ? [s.sidebar] : []),
        querySelector: () => null,
        fonts: { ready: Promise.resolve() }
    };
    const sandbox = {
        document,
        window: { addEventListener: () => {}, __solonBridgeBound: false },
        getComputedStyle: () => ({ direction: dir }),
        requestAnimationFrame: (fn) => { fn(); return 1; },
        cancelAnimationFrame: () => {},
        localStorage: { getItem: () => null, setItem: () => {}, removeItem: () => {} },
        matchMedia: () => ({ matches: false, addEventListener: () => {} }),
        navigator: { maxTouchPoints: 0 },
        console
    };
    sandbox.window.document = document;
    sandbox.globalThis = sandbox;
    vm.createContext(sandbox);
    vm.runInContext(fs.readFileSync(JS_SRC, 'utf8'), sandbox);

    sandbox.window.solonUx.initActiveBridge();
    return { props: s.sidebar.props, bridgeShown: s.sidebar.classList.contains('has-bridge') };
}

/* ─────────────── انتظارها ───────────────
   قرارداد:
   ۱) پل باید از لبه‌ی inline-start خودِ آیتم فعال شروع شود.
   ۲) پل باید ۱ پیکسل داخل پنل محتوا تمام شود (نه کمتر، نه بیشتر):
      فاصله‌ی کپسول تا پنل + ضخامت حاشیه کپسول + ۱
   ۳) ارتفاع پل = ارتفاع آیتم فعال.
   ۴) لایه‌ی pnav-bridge در DOM هست و کلاس has-bridge اضافه می‌شود. */
const cases = [
    { name: 'RTL / باز (دسکتاپ عریض)', dir: 'rtl', rail: false, capsuleLeft: 1264 },
    { name: 'RTL / بسته (Rail)', dir: 'rtl', rail: true, capsuleLeft: 1422 },
    { name: 'LTR / باز', dir: 'ltr', rail: false, capsuleLeft: 16 },
    { name: 'LTR / بسته (Rail)', dir: 'ltr', rail: true, capsuleLeft: 16 }
];

let failed = 0;

for (const c of cases) {
    const out = run(c);
    const gap = 16;
    const border = 1;
    const wCapsule = c.rail ? 78 : 236;

    // انتظار ۱: شروع از لبه‌ی آیتم فعال
    const expectedStart = c.rail
        ? (wCapsule - 2 * border - 44) / 2      // دایره‌ی وسط‌چین
        : 10;                                    // از لبه‌ی padding کپسول

    // انتظار ۲: پایان ۱ پیکسل داخل پنل
    const expectedOverhang = gap + border + 1;

    const start = parseFloat(out.props['--bridge-start']);
    const overhang = parseFloat(out.props['--bridge-overhang']);
    const h = parseFloat(out.props['--bridge-h']);

    const checks = [
        ['شروع پل از لبه آیتم', start, expectedStart],
        ['سرکشی تا ۱px داخل پنل', overhang, expectedOverhang],
        ['ارتفاع پل = ارتفاع آیتم', h, 44],
        ['has-bridge اضافه شده', out.bridgeShown, true]
    ];

    // عرض نهایی پل باید دقیقاً از لبه‌ی آیتم تا ۱px داخل پنل باشد.
    // چون هر دو ویژگی منطقی‌اند، فرمول در RTL و LTR یکسان است:
    // فاصله‌ی لبه‌ی شروع پل از لبه‌ی inline-start کپسول + عرض پل، منهای
    // عرض ناحیه‌ی padding کپسول = مقدار سرریز به سمت پنل.
    const padBoxW = wCapsule - 2 * border;
    const bridgeW = padBoxW - start + overhang;
    checks.push(['سرریز پل به سمت پنل', (start + bridgeW) - padBoxW, gap + border + 1]);

    console.log(`\n▸ ${c.name}`);
    for (const [label, actual, expected] of checks) {
        const ok = Math.abs(actual - expected) < 0.001;
        if (!ok) failed++;
        console.log(`   ${ok ? '✅' : '❌'} ${label}: ${actual}  (انتظار: ${expected})`);
    }
    console.log(`   ℹ️  عرض نهایی پل: ${bridgeW}px، ارتفاع: ${h}px`);
}

/* ─────────────── حالت‌های منفی (فروپاشی محترمانه) ───────────────
   در موبایل کپسول پنهان است و offsetParent تهی می‌شود. در آن حالت نباید
   هیچ پل یا متغیر موقعیتی روی Sidebar نوشته شود تا قاعده‌ی پشتیبان CSS
   قرص فعال را با پس‌زمینه‌ی ساده نشان دهد. */
console.log('\n▸ حالت‌های منفی');
const negatives = [
    ['موبایل — کپسول پنهان', { dir: 'rtl', rail: false, capsuleLeft: 0, visible: false }]
];
for (const [label, opts] of negatives) {
    const out = run(opts);
    const ok = !out.bridgeShown && Object.keys(out.props).length === 0;
    if (!ok) failed++;
    console.log(`   ${ok ? '✅' : '❌'} ${label}: پل خاموش ماند و متغیری نوشته نشد`);
}

console.log('\n' + (failed === 0 ? '✅ همه بررسی‌ها موفق' : `❌ ${failed} بررسی ناموفق`));
process.exit(failed === 0 ? 0 : 1);
