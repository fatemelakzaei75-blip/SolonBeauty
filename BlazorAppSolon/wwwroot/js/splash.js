/* ==========================================================================
   SolonBeauty — موتور صفحه آغاز (نماد بی‌نهایت)
   --------------------------------------------------------------------------
   قرارداد عمومی:
       window.solonSplash.setProgress(p)        // p بین ۰ و ۱
       window.solonSplash.getProgress()         // مقدار نمایش‌داده‌شده
       window.solonSplash.setBrand(o)           // { name, latin, logo } از تنظیمات سالن
       window.solonSplash.watchBlazorLoad(cap)  // فاز اول: پیشرفت واقعی خود Blazor
       window.solonSplash.stopWatching()        // پایان فاز اول (شروع فاز دوم)
       window.solonSplash.preloadImage(url, ms) // Promise<bool>
       window.solonSplash.awaitFont(spec, ms)   // Promise<bool>
       window.solonSplash.finish()              // Promise — پایان واقعی و محو شدن نرم
       window.solonSplash.fail(msg)             // خطا: خشک شدن نماد + پیام + تلاش دوباره
       window.solonSplash.init()                // آماده‌سازی دستی (معمولاً لازم نیست)

   --------------------------------------------------------------------------
   دو فاز پیشرفت، هر دو واقعی:

   فاز اول (۰ تا ۴۵٪) — پیش از بالا آمدن دات‌نت.
   Blazor خودش هنگام دانلود فایل‌ها متغیر --blazor-load-percentage را روی ریشه
   سند می‌نویسد. ما همان را رصد می‌کنیم. این فاز باید خودکار و از همین فایل
   شروع شود، نه از کد سی‌شارپ: چون این دانلود *پیش از* اجرای دات‌نت اتفاق
   می‌افتد، و اگر منتظر سی‌شارپ بمانیم، دیگر تمام شده است.

   فاز دوم (۴۵ تا ۱۰۰٪) — مراحل واقعی کار نرم‌افزار.
   در BootService.cs: خواندن نشست و توکن، گرفتن تنظیمات سالن از API، لوگو،
   فونت وزیرمتن و تصویر هیرو. هر مرحله سقف زمانی دارد و کل فاز بودجه زمانی
   دارد تا هیچ‌چیز نتواند نوار را برای همیشه میخ‌کوب کند.

   --------------------------------------------------------------------------
   سه تصمیم مهم:

   ۱) نگهبان یک‌نوا. یک ایشوی شناخته‌شده در مخزن aspnetcore می‌گوید مقدار
      --blazor-load-percentage می‌تواند از ۱۰۰٪ به ۹۸٪ برگردد، چون مخرج کسر
      (تعداد کل منابع) در حین کار بزرگ‌تر می‌شود. اگر همین‌طور به کاربر نشان
      داده شود، نوار عقب می‌رود و حس خطا می‌دهد. پس همه ورودی‌ها — از هر دو
      فاز — از یک قیف یک‌نوا رد می‌شوند.

   ۲) خزیدن محدود در فاز اول. دانلود یک فایل بزرگ (مثل dotnet.wasm) می‌تواند
      چند ثانیه طول بکشد و در این فاصله درصدی که Blazor گزارش می‌کند تکان
      نمی‌خورد. برای اینکه نوار «یخ‌زده» حس نشود، اگر دو ثانیه هیچ گزارش
      تازه‌ای نرسد، مقدار نمایش‌داده‌شده آرام‌آرام به سقف فاز اول نزدیک
      می‌شود — ولی هرگز از آن رد نمی‌شود و هرگز به ۱۰۰ نمی‌رسد. یعنی حرکت به
      کاربر «زنده بودن» می‌دهد بدون اینکه درباره پایان کار دروغ بگوید.

   ۳) بدون وابستگی بیرونی. کل فایل با API استاندارد مرورگر نوشته شده.
   ========================================================================== */

