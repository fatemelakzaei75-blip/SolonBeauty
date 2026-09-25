using System.ComponentModel.DataAnnotations;

namespace BlazorAppSolon.Auth;

/// <summary>نقش های کاربری سیستم</summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Personnel = "Personnel";
    public const string Customer = "Customer";

    public static string Persian(string role) => role switch
    {
        Admin => "مدیر سیستم",
        Personnel => "پرسنل",
        Customer => "مشتری",
        _ => role
    };

    /// <summary>مسیر پنل هر نقش</summary>
    public static string PanelUrl(string role) => role switch
    {
        Admin => "panel/admin",
        Personnel => "panel/personnel",
        _ => "panel/customer"
    };
}

public class SendCodeRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;
}

public class LoginRequest
{
    // الگو پس از تبدیل ارقام فارسی/عربی در صفحه ورود بررسی می شود
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید الزامی است")]
    public string Code { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public Guid Tc { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class LoginPasswordRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید الزامی است")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
    public string NewPassword { get; set; } = string.Empty;
}
