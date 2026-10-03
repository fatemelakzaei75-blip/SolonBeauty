#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
منبع واحد آیکون‌های برداری صفحه ورود.

چرا این فایل هست: همان آیکون‌ها در دو جا لازم می‌شوند — در کامپوننت رِیزور
(AuthIcons.razor) که کاربر نهایی می‌بیند، و در پیش‌نمایشی که برای بازبینی چشمی
ساخته می‌شود. اگر هر کدام نسخه خودش را داشته باشد، دیر یا زود از هم می‌افتند.

مسیرها عیناً همان چیزی است که در AuthIcons.razor نوشته شده؛
tools/test-login.js رشته‌های مشترک را بین این دو مقایسه می‌کند.
"""

ICON_PATHS = {
    "phone": [
        '<rect x="7" y="2.5" width="10" height="19" rx="2.6" />',
        '<path d="M10.6 5.3h2.8" />',
        '<path d="M11.2 18.6h1.6" />',
    ],
    "lock": [
        '<rect x="4.6" y="10" width="14.8" height="10.4" rx="3" />',
        '<path d="M8.2 10V7.5a3.8 3.8 0 0 1 7.6 0V10" />',
        '<path d="M12 14.1v2.3" />',
    ],
    "eye": [
        '<path d="M2.3 12S5.7 5.7 12 5.7 21.7 12 21.7 12 18.3 18.3 12 18.3 2.3 12 2.3 12Z" />',
        '<circle cx="12" cy="12" r="3.1" />',
    ],
    "eye-off": [
        '<path d="M4 4l16 16" />',
        '<path d="M9.8 5.95A9.6 9.6 0 0 1 12 5.7c6.3 0 9.7 6.3 9.7 6.3a17.3 17.3 0 0 1-3.4 4.2" />',
        '<path d="M6.4 7.75A17.5 17.5 0 0 0 2.3 12S5.7 18.3 12 18.3c1.25 0 2.36-.19 3.35-.53" />',
        '<path d="M10.05 10.15a3.1 3.1 0 0 0 4.3 4.3" />',
    ],
    "alert": [
        '<circle cx="12" cy="12" r="9" />',
        '<path d="M12 7.7v5.1" />',
        '<circle cx="12" cy="16.2" r="1" fill="currentColor" stroke="none" />',
    ],
    "check": [
        '<circle cx="12" cy="12" r="9" />',
        '<path d="M8.2 12.3l2.6 2.6 5-5.2" />',
    ],
    "key": [
        '<circle cx="8.6" cy="15.4" r="3.6" />',
        '<path d="M11.2 12.8 19 5" />',
        '<path d="M16.4 7.6l1.8 1.8" />',
        '<path d="M18.9 5.1l1.8 1.8" />',
    ],
    "close": [
        '<path d="M6.4 6.4l11.2 11.2" />',
        '<path d="M17.6 6.4L6.4 17.6" />',
    ],
    "sparkle": [
        '<path d="M12 3.4c.8 3.9 2.3 5.4 6.2 6.2-3.9.8-5.4 2.3-6.2 6.2-.8-3.9-2.3-5.4-6.2-6.2 3.9-.8 5.4-2.3 6.2-6.2Z" />',
        '<path d="M17.6 15.4c.4 1.9 1.1 2.6 3 3-1.9.4-2.6 1.1-3 3-.4-1.9-1.1-2.6-3-3 1.9-.4 2.6-1.1 3-3Z" />',
    ],
}


def icon(name, cls=None, extra=""):
    """خروجی SVG یک آیکون، هم‌شکل با AuthIcons.razor"""
    klass = f' class="{cls}"' if cls else ""
    body = "\n    ".join(ICON_PATHS.get(name, []))
    return (
        f'<svg{klass} viewBox="0 0 24 24" fill="none" stroke="currentColor"\n'
        f'     stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"\n'
        f'     aria-hidden="true" focusable="false"{extra}>\n'
        f'    {body}\n'
        f'</svg>'
    )
