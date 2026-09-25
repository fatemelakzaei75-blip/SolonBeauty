using System.Text;

namespace WebApiSolon.Services;

/// <summary>تبدیل ارقام فارسی/عربی به انگلیسی (برای ورودی موبایل و کد تأیید)</summary>
public static class DigitHelper
{
    private const string Persian = "۰۱۲۳۴۵۶۷۸۹";
    private const string Arabic = "٠١٢٣٤٥٦٧٨٩";

    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var sb = new StringBuilder(input.Length);
        foreach (var ch in input.Trim())
        {
            var pi = Persian.IndexOf(ch);
            var ai = Arabic.IndexOf(ch);

            if (pi >= 0) sb.Append((char)('0' + pi));
            else if (ai >= 0) sb.Append((char)('0' + ai));
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_' or '‌') continue;
            else sb.Append(ch);
        }
        return sb.ToString();
    }
}
