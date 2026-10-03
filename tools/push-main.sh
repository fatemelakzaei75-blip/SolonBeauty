#!/usr/bin/env bash
# ==========================================================================
# ارسال کار صفحه آغاز به main — یک عملیات اتمی
# --------------------------------------------------------------------------
# چرا اتمی: در این محیط، متادیتای گیت (.git) بین اجراها چند بار به حالت
# قدیمی برگشته است. اگر fetch و commit و push در چند دستور جدا اجرا شوند،
# خطر این هست که وسط کار تاریخچه عقب برود و کامیت گم شود. پس همه‌چیز در یک
# اجرا انجام می‌گیرد.
#
# چرا به origin/main بازنشانی می‌شویم: کارهای پیشین (سایدبار، مایگریشن) روی
# گیت‌هاب نشسته‌اند ولی ممکن است در نسخه محلی نباشند. بازنشانی --mixed فقط
# اشاره‌گر و ایندکس را جلو می‌برد؛ هیچ فایلی از درخت کار حذف نمی‌شود. پس
# فایل‌های تازه سر جای خودشان می‌مانند و روی نسخه درست سوار می‌شوند.
#
# پیام کامیت:
#     از فایل tools/commit-message.txt خوانده می‌شود (فارسی، چند خطی). چرا فایل
#     و نه heredoc داخل اسکریپت: هر بار که کار تازه‌ای کامیت می‌شود، پیام باید
#     عوض شود؛ اگر داخل اسکریپت باشد، یا باید اسکریپت را دستی ویرایش کرد یا
#     پیام کار قبلی دوباره فرستاده می‌شود (یک‌بار همان اتفاق افتاد).
#
# توکن:
#     هرگز در فایل نوشته نمی‌شود و در آدرس ریموت ذخیره نمی‌شود؛ فقط با
#     متغیر محیطی GH_TOKEN خوانده و در همان یک دستور push مصرف می‌شود.
#     توجه: اگر توکن را روی خط فرمان بنویسید، در تاریخچه پوسته می‌ماند.
#     راه امن‌تر:  read -rs GH_TOKEN && export GH_TOKEN
#
# اجرا:
#     GH_TOKEN=... bash tools/push-main.sh
# ==========================================================================
set -euo pipefail

REMOTE="https://github.com/fatemelakzaei75-blip/SolonBeauty.git"
BRANCH="main"

if [ -z "${GH_TOKEN:-}" ]; then
    echo "❌ متغیر GH_TOKEN تنظیم نشده است." >&2
    echo "   راه امن:  read -rs GH_TOKEN && export GH_TOKEN" >&2
    exit 1
fi

cd "$(dirname "$0")/.."

echo "▸ ۱) پاک‌سازی خروجی‌های ساخت (تا وارد کامیت نشوند)"
rm -rf BlazorAppSolon/bin BlazorAppSolon/obj \
       DataCore/bin DataCore/obj DataLayer/bin DataLayer/obj \
       WebApiSolon/bin WebApiSolon/obj 2>/dev/null || true
find . -name __pycache__ -type d -exec rm -rf {} + 2>/dev/null || true

echo "▸ ۲) تنظیم ریموت (بدون توکن در آدرس)"
if git remote get-url origin >/dev/null 2>&1; then
    git remote set-url origin "$REMOTE"
else
    git remote add origin "$REMOTE"
fi
git config user.name  "Arena Agent"
git config user.email "agent@arena.ai"

echo "▸ ۳) واکشی وضعیت واقعی گیت‌هاب"
git fetch origin "$BRANCH"

echo "▸ ۴) نشستن روی آخرین وضعیت گیت‌هاب (فایل‌های محلی دست‌نخورده می‌مانند)"
git reset --mixed "origin/$BRANCH"

echo "▸ ۵) فایل‌های تازه"
git add -A

if git diff --cached --quiet; then
    echo "   هیچ تغییر تازه‌ای برای کامیت نیست؛ فقط push انجام می‌شود."
else
    git commit -F "${MSG_FILE:-tools/commit-message.txt}"
fi


echo "▸ ۶) ارسال به گیت‌هاب"
# توکن فقط در همین یک دستور مصرف می‌شود و هیچ‌جا ذخیره نمی‌شود.
# sed خروجی را از توکن پاک می‌کند تا در گزارش‌ها لو نرود.
git push "https://x-access-token:${GH_TOKEN}@github.com/fatemelakzaei75-blip/SolonBeauty.git" \
    "HEAD:refs/heads/${BRANCH}" 2>&1 | sed "s/${GH_TOKEN}/[TOKEN-REDACTED]/g"

echo ""
echo "✅ انجام شد."
git log --oneline -3
