using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

/// <summary>
/// مدل درخواست ارسال کد تأیید ورود
/// </summary>
public class SendCodeRequestDto
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "فرمت شماره موبایل باید ۱۱ رقمی و با ۰۹ شروع شود")]
    public string MobileNumber { get; set; } = string.Empty;
}

/// <summary>
/// مدل درخواست ورود با کد یکبار مصرف (OTP)
/// </summary>
public class LoginOtpRequestDto
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید الزامی است")]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// مدل درخواست ورود با رمز عبور ثابت
/// </summary>
public class LoginPasswordRequestDto
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// مدل درخواست بازیابی و تنظیم مجدد رمز عبور
/// </summary>
public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید الزامی است")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
    [MinLength(6, ErrorMessage = "رمز عبور جدید باید حداقل ۶ کاراکتر باشد")]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// مدل پاسخ احراز هویت و صدور توکن JWT
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public Guid Tc { get; set; }

    public List<string> Roles { get; set; } = new();
}
