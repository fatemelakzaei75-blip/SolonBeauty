/* ==========================================================================
   SolonBeauty — هلپر تجربه کاربری موبایل (Mobile UX Helper)
   --------------------------------------------------------------------------
   مسئولیت‌ها:
   ۱) قفل/آزادسازی اسکرول پس‌زمینه هنگام باز بودن Drawer یا مودال
      (بدون این مورد، کاربر موبایل پشت منو هم اسکرول می‌کند)
   ۲) بستن Drawer/مودال با کلید Escape و دکمه بازگشت اندروید
   ۳) حفظ موقعیت اسکرول هنگام قفل شدن صفحه (بدون پرش به بالا)
   ========================================================================== */

(function () {
    'use strict';

    // تعداد عناصری که درخواست قفل اسکرول کرده‌اند (شمارشِ مرجع)
    var lockCount = 0;
    var savedScrollY = 0;

    function lockScroll() {
        lockCount++;
        if (lockCount > 1) return;

        savedScrollY = window.scrollY || window.pageYOffset || 0;
        document.documentElement.classList.add('ux-scroll-locked');
        document.body.classList.add('ux-scroll-locked');

        // تثبیت بصری موقعیت فعلی تا صفحه به بالا نپرد
        document.body.style.top = '-' + savedScrollY + 'px';
        document.body.style.position = 'fixed';
        document.body.style.width = '100%';
    }

    function unlockScroll() {
        if (lockCount > 0) lockCount--;
        if (lockCount > 0) return;

        document.documentElement.classList.remove('ux-scroll-locked');
        document.body.classList.remove('ux-scroll-locked');

        document.body.style.position = '';
        document.body.style.top = '';
        document.body.style.width = '';

        window.scrollTo(0, savedScrollY);
    }

    /**
     * قفل یا آزادسازی اسکرول پس‌زمینه.
     * @param {boolean} locked
     */
    function setScrollLock(locked) {
        if (locked) { lockScroll(); } else { unlockScroll(); }
    }

    /**
     * تشخیص دستگاه لمسی — برای تصمیم‌گیری‌های رفتاری در کامپوننت‌ها
     * @returns {boolean}
     */
    function isTouchDevice() {
        return window.matchMedia('(hover: none), (pointer: coarse)').matches;
    }

    /**
     * تشخیص موبایل واقعی (عرض کمتر از نقطه شکست md)
     * @returns {boolean}
     */
    function isMobileViewport() {
        return window.matchMedia('(max-width: 900px)').matches;
    }

    /**
     * اسکرول نرم به بالای صفحه — در تغییر مسیرهای موبایل استفاده می‌شود
     * تا کاربر وسط صفحه جدید فرود نیاید.
     */
    function scrollToTop() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    /**
     * بستن خودکار Drawer در صورت چرخش دستگاه به حالت تبلت/دسکتاپ
     * تا منوی کشویی روی صفحه بزرگ گیر نکند.
     * @param {any} dotNetRef ارجاع به کامپوننت Blazor
     */
    var viewportMq = null;
    var viewportHandler = null;
    var viewportRef = null;

    function watchViewport(dotNetRef) {
        unwatchViewport();

        viewportRef = dotNetRef;
        viewportMq = window.matchMedia('(min-width: 1024px)');
        viewportHandler = function (e) {
            if (!e.matches) return;
            try { viewportRef.invokeMethodAsync('OnViewportBecameDesktop'); }
            catch (err) { /* کامپوننت آزاد شده است */ }
        };
        viewportMq.addEventListener('change', viewportHandler);
    }

    function unwatchViewport() {
        if (viewportMq && viewportHandler) {
            viewportMq.removeEventListener('change', viewportHandler);
        }
        viewportMq = null;
        viewportHandler = null;
        viewportRef = null;
    }

    /* ======================================================================
       وضعیت باز/بسته بودن Sidebar پنل
       ----------------------------------------------------------------------
       ذخیره در localStorage تا بین بارگذاری‌های صفحه حفظ شود.
       ====================================================================== */
    var SIDEBAR_KEY = 'solon.panel.sidebar';

    /**
     * خواندن وضعیت ذخیره‌شده Sidebar.
     * @returns {string|null} 'expanded' یا 'rail' یا null اگر ذخیره نشده باشد
     */
    function getSidebarState() {
        try {
            var v = window.localStorage.getItem(SIDEBAR_KEY);
            return (v === 'expanded' || v === 'rail') ? v : null;
        } catch (e) {
            // حالت مرور خصوصی یا مسدود بودن localStorage
            return null;
        }
    }

    /**
     * ذخیره وضعیت Sidebar.
     * @param {string} state 'expanded' | 'rail'
     */
    function setSidebarState(state) {
        if (state !== 'expanded' && state !== 'rail') return;
        try { window.localStorage.setItem(SIDEBAR_KEY, state); } catch (e) { }
    }

    /**
     * دسکتاپ عریض (≥1280px) — حالت پیش‌فرض باز در این عرض است.
     * در لپ‌تاپ و تبلت افقی (1024 تا 1279) پیش‌فرض حالت بسته (Rail) است.
     * @returns {boolean}
     */
    function isWideDesktop() {
        return window.matchMedia('(min-width: 1280px)').matches;
    }

    /**
     * آیا کاربر روی صفحه‌ای است که Sidebar کپسولی نمایش داده می‌شود؟ (≥1024px)
     * @returns {boolean}
     */
    function isSidebarCapable() {
        return window.matchMedia('(min-width: 1024px)').matches;
    }

    /**
     * پاک‌کردن وضعیت ذخیره‌شده — برای بازگشت به پیش‌فرض بر اساس عرض صفحه.
     */
    function clearSidebarState() {
        try { window.localStorage.removeItem(SIDEBAR_KEY); } catch (e) { }
    }

    /* ======================================================================
       بستن دراور با Escape و دکمه بازگشت، به‌همراه مدیریت فوکوس
       ----------------------------------------------------------------------
       نکته: این بخش در نسخه پیشین فقط در توضیحات بالای فایل ادعا شده بود و
       هیچ کدی نداشت؛ یعنی دراور با Escape بسته نمی‌شد. حالا واقعاً پیاده
       شده است.

       چرا فوکوس مهم است: وقتی دراور باز می‌شود، کاربر کیبورد باید داخل آن
       حرکت کند و وقتی بسته می‌شود، فوکوس باید به همان دکمه‌ای برگردد که
       دراور را باز کرده. بدون این، فوکوس پشت لایه تاریک گم می‌شود و کاربر
       کیبورد عملاً در صفحه زندانی می‌ماند.

       چرا یک دکمه تاریخچه اضافه می‌شود: دکمه بازگشت اندروید در مرورگر
       همان popstate است. با یک pushState هنگام باز شدن، دکمه بازگشت اول
       دراور را می‌بندد و کاربر از صفحه بیرون نمی‌رود.
       ====================================================================== */
    var drawerHistoryPushed = false;
    var lastFocused = null;

    function drawerEl() { return document.querySelector('.pnav-drawer'); }

    function isDrawerOpen() {
        var d = drawerEl();
        return !!(d && d.classList.contains('is-open'));
    }

    function findDrawerToggle() {
        return document.querySelector('.pnav-bnav-item[aria-controls="panel-drawer"], .panel-mob-toggle');
    }

    /**
     * فوکوس را داخل دراور نگه می‌دارد. بدون این، Tab از انتهای منو بیرون
     * می‌زند و روی محتوای پشت لایه تاریک می‌چرخد.
     */
    function trapFocus(e) {
        if (e.key !== 'Tab') return;

        var d = drawerEl();
        if (!d) return;

        var focusables = d.querySelectorAll(
            'a[href], button:not([disabled]), [tabindex]:not([tabindex="-1"])');
        if (!focusables.length) return;

        var first = focusables[0];
        var last = focusables[focusables.length - 1];

        if (e.shiftKey && document.activeElement === first) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
            e.preventDefault();
            first.focus();
        }
    }

    function onKeyDown(e) {
        if (e.key === 'Escape' && isDrawerOpen()) {
            e.preventDefault();
            closeDrawerFromJs();
            return;
        }
        trapFocus(e);
    }

    /**
     * دراور را از سمت JS می‌بندد و به کامپوننت اطلاع می‌دهد.
     * رویداد سفارشی پل ارتباطی است: کامپوننت Blazor وضعیت خودش را دارد و
     * JS نمی‌تواند مستقیماً آن را عوض کند.
     */
    function closeDrawerFromJs() {
        // همان ارجاع Blazor که watchViewport گرفته، استفاده می‌شود؛ لازم نیست
        // دو کانال ارتباطی موازی داشته باشیم.
        if (viewportRef) {
            try {
                viewportRef.invokeMethodAsync('OnDrawerCloseRequested');
                return;
            } catch (err) {
                // کامپوننت آزاد شده یا ارتباط قطع است — مسیر جایگزین پایین
            }
        }

        // مسیر جایگزین: در محیطی که ارجاع Blazor نیست (مثل پیش‌نمایش مستقل
        // یا اتصال قطع‌شده)، کلاس را مستقیم از DOM برمی‌داریم تا دراور
        // همچنان بسته شود و کاربر گیر نکند.
        var d = drawerEl();
        if (d) d.classList.remove('is-open');
    }

    function onPopState() {
        if (isDrawerOpen()) closeDrawerFromJs();
    }

    /**
     * باید پس از هر رندر از سمت Blazor صدا زده شود. تغییر وضعیت دراور را
     * از DOM می‌خواند و کارهای مربوط به فوکوس و تاریخچه را هم‌گام می‌کند.
     */
    function syncDrawerState() {
        var open = isDrawerOpen();

        if (open && !window.__solonDrawerBound) {
            document.addEventListener('keydown', onKeyDown);
            window.addEventListener('popstate', onPopState);
            window.__solonDrawerBound = true;

            lastFocused = document.activeElement;
            if (!history.state || !history.state.solonDrawer) {
                history.pushState({ solonDrawer: true }, '');
                drawerHistoryPushed = true;
            }

            // فوکوس روی اولین کنترل دراور تا کاربر کیبورد بلافاصله داخلش باشد
            var d = drawerEl();
            var target = d && d.querySelector('.pnav-drawer-close');
            if (target) {
                // تأخیر تا انیمیشن شروع شود و پرش فوکوس دیده نشود
                setTimeout(function () { try { target.focus(); } catch (e) { } }, 60);
            }
        } else if (!open && window.__solonDrawerBound) {
            document.removeEventListener('keydown', onKeyDown);
            window.removeEventListener('popstate', onPopState);
            window.__solonDrawerBound = false;

            if (drawerHistoryPushed) {
                drawerHistoryPushed = false;
                // فقط اگر همان ورودی تاریخچه خودمان بالای پشته باشد برمی‌گردیم
                if (history.state && history.state.solonDrawer) history.back();
            }

            if (lastFocused && document.contains(lastFocused)) {
                try { lastFocused.focus(); } catch (e) { }
            } else {
                var toggle = findDrawerToggle();
                if (toggle) { try { toggle.focus(); } catch (e) { } }
            }
            lastFocused = null;
        }
    }

    /* ======================================================================
       پل اتصال قرص فعال به پنل محتوا
       ----------------------------------------------------------------------
       چرا این کار با JS انجام می‌شود و نه CSS خالص:
       فهرست منو (.pnav-list) برای اسکرول عمودی overflow دارد و overflow در
       یک محور باعث می‌شود محور دیگر هم بُرده شود. پس هر قرصی که بخواهد از
       لبه کپسول بیرون بزند، حذف می‌شود. راه‌حل درست: قرص فعال داخل فهرست
       کاملاً شفاف می‌ماند و یک عنصر جداگانه بیرون از فهرست، پس‌زمینه و
       گوشه‌های مقعر را می‌کشد. این عنصر موقعیت خود را از ارتفاع آیتم فعال
       می‌گیرد.
       ====================================================================== */
    var bridgeRaf = null;

    function syncActiveBridge() {
        if (bridgeRaf) cancelAnimationFrame(bridgeRaf);
        bridgeRaf = requestAnimationFrame(applyBridgeGeometry);
    }

    function applyBridgeGeometry() {
        bridgeRaf = null;
        // روی همه Sidebarهای صفحه کار می‌کند. در اپ یکی است، ولی در ابزار
        // پیش‌نمایش دو نمونه (باز و بسته) کنار هم رندر می‌شوند و همین باعث
        // می‌شود پیش‌نمایش دقیقاً با کد واقعی ساخته شود، نه با کد موازی.
        var sidebars = document.querySelectorAll('.panel-sidebar');
        for (var i = 0; i < sidebars.length; i++) {
            applyBridgeTo(sidebars[i]);
        }
    }

    function applyBridgeTo(sidebar) {
        var capsule = sidebar.querySelector('.pnav-capsule');
        var list = sidebar.querySelector('.pnav-list');
        var active = sidebar.querySelector('.pnav-item.is-active');
        var bridge = sidebar.querySelector('.pnav-bridge');
        if (!capsule || !list || !bridge) return;

        var hide = function () { sidebar.classList.remove('has-bridge'); };

        // در موبایل کپسول پنهان است (offsetParent تهی می‌شود) و پل لازم نیست
        if (!active || sidebar.offsetParent === null) { hide(); return; }

        var cRect = capsule.getBoundingClientRect();
        var lRect = list.getBoundingClientRect();
        var aRect = active.getBoundingClientRect();

        // اگر آیتم فعال با اسکرول از کادر فهرست بیرون رفته باشد، پل خارج از
        // کپسول رندر می‌شود. در این حالت کنارش می‌گذاریم و خود آیتم با
        // قاعده پشتیبان CSS پس‌زمینه می‌گیرد.
        if (aRect.bottom <= lRect.top + 1 || aRect.top >= lRect.bottom - 1) {
            hide();
            return;
        }

        // ضخامت حاشیه کپسول: مختصات getBoundingClientRect روی border-box است
        // ولی عناصر absolute نسبت به padding-box موقعیت می‌گیرند.
        var border = capsule.clientLeft || 0;
        var borderTop = capsule.clientTop || 0;

        var rtl = getComputedStyle(document.documentElement).direction === 'rtl';

        // شروع پل = لبه‌ی inline-start خودِ آیتم فعال. این فرمول در هر دو
        // حالت باز و بسته یکسان است: در حالت باز قرص از لبه‌ی متن شروع
        // می‌شود و در حالت بسته از لبه‌ی دایره‌ی آیکون.
        var startPx = rtl
            ? (cRect.right - border) - aRect.right
            : aRect.left - (cRect.left + border);

        // سرکشی پل تا لبه‌ی پنل محتوا. اندازه‌گیری واقعی به‌جای تکیه بر
        // سایز حفره گرید، تا اگر فاصله در بریک‌پوینتی عوض شد پل جدا نیفتد.
        // جست‌وجو در همان پوسته انجام می‌شود نه در کل سند، تا اگر چند پوسته
        // در صفحه باشد هر پل به پنل خودش وصل شود.
        var shell = sidebar.closest('.panel-shell--capsule');
        var content = shell ? shell.querySelector('.panel-content') : null;
        var overhang = null;
        if (content) {
            var pRect = content.getBoundingClientRect();
            var gap = rtl
                ? (cRect.left + border) - pRect.right
                : pRect.left - (cRect.right - border);
            // یک پیکسل داخل پنل هم می‌رود تا خط موییِ ناشی از گردکردن
            // اعشار در رزولوشن‌های خاص بین پل و پنل باقی نماند.
            overhang = gap + 1;
        }

        sidebar.style.setProperty('--bridge-start', Math.max(startPx, 0) + 'px');
        sidebar.style.setProperty('--bridge-y', (aRect.top - cRect.top - borderTop) + 'px');
        sidebar.style.setProperty('--bridge-h', aRect.height + 'px');
        if (overhang !== null && overhang >= 0) {
            sidebar.style.setProperty('--bridge-overhang', overhang + 'px');
        }

        sidebar.classList.add('has-bridge');
    }

    /**
     * اتصال شنونده‌های لازم: اسکرول فهرست، تغییر اندازه پنجره و بارگذاری فونت.
     * بدون این‌ها، پل پس از اسکرول یا تغییر اندازه از جای آیتم فعال می‌افتد.
     */
    function initActiveBridge() {
        var sidebars = document.querySelectorAll('.panel-sidebar');
        for (var i = 0; i < sidebars.length; i++) {
            bindBridgeTo(sidebars[i]);
        }

        if (!window.__solonBridgeBound) {
            window.addEventListener('resize', syncActiveBridge, { passive: true });
            window.addEventListener('orientationchange', syncActiveBridge, { passive: true });
            // فونت‌ها پس از بارگذاری ارتفاع آیتم‌ها را تغییر می‌دهند
            if (document.fonts && document.fonts.ready) {
                document.fonts.ready.then(syncActiveBridge);
            }
            window.__solonBridgeBound = true;
        }

        syncActiveBridge();
    }

    function bindBridgeTo(sidebar) {
        var list = sidebar.querySelector('.pnav-list');
        if (list && !list.dataset.bridgeBound) {
            list.addEventListener('scroll', syncActiveBridge, { passive: true });
            list.dataset.bridgeBound = '1';
        }

        // انیمیشن عرض کپسول ۲۲۰ms است؛ در میانه‌ی آن ارتفاع و جای آیتم فعال
        // هنوز نهایی نشده و پل باید پس از پایان انیمیشن دوباره خوانده شود.
        //
        // نام ویژگی عمداً بررسی نمی‌شود: مرورگرها برای ویژگی‌های منطقی
        // (inline-size) گاهی نام فیزیکی (width) را گزارش می‌کنند و گیرکردن
        // روی یک نام، پل را برای همیشه در جای اشتباه رها می‌کند. هزینه‌ی
        // خواندن دوباره صفر است چون با requestAnimationFrame مهار شده.
        var capsule = sidebar.querySelector('.pnav-capsule');
        if (capsule && !capsule.dataset.bridgeBound) {
            capsule.addEventListener('transitionend', function () { syncActiveBridge(); });
            capsule.dataset.bridgeBound = '1';
        }
    }

    window.solonUx = {
        setScrollLock: setScrollLock,
        isTouchDevice: isTouchDevice,
        isMobileViewport: isMobileViewport,
        scrollToTop: scrollToTop,
        watchViewport: watchViewport,
        unwatchViewport: unwatchViewport,
        getSidebarState: getSidebarState,
        setSidebarState: setSidebarState,
        isWideDesktop: isWideDesktop,
        isSidebarCapable: isSidebarCapable,
        clearSidebarState: clearSidebarState,
        initActiveBridge: initActiveBridge,
        syncActiveBridge: syncActiveBridge,
        syncDrawerState: syncDrawerState
    };
})();
