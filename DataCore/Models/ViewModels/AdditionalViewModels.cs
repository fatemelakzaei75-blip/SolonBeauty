using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class FAQViewModel
{
    public Guid Tc { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public string Category { get; set; } = "عمومی";

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}

public class FAQCreateDto
{
    [Required(ErrorMessage = "متن پرسش الزامی است")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "متن پاسخ الزامی است")]
    public string Answer { get; set; } = string.Empty;

    public string Category { get; set; } = "عمومی";

    public int DisplayOrder { get; set; }
}

public class SalonSettingViewModel
{
    public Guid Tc { get; set; }

    public string SalonName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public string Slogan { get; set; } = string.Empty;

    public string AboutText { get; set; } = string.Empty;

    public string WorkingDays { get; set; } = string.Empty;

    public string WorkingHours { get; set; } = string.Empty;

    public string HolidaySchedule { get; set; } = string.Empty;

    public string PhonePrimary { get; set; } = string.Empty;

    public string PhoneSecondary { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string InstagramUrl { get; set; } = string.Empty;

    public string TelegramUrl { get; set; } = string.Empty;

    public string WhatsAppNumber { get; set; } = string.Empty;

    public decimal DefaultDepositAmount { get; set; }

    public bool VariablePriceWarningEnabled { get; set; }

    public string VariablePriceWarningText { get; set; } = string.Empty;

    public int CancelHoursLimit { get; set; }

    public int CancelPenaltyPercent { get; set; }

    public string ActiveTheme { get; set; } = "luxury";
}

public class NewsDayViewModel
{
    public Guid Tc { get; set; }

    public string Title { get; set; } = string.Empty;

    public string News { get; set; } = string.Empty;

    public string? Link { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime RegisterData { get; set; }
}

public class SocialViewModel
{
    public Guid Tc { get; set; }

    public string SocialMedia { get; set; } = string.Empty;

    public string AddressSocialMedia { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class RoleViewModel
{
    public Guid Tc { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class PermissionViewModel
{
    public Guid Tc { get; set; }

    public string PermissionName { get; set; } = string.Empty;
}

public class UserRoleDto
{
    public Guid PersonalTC { get; set; }

    public Guid RoleTC { get; set; }

    public string? RoleName { get; set; }
}

public class DashboardKpiDto
{
    public int TotalCustomers { get; set; }

    public int TodayReservations { get; set; }

    public int PendingReservations { get; set; }

    public int CompletedReservations { get; set; }

    public int CanceledReservations { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalRefunds { get; set; }

    public decimal NetIncome { get; set; }

    public int ActivePersonnel { get; set; }

    public int ActiveServices { get; set; }
}

public class ReportSummaryDto
{
    public string Period { get; set; } = "monthly";

    public int TotalCount { get; set; }

    public decimal TotalRevenue { get; set; }

    public List<TopServiceStatDto> TopServices { get; set; } = new();
}

public class TopServiceStatDto
{
    public string ServiceName { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal TotalAmount { get; set; }
}

public class StaffPerformanceDto
{
    public Guid PersonalTc { get; set; }

    public string StaffName { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public int TotalReservations { get; set; }

    public decimal TotalRevenue { get; set; }

    public double SatisfactionRating { get; set; }
}
