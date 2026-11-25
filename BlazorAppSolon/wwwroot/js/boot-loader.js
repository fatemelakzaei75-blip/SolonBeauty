/* ==========================================================================
   SolonBeauty — لودر زیرنظر بارگذاری Blazor (پیشرفت بایت‌به‌بایت)
   --------------------------------------------------------------------------
   چرا این فایل وجود دارد:
   موتور بوت دات‌نت پیشرفت دانلود را فقط «به‌ازای هر فایل» گزارش می‌کند و
   متغیر --blazor-load-percentage آن می‌تواند زودتر از موعد جهش کند (مخرج
   کسر در حین کار بزرگ‌تر می‌شود). نتیجه روی اینترنت واقعی: نوار وسط راه
   (مثلاً روی ۳۷٪) قفل می‌شود و دیگر تکان نمی‌خورد. اینجا با
   loadBootResource همه دانلودها را خودمان انجام می‌دهیم و هر بایت را
   گزارش می‌کنیم:

       ۱) پیشرفت واقعی و نرم، حتی داخل فایل‌های بزرگ
       ۲) نگهبان سراسری: اگر مجموع همه دانلودها ۴۵ ثانیه حتی یک بایت
          نگیرند (اتصال مرده)، پیام روشن + دکمه تلاش دوباره — نه یخ‌زدگی
       ۳) تمدید نگهبان splash با هر بایت: اتصال کند هرگز کشته نمی‌شود
       ۴) پس از پایان دانلودها، مهلت سخاوتمندانه برای «فاز خاموش»
          (کامپایل WASM و راه‌اندازی runtime) که هیچ رویدادی ندارد

   قرارداد: window.Blazor.start({ loadBootResource }) — جز برای نوع
   'dotnetjs' که باید رشته URL بدهیم یا هیچ (تا مسیر پیش‌فرض برود).
   ========================================================================== */

