using System.ComponentModel.DataAnnotations;

namespace WebApiSolon.Models;

/// <summary>درخواست ارسال کد تأیید به موبایل</summary>
public class SendCodeRequest
{
    // اعتبارسنجی الگو پس از نرمال سازی ارقام فارسی/عربی در کنترلر انجام می شود
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;
}

/// <summary>ورود با موبایل و کد تأیید</summary>
public class LoginRequest
{
    // اعتبارسنجی الگو پس از نرمال سازی ارقام فارسی/عربی در کنترلر انجام می شود
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید الزامی است")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>ورود با موبایل و رمز عبور (روش دوم - بند ۱۳ سند)</summary>
public class LoginPasswordRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>درخواست بازیابی رمز عبور با OTP (بند ۱۳ سند)</summary>
public class ResetPasswordRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید الزامی است")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>پاسخ ورود موفق</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public Guid Tc { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class ApiResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
