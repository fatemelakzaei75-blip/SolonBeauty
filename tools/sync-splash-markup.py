#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
هم‌گام‌سازی مارک‌آپ صفحه آغاز در index.html.

چرا این ابزار لازم است: مارک‌آپ صفحه آغاز هم در index.html لازم است و هم در
پیش‌نمایش. اگر هر کدام دستی نگه داشته شود، دیر یا زود ازهم می‌افتند و
پیش‌نمایش چیزی را نشان می‌دهد که در اپ نیست. پس هر دو از tools/splash_markup.py
می‌آیند و این ابزار بخش بین دو نشانگر را با نسخه تازه پر می‌کند.

اجرا:  python3 tools/sync-splash-markup.py            فقط وضعیت را گزارش می‌دهد
       python3 tools/sync-splash-markup.py --write    فایل را بازنویسی می‌کند
"""
import io
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from splash_markup import splash, START_MARKER, END_MARKER   # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
INDEX = os.path.join(ROOT, "BlazorAppSolon", "wwwroot", "index.html")
BLOCK = splash(progress=None, element_id="solon-splash", with_defs=True)


def main():
    write = "--write" in sys.argv

    with io.open(INDEX, encoding="utf-8") as f:
        html = f.read()

    if START_MARKER not in html or END_MARKER not in html:
        print("❌ نشانگرها در index.html پیدا نشد:", INDEX)
        return 1

    head, rest = html.split(START_MARKER, 1)
    _, tail = rest.split(END_MARKER, 1)

    current = rest.split(END_MARKER, 1)[0]
    in_sync = current.strip() == BLOCK.strip()

    print("وضعیت مارک‌آپ index.html:", "هم‌گام ✅" if in_sync else "ناهم‌گام ❌")
    print("  طول نسخه فایل  :", len(current.strip()))
    print("  طول نسخه منبع  :", len(BLOCK.strip()))

    if in_sync or not write:
        if not in_sync:
            print("برای بازنویسی: python3 tools/sync-splash-markup.py --write")
        return 0

    with io.open(INDEX, "w", encoding="utf-8") as f:
        f.write(head + START_MARKER + "\n    " + BLOCK + "\n    " + END_MARKER + tail)

    print("✅ بازنویسی شد:", INDEX)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