(function () {
    'use strict';

    // ---------- وضعیت ----------
    var pending = 0;          // دانلودهای در جریان
    var unitsDone = 0;        // مجموع کسرهای تکمیل‌شده (هر فایل از ۰ تا ۱)
    var unitsQueued = 0;      // تعداد فایل‌های دیده‌شده
    var expectedTotal = 0;    // کل واقعی فایل‌ها از blazor.boot.json
    var lastByteAt = 0;       // آخرین بایت دریافتی از «هر» دانلودی
    var granted = false;      // فاز خاموش (کامپایل) شروع شده
    var failed = false;

    var GLOBAL_IDLE_MS = 45000;      // کل دانلودها ۴۵ ثانیه بی‌بایت = مرده
    var QUIET_MS = 2000;             // سکوت پس از آخرین فعالیت = فاز خاموش
    var COMPILE_GRANT_MS = 120000;   // مهلت کامپایل WASM + راه‌اندازی

    function splash() { return window.solonSplash || null; }

    function touch() {
        lastByteAt = Date.now();
        var s = splash();
        if (s && s.touch) s.touch();
    }

    /**
     * مخرج کسر پیشرفت. هرگز «صف دیده‌شده» تنها نیست: صف موتور بوت در حین
     * کار بزرگ‌تر می‌شود و اگر مخرج همان صف لحظه‌ای باشد، کسر می‌تواند
     * زودتر از موعد به ۱ برسد و قیف یک‌نوا آن جهش را برای همیشه قفل کند
     * (دقیقاً همان اشکال معروف --blazor-load-percentage). پس مخرج بزرگ‌ترین
     * مقدار میان «صف لحظه‌ای» و «کل واقعی از blazor.boot.json» است.
     */
    function denominator() {
        return Math.max(unitsQueued, expectedTotal);
    }

    function report() {
        var s = splash();
        var denom = denominator();
        if (s && s.setBootUnits && denom > 0) {
            s.setBootUnits(Math.min(unitsDone, denom), denom);
        }
    }

    /**
     * کل فایل‌های قابل‌دانلود را از blazor.boot.json می‌شماریم. این عدد
     * عمداً کمی بزرگ‌تر از تعداد فراخوانی‌های واقعی لودر است (ماژول‌های JS
     * از مسیر لودر نمی‌آیند و بعضی تک‌فایل‌ها هم شاید نیایند) — بزرگ‌تر
     * بودن مخرج یعنی هرگز جهش زودهنگام به سقف نخواهیم داشت.
     */
    function learnExpectedTotal(bootJson) {
        try {
            var res = (bootJson && bootJson.resources) || {};
            var n = 0;
            ['assembly', 'pdb', 'lazyAssembly'].forEach(function (k) {
                if (res[k] && typeof res[k] === 'object') n += Object.keys(res[k]).length;
            });
            ['icu', 'wasmNative', 'jsModuleNative', 'jsModuleRuntime'].forEach(function (k) {
                if (res[k] && typeof res[k] === 'object' && Object.keys(res[k]).length) n += Object.keys(res[k]).length;
            });
            expectedTotal = n;
            report();
        } catch (e) { /* بی‌اهمیت */ }
    }

    function failOnce(msg) {
        if (failed) return;
        failed = true;
        var s = splash();
        if (s) s.fail(msg);
    }

    /**
     * fetch زیرنظر: پیشرفت بایتی + گزارش به نوار.
     * همیشه Promise<Response> برمی‌گرداند (یا reject می‌کند).
     */
    function trackedFetch(url, name) {
        pending++;
        unitsQueued++;
        report();
        touch();

        var fraction = 0;
        var controller = typeof AbortController !== 'undefined' ? new AbortController() : null;

        function unitDone(delta) {
            if (delta <= 0) return;
            unitsDone += delta;
            report();
            touch();
        }

        return fetch(url, {
            cache: 'no-cache',
            credentials: 'same-origin',
            signal: controller ? controller.signal : undefined
        }).then(function (res) {
            if (!res.ok) throw new Error('HTTP ' + res.status + ' — ' + name);

            var total = parseInt(res.headers.get('content-length') || '0', 10);

            // مرورگر بدون stream: همان پاسخ را برگردان و فایل را کامل حساب کن
            if (!res.body || typeof res.body.getReader !== 'function') {
                unitDone(1 - fraction);
                pending--;
                return res;
            }

            var reader = res.body.getReader();
            var received = 0;

            var stream = new ReadableStream({
                start: function (ctrl) {
                    (function pump() {
                        return reader.read().then(function (r) {
                            if (r.done) {
                                unitDone(1 - fraction);   // تضمین تکمیل تا ۱
                                pending--;
                                touch();
                                ctrl.close();
                                return;
                            }
                            received += r.value.byteLength;
                            touch();

                            if (total > 0) {
                                var want = Math.min(1, received / total);
                                if (want > fraction) {
                                    unitDone(want - fraction);
                                    fraction = want;
                                }
                            }

                            ctrl.enqueue(r.value);
                            return pump();
                        }, function (e) {
                            pending--;
                            ctrl.error(e);
                            failOnce('دانلود فایل‌های برنامه ناقص ماند. دوباره تلاش کنید.');
                        });
                    })();
                },
                cancel: function (reason) {
                    try { reader.cancel(reason); } catch (e) { }
                }
            });

            return new Response(stream, {
                status: res.status,
                statusText: res.statusText,
                headers: res.headers
            });
        }).catch(function (e) {
            // اگر failOnce قبلاً صدا شده، فقط رد می‌کنیم
            if (!failed) {
                pending--;
                failOnce('دریافت فایل‌های برنامه از سرور ممکن نشد. '
                    + 'اتصال خود را بررسی کنید و دوباره تلاش کنید.');
            }
            throw e;
        });
    }

    /** لودر سفارشی Blazor */
    function loadBootResource(type, name, defaultUri) {
        // ماژول‌های JS باید URL بدهند؛ مسیر پیش‌فرض را بگیر
        if (type === 'dotnetjs') return undefined;
        return trackedFetch(defaultUri, name || type);
    }

    /**
     * نگهبان سراسری + تشخیص «فاز خاموش»:
     * - اگر در مجموع هیچ بایتی برای GLOBAL_IDLE_MS نرسد ← خطای روشن.
     * - اگر دانلودی در جریان نباشد و چند ثانیه سکوت باشد ← فاز کامپایل
     *   شروع شده: نوار روی سقف فاز اول می‌ماند و نگهبان مهلت بلند می‌گیرد.
     */
    setInterval(function () {
        if (failed) return;

        var idle = Date.now() - lastByteAt;

        if (!granted) {
            if (unitsQueued > 0 && idle > GLOBAL_IDLE_MS) {
                failOnce('دریافت فایل‌های برنامه از سرور کامل نشد. '
                    + 'اتصال اینترنت خود را بررسی کنید و دوباره تلاش کنید.');
                return;
            }
            if (pending === 0 && unitsQueued > 0 && idle > QUIET_MS) {
                granted = true;
                var s = splash();
                if (!s) return;
                if (s.setBootUnits) {
                    var denom = denominator();
                    s.setBootUnits(Math.min(unitsDone, denom), denom);
                }
                if (s.grant) s.grant(COMPILE_GRANT_MS);
            }
        }
    }, 500);

    /** شروع Blazor با لودر سفارشی */
    function start() {
        // نخست: شمارش کل فایل‌ها از blazor.boot.json (برای مخرج درست کسر).
        // شکست این خواندن مهلت‌دار است و هرگز شروع را به تأخیر نمی‌اندازد.
        fetch('_framework/blazor.boot.json', { cache: 'no-cache' })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(learnExpectedTotal)
            .catch(function (e) { void e; });

        var B = window.Blazor;
        if (!B || typeof B.start !== 'function') {
            // هنوز نیامده؛ تا DOM آماده شود صبر کن
            document.addEventListener('DOMContentLoaded', start);
            return;
        }
        try {
            var p = B.start({ loadBootResource: loadBootResource });
            if (p && typeof p.catch === 'function') {
                p.catch(function (e) {
                    console.error(e);
                    failOnce('راه‌اندازی نرم‌افزار کامل نشد. صفحه را نوسازی کنید '
                        + 'یا دوباره تلاش کنید.');
                });
            }
        } catch (e) {
            console.error(e);
            failOnce('راه‌اندازی نرم‌افزار کامل نشد. صفحه را نوسازی کنید یا دوباره تلاش کنید.');
        }
    }

    start();
})();