(function () {
    'use strict';

    /* ---------- زمان شروع ----------
       این اسکریپت آخر بدنه صفحه اجرا می‌شود، یعنی دقیقاً پیش از نخستین رنگ
       آمیزی صفحه آغاز. پس همین لحظه بهترین تخمین برای «از کِی کاربر این صفحه
       را می‌بیند» است و مبنای قاعده حداقل زمان نمایش قرار می‌گیرد. */
    var t0 = Date.now();

    /* ---------- تنظیمات ---------- */

    // ضریب نرم‌سازی: هر فریم چه کسری از فاصله باقی‌مانده پیموده شود.
    var EASE = 0.14;

    // زیر این فاصله، پرش مستقیم به هدف و توقف حلقه.
    var SNAP = 0.0006;

    // سقف فاز اول. فاز دوم (BootService) از همین‌جا ادامه می‌دهد.
    var BOOT_CAP = 0.45;

    // حداقل زمان نمایش. صفحه آغاز نباید چشمک بزند: اگر همه‌چیز سریع آماده
    // شد، این مدت صبر می‌کنیم و بعد محو می‌شویم.
    var MIN_VISIBLE_MS = 800;

    // سقف بی‌کاری فاز اول پیش از شروع خزیدن، و خود نرخ خزیدن.
    var CREEP_AFTER_MS = 2000;
    var CREEP_STEP = 0.004;        // هر ۲۵۰ میلی‌ثانیه ≈ ۱٫۶٪ در ثانیه
    var CREEP_CEILING = 0.92;      // تا ۹۲٪ سقف فاز اول، نه بیشتر

    // اگر پس از این مدت هیچ‌چیز از دات‌نت خبری نشد، یعنی خود Blazor بالا
    // نیامده و هیچ کد سی‌شارپی نمی‌تواند این را تشخیص دهد یا گزارش کند.
    // پس نگهبان همین‌جا در جاوااسکریپت می‌ماند.
    var WATCHDOG_MS = 20000;

    var FA_DIGITS = '۰۱۲۳۴۵۶۷۸۹';

    try {
        if (window.matchMedia &&
            window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            EASE = 1;   // پیشرفت واقعی می‌ماند، فقط نرم‌سازی حرکت حذف می‌شود
        }
    } catch (e) { /* مرورگر قدیمی — مقدار پیش‌فرض می‌ماند */ }

    /* ---------- وضعیت ---------- */

    var instances = [];
    var geomPath = null;      // مسیر هندسی مشترک — تنها تعریف هندسه در کل سند
    var totalLen = 0;         // طول واقعی مسیر، برای getPointAtLength
    var loadObserver = null;
    var creepTimer = null;
    var watchdogTimer = null;
    var lastRealUpdate = 0;

    /* ---------- کمکی‌ها ---------- */

    function now() { return Date.now(); }

    function clamp01(v) {
        v = Number(v);
        if (!isFinite(v)) return 0;
        return v < 0 ? 0 : (v > 1 ? 1 : v);
    }

    /** تبدیل عدد به رشته با ارقام فارسی */
    function toPersianDigits(n) {
        return String(n).replace(/\d/g, function (d) { return FA_DIGITS[+d]; });
    }

    /**
     * خواندن طول واقعی مسیر. فقط یک بار انجام می‌شود چون getTotalLength
     * اجباری است و نتیجه‌اش تا آخر عمر صفحه ثابت می‌ماند.
     */
    function measure() {
        if (geomPath) return totalLen > 0;

        // سراسری جست‌وجو می‌شود، نه داخل ریشه یک نمونه: هندسه در کل سند یکی
        // است و نمونه‌ها آن را به اشتراک می‌گذارند.
        geomPath = document.querySelector('.s-inf__geom');
        if (!geomPath) return false;

        try {
            totalLen = geomPath.getTotalLength();
        } catch (e) {
            totalLen = 0;
        }
        return totalLen > 0;
    }

    /**
     * کشیدن وضعیت جاری روی صفحه. همه چیز از یک عدد تغذیه می‌شود، پس خط
     * پیشرفت و نقطه نور و درصد هرگز از هم جدا نمی‌افتند.
     */
    function render(inst) {
        if (!inst.svg) return;

        // خط پیشرفت و هاله هر دو از همین متغیر تغذیه می‌کنند
        inst.svg.style.setProperty('--inf-p', String(inst.shown));

        // ---- نقطه نور ----
        var head = inst.head;
        if (head && totalLen > 0) {
            var pt = geomPath.getPointAtLength(totalLen * inst.shown);
            head.style.transform = 'translate(' + pt.x + 'px,' + pt.y + 'px)';

            // سر نور در صفر کاملاً محو است و در ۵٪ کامل ظاهر می‌شود. دلیل:
            // در شروع هیچ نوری روی مسیر نیست، ولی یک نقطه تنها در وسط نماد
            // شبیه باقی‌مانده یا باگ دیده می‌شود.
            head.style.opacity = inst.shown <= 0.002
                ? '0'
                : String(Math.min(1, inst.shown * 20));
        }

        // ---- درصد ----
        var pct = Math.round(inst.shown * 100);

        if (pct !== inst.pctShown) {
            inst.pctShown = pct;

            if (inst.pctEl) inst.pctEl.textContent = toPersianDigits(pct) + '٪';

            // اعلام برای صفحه‌خوان فقط سر هر ۱۰ درصد. عددِ دیده‌شدنی در هر
            // تغییر بازنویسی می‌شود؛ اگر خودش در ناحیه زنده بود، تا ۱۰۰ اعلام
            // پشت سر هم می‌شد و تجربه را خراب می‌کرد.
            var decade = Math.floor(pct / 10);

            // سر دهگان صفر اعلامی ندارد: گفتن «۰ درصد بارگذاری شد» در همان
            // لحظه ورود فقط سر و صداست و اطلاعاتی اضافه نمی‌کند.
            if (inst.srEl && decade > 0 && decade !== inst.srDecade) {
                inst.srDecade = decade;
                inst.srEl.textContent = 'در حال بارگذاری نرم‌افزار، '
                    + toPersianDigits(decade * 10) + ' درصد';
            }
        }
    }

    /* ---------- حلقه نرم‌سازی ---------- */

    function tick(inst) {
        inst.raf = null;

        var delta = inst.target - inst.shown;

        if (Math.abs(delta) < SNAP) {
            inst.shown = inst.target;
            render(inst);
            return;                          // حلقه تمام می‌شود؛ بدون کار، بدون فریم
        }

        inst.shown += delta * EASE;
        render(inst);
        inst.raf = requestAnimationFrame(function () { tick(inst); });
    }

    /**
     * ثبت مقدار هدف. اینجا همان نگهبان یک‌نوا است: مقدار مساوی یا کمتر
     * نادیده گرفته می‌شود.
     */
    function setTarget(inst, value) {
        var next = clamp01(value);

        if (next <= inst.target) return;     // هرگز عقب نمی‌رویم
        if (inst.done || inst.failed) return;

        inst.target = next;

        if (inst.raf === null) {
            inst.raf = requestAnimationFrame(function () { tick(inst); });
        }
    }

    /* ---------- آماده‌سازی ---------- */

    function makeInstance(root) {
        var svg = root.querySelector('.s-inf');
        if (!svg) return null;

        return {
            root: root,
            svg: svg,
            head: svg.querySelector('.s-inf__head'),
            pctEl: root.querySelector('[data-pct]'),
            srEl: root.querySelector('[data-sr]'),
            // «حالت زنده» یعنی نمونه‌ای که مقدار ثابت نمایشی ندارد. در اپ،
            // ویژگی data-progress خالی است و همین پرچم روشن می‌شود.
            live: root.getAttribute('data-progress') === null,
            target: 0,
            shown: 0,
            pctShown: -1,
            srDecade: -1,
            raf: null,
            done: false,
            failed: false
        };
    }

    /**
     * قفل کردن اسکرول پس‌زمینه تا وقتی صفحه آغاز روی صحنه است.
     *
     * چرا لازم است: پوسته صفحه آغاز fixed و پوشاننده است، ولی این جلوی
     * اسکرول لمسی صفحه زیرین را نمی‌گیرد. کاربر می‌تواند ناخواسته صفحه را
     * جابه‌جا کند و لحظه محو شدن، جهش محتوا ببیند. با این قفل، «هیچ اسکرولی
     * وجود ندارد» به‌طور واقعی درست می‌شود و بعد از پایان هم آزاد می‌شود.
     */
    function lockScroll(on) {
        try {
            document.documentElement.style.overflow = on ? 'hidden' : '';
        } catch (e) { /* بی‌اهمیت */ }
        try {
            if (document.body) document.body.style.overflow = on ? 'hidden' : '';
        } catch (e) { /* بی‌اهمیت */ }
    }

    function bindRetry(root) {
        var btn = root.querySelector('[data-retry]');
        if (!btn || btn.dataset.bound === '1') return;

        btn.dataset.bound = '1';
        btn.addEventListener('click', function () {
            // بارگذاری کامل صفحه. دلیل انتخاب این و نه «اجرای دوباره مراحل»:
            // اگر خود Blazor بالا نیامده باشد، هیچ کد سی‌شارپی برای شنیدن
            // این کلیک وجود ندارد؛ بارگذاری مجدد در هر دو حالت کار می‌کند.
            window.location.reload();
        });
    }

    function init() {
        var nodes = document.querySelectorAll('.s-splash');
        if (!nodes.length) return false;

        measure();
        instances = [];

        Array.prototype.forEach.call(nodes, function (node) {
            var inst = makeInstance(node);
            if (!inst) return;

            instances.push(inst);
            render(inst);
            bindRetry(node);

            // مقدار شروع از data-progress می‌آید. در اپ این ویژگی وجود ندارد
            // و پیشرفت صفر می‌ماند تا مراحل واقعی آن را بالا ببرند. در
            // پیش‌نمایش، هر نمونه با یک عدد ثابت به حرکت درمی‌آید.
            var seed = node.getAttribute('data-progress');
            if (seed !== null) setTarget(inst, seed);
        });

        if (!instances.length) return false;

        var main = primary();

        // هم قفل اسکرول و هم فاز اول و هم نگهبان، فقط در «حالت زنده» فعال
        // می‌شوند؛ یعنی جایی که این صفحه، پوشاننده واقعی نرم‌افزار است. در
        // پیش‌نمایش که چند نمونه کنار هم روی یک صفحه می‌نشینند، قفل اسکرول
        // فقط صفحه پیش‌نمایش را بی‌استفاده می‌کرد.
        if (main && main.live) {
            lockScroll(true);

            // فاز اول از همین‌جا خودکار شروع می‌شود، چون دانلود فایل‌های
            // Blazor پیش از اجرای دات‌نت اتفاق می‌افتد.
            watchBlazorLoad(BOOT_CAP);
            armWatchdog(WATCHDOG_MS);
        }

        return true;
    }

    /** نمونه اصلی همان است که شناسه solon-splash دارد. */
    function primary() {
        for (var i = 0; i < instances.length; i++) {
            if (instances[i].root.id === 'solon-splash') return instances[i];
        }
        return instances[0] || null;
    }

    /* ---------- فاز اول: پیشرفت واقعی خود Blazor ---------- */

    /** خواندن --blazor-load-percentage و نگاشت آن به سقف فاز اول. */
    function readBlazorLoad(cap) {
        var raw = '';
        try {
            raw = getComputedStyle(document.documentElement)
                .getPropertyValue('--blazor-load-percentage') || '';
        } catch (e) { return; }

        var num = parseFloat(String(raw).replace('%', '').trim());
        if (!isFinite(num)) return;

        lastRealUpdate = now();

        var inst = primary();
        if (inst) setTarget(inst, (num / 100) * cap);
    }

    /**
     * خزیدن محدود: اگر گزارش تازه‌ای نرسیده باشد، آرام به سقف نزدیک می‌شویم.
     * این یک تایمر تقلبی نیست — هیچ‌وقت از سقف فاز اول رد نمی‌شود و هرگز
     * عدد ۱۰۰ را نشان نمی‌دهد؛ فقط از حس «یخ‌زدگی» جلوگیری می‌کند.
     */
    function startCreep(cap) {
        if (creepTimer) return;

        var ceiling = cap * CREEP_CEILING;

        creepTimer = setInterval(function () {
            var inst = primary();
            if (!inst || inst.done || inst.failed) return;
            if (now() - lastRealUpdate < CREEP_AFTER_MS) return;
            if (inst.target >= ceiling) return;

            setTarget(inst, Math.min(inst.target + CREEP_STEP, ceiling));
        }, 250);
    }

    function watchBlazorLoad(cap) {
        if (loadObserver || typeof MutationObserver === 'undefined') return;

        var limit = isFinite(Number(cap)) && Number(cap) > 0 ? Number(cap) : BOOT_CAP;

        lastRealUpdate = now();
        readBlazorLoad(limit);               // شاید مقدار از قبل ست شده باشد

        // چرا MutationObserver و نه خواندن دوره‌ای: Blazor این متغیر را
        // به‌عنوان style درون‌خطی روی ریشه سند می‌نویسد. با رصد تغییر ویژگی
        // style، هر به‌روزرسانی بدون هیچ نظرسنجی دوره‌ای گرفته می‌شود.
        loadObserver = new MutationObserver(function () { readBlazorLoad(limit); });
        loadObserver.observe(document.documentElement, {
            attributes: true,
            attributeFilter: ['style']
        });

        startCreep(limit);
    }

    /** پایان فاز اول. BootService در شروع فاز دوم این را صدا می‌زند. */
    function stopWatching() {
        if (loadObserver) {
            loadObserver.disconnect();
            loadObserver = null;
        }
        if (creepTimer) {
            clearInterval(creepTimer);
            creepTimer = null;
        }
    }

    /* ---------- نگهبان ---------- */

    /**
     * اگر پس از این مدت نه پایان بیاید و نه خطا، یعنی خود Blazor بالا نیامده
     * (مثلاً فایلش دانلود نشده). در آن حالت هیچ کد سی‌شارپی وجود ندارد که
     * این را گزارش کند، پس پیام خطا از همین‌جا می‌آید.
     */
    function armWatchdog(ms) {
        if (watchdogTimer) return;

        watchdogTimer = setTimeout(function () {
            var inst = primary();
            if (!inst || inst.done || inst.failed) return;

            fail('بارگذاری نرم‌افزار کامل نشد. اتصال اینترنت را بررسی کنید و '
                + 'دوباره تلاش کنید.');
        }, ms);
    }

    function disarmWatchdog() {
        if (watchdogTimer) {
            clearTimeout(watchdogTimer);
            watchdogTimer = null;
        }
    }

    /* ---------- کمکی‌های بارگذاری واقعی ---------- */

    /**
     * پیش‌بارگذاری تصویر. همیشه resolve می‌شود (هرگز reject) تا سمت سی‌شارپ
     * به try/catch نیاز نداشته باشد: true یعنی واقعاً بار شد، false یعنی
     * خطا یا گذشت زمان.
     */
    function preloadImage(url, timeoutMs) {
        return new Promise(function (resolve) {
            if (!url) { resolve(false); return; }

            var settled = false;
            var timer = setTimeout(function () {
                if (settled) return;
                settled = true;
                resolve(false);
            }, timeoutMs || 6000);

            var img = new Image();
            img.onload = function () {
                if (settled) return;
                settled = true;
                clearTimeout(timer);
                resolve(true);
            };
            img.onerror = function () {
                if (settled) return;
                settled = true;
                clearTimeout(timer);
                resolve(false);
            };
            img.src = url;
        });
    }

    /**
     * انتظار برای آماده شدن فونت.
     * document.fonts.load وقتی فونت واقعاً بار شده باشد آرایه‌ای با حداقل یک
     * عضو می‌دهد و وقتی خانواده پیدا نشود آرایه خالی. پس مقدار بازگشتی
     * معنادار است، نه فقط «تایم‌اوت نشد».
     */
    function awaitFont(spec, timeoutMs) {
        return new Promise(function (resolve) {
            if (!document.fonts || typeof document.fonts.load !== 'function') {
                resolve(false);
                return;
            }

            var settled = false;
            var timer = setTimeout(function () {
                if (settled) return;
                settled = true;
                resolve(false);
            }, timeoutMs || 6000);

            document.fonts.load(spec || '1rem Vazirmatn').then(function (faces) {
                if (settled) return;
                settled = true;
                clearTimeout(timer);
                resolve(!!faces && faces.length > 0);
            }, function () {
                if (settled) return;
                settled = true;
                clearTimeout(timer);
                resolve(false);
            });
        });
    }

    /* ---------- سطح عمومی ---------- */

    function setProgress(value) {
        if (!instances.length && !init()) return;

        var inst = primary();
        if (!inst) return;

        setTarget(inst, value);
    }

    function getProgress() {
        var inst = primary();
        return inst ? inst.shown : 0;
    }

    /**
     * نشاندن اطلاعات برند از تنظیمات سالن.
     * همه فیلدها اختیاری‌اند و اگر نیایند، همان مقادیر پیش‌فرض داخل HTML سر
     * جای خودشان می‌مانند. پس هرگز متن خالی یا جای خالی دیده نمی‌شود.
     */
    function setBrand(o) {
        var inst = primary();
        if (!inst || !o) return;

        var root = inst.root;
        var nameEl = root.querySelector('[data-brand]');
        var latinEl = root.querySelector('[data-brand-latin]');
        var logoEl = root.querySelector('[data-logo]');
        var node;

        if (o.name && nameEl) {
            nameEl.textContent = o.name;
            try {
                document.title = o.latin ? (o.name + ' | ' + o.latin) : o.name;
            } catch (e) { }
        }
        if (o.latin && latinEl) latinEl.textContent = o.latin;

        // لوگو اگر از API آمد جای نام لاتین را می‌گیرد. تا وقتی تصویر واقعاً
        // بار نشده، hidden برداشته نمی‌شود؛ با onload این کار انجام می‌گیرد
        // تا هرگز کادر خالی یا آیکون تصویر شکسته دیده نشود.
        if (o.logo && logoEl) {
            node = logoEl;
            node.addEventListener('load', function () {
                node.hidden = false;
                if (latinEl) latinEl.hidden = true;
            });
            node.addEventListener('error', function () {
                node.hidden = true;          // آدرس خراب بود: مونوگرام متنی می‌ماند
                if (latinEl) latinEl.hidden = false;
            });
            node.src = o.logo;
        }
    }

    /**
     * پایان واقعی: محو شدن نرم و کنار رفتن لایه.
     * یک Promise برمی‌گرداند که پس از کامل شدن محو شدن resolve می‌شود، تا
     * سمت سی‌شارپ بتواند اگر لازم بود منتظر بماند.
     */
    function finish() {
        var inst = primary();

        if (!inst || inst.done || inst.failed) {
            return Promise.resolve(false);
        }

        inst.done = true;
        disarmWatchdog();
        stopWatching();
        if (inst.raf !== null) { cancelAnimationFrame(inst.raf); inst.raf = null; }

        inst.shown = inst.target = 1;
        render(inst);

        // پاداش زمانی: هیچ‌چیز پیش از این مقدار ظاهر نمی‌شود. اگر بارگذاری
        // سریع تمام شده باشد، باقی‌مانده اینجا صبر می‌شود.
        var wait = Math.max(0, (t0 + MIN_VISIBLE_MS) - now());

        return new Promise(function (resolve) {
            setTimeout(function () { doFinish(inst, resolve); }, wait);
        });
    }

    function doFinish(inst, resolve) {
        var root = inst.root;
        var settled = false;

        var done = function () {
            if (settled) return;
            settled = true;
            root.hidden = true;              // کاملاً از دسترس و از دید خارج می‌شود
            lockScroll(false);               // اسکرول صفحه آزاد می‌شود
            resolve(true);
        };

        root.classList.add('is-done');

        // هم transitionend و هم یک زمان‌سنج ایمنی: اگر زبانه در پس‌زمینه باشد
        // گذار ممکن است رویداد ندهد و لایه روی صفحه بماند.
        root.addEventListener('transitionend', done, { once: true });
        setTimeout(done, 900);
    }

    /** خطا: نماد همان‌جا خشک می‌شود و پیام ساده فارسی می‌آید. */
    function fail(msg) {
        var inst = primary();
        if (!inst || inst.done || inst.failed) return false;

        inst.failed = true;
        disarmWatchdog();
        stopWatching();
        if (inst.raf !== null) { cancelAnimationFrame(inst.raf); inst.raf = null; }

        lockScroll(false);                   // در حالت خطا هم اسکرول آزاد می‌شود

        var root = inst.root;
        var box = root.querySelector('[data-error]');
        var txt = root.querySelector('[data-error-msg]');

        if (txt && msg) txt.textContent = msg;
        if (box) box.hidden = false;

        root.classList.add('is-failed');
        return true;
    }

    /* ---------- راه‌اندازی ---------- */

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    window.solonSplash = {
        init: init,
        setProgress: setProgress,
        getProgress: getProgress,
        setBrand: setBrand,
        watchBlazorLoad: watchBlazorLoad,
        stopWatching: stopWatching,
        preloadImage: preloadImage,
        awaitFont: awaitFont,
        finish: finish,
        fail: fail,
        toPersianDigits: toPersianDigits,
        elapsedMs: function () { return now() - t0; }
    };
})();
