using System;
using System.Globalization;

namespace DataCore.Helpers
{
    /// <summary>
    /// تبدیل و قالب‌بندی تاریخ‌های میلادی به شمسی و برعکس (بند ۷ سند نیازمندی‌ها)
    /// کلیه تاریخ‌ها در دیتابیس به صورت DateTime ذخیره و در واسط کاربری به شمسی نمایش داده می‌شوند.
    /// </summary>
    public static class PersianDateHelper
    {
        private static readonly PersianCalendar _pc = new PersianCalendar();

        private static readonly string[] _monthNames =
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

        private static readonly string[] _dayOfWeekNames =
        {
            "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه"
        };

        /// <summary>قالب استاندارد: ۱۴۰۳/۰۷/۰۴</summary>
        public static string ToShamsi(DateTime dt, bool includeTime = false)
        {
            if (dt == default || dt == DateTime.MinValue) return "-";
            int y = _pc.GetYear(dt);
            int m = _pc.GetMonth(dt);
            int d = _pc.GetDayOfMonth(dt);
            var dateStr = $"{y:0000}/{m:00}/{d:00}";
            return includeTime ? $"{dateStr} {dt:HH:mm}" : dateStr;
        }

        /// <summary>قالب تفصیلی: چهارشنبه ۴ مهر ۱۴۰۳</summary>
        public static string ToShamsiLong(DateTime dt)
        {
            if (dt == default || dt == DateTime.MinValue) return "-";
            int y = _pc.GetYear(dt);
            int m = _pc.GetMonth(dt);
            int d = _pc.GetDayOfMonth(dt);
            string dow = _dayOfWeekNames[(int)dt.DayOfWeek];
            string mName = _monthNames[m - 1];
            return $"{dow} {d} {mName} {y}";
        }

        /// <summary>تبدیل سال/ماه/روز شمسی به DateTime میلادی</summary>
        public static DateTime ToMiladi(int year, int month, int day, int hour = 0, int minute = 0)
        {
            return _pc.ToDateTime(year, month, day, hour, minute, 0, 0);
        }

        /// <summary>تجزیه رشته شمسی (مانند 1403/07/04) به DateTime میلادی</summary>
        public static bool TryParseShamsi(string? text, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim().Replace("-", "/").Replace(".", "/");
            var parts = text.Split('/');
            if (parts.Length != 3) return false;

            if (int.TryParse(parts[0], out int y) &&
                int.TryParse(parts[1], out int m) &&
                int.TryParse(parts[2], out int d))
            {
                try
                {
                    result = _pc.ToDateTime(y, m, d, 12, 0, 0, 0);
                    return true;
                }
                catch { return false; }
            }
            return false;
        }

        public static (int Year, int Month, int Day) GetShamsiParts(DateTime dt)
        {
            return (_pc.GetYear(dt), _pc.GetMonth(dt), _pc.GetDayOfMonth(dt));
        }

        public static string GetMonthName(int month) => (month >= 1 && month <= 12) ? _monthNames[month - 1] : "";

        public static string[] GetMonthNames() => (string[])_monthNames.Clone();
    }
}
