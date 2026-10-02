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

    window.solonUx = {
        setScrollLock: setScrollLock,
        isTouchDevice: isTouchDevice,
        isMobileViewport: isMobileViewport,
        scrollToTop: scrollToTop,
        watchViewport: watchViewport,
        unwatchViewport: unwatchViewport
    };
})();
