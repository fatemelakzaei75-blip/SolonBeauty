using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class DiscountViewModel
{
    public Guid Tc { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Percent { get; set; }

    public decimal MaxDiscount { get; set; }

    public decimal MinPurchase { get; set; }

    public DateTime? ExpireDate { get; set; }

    public int UsageLimit { get; set; }

    public int UsedCount { get; set; }

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class DiscountCreateDto
{
    [Required(ErrorMessage = "کد تخفیف الزامی است")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "عنوان تخفیف الزامی است")]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "درصد تخفیف باید بین ۱ تا ۱۰۰ باشد")]
    public int Percent { get; set; }

    public decimal MaxDiscount { get; set; }

    public decimal MinPurchase { get; set; }

    public DateTime? ExpireDate { get; set; }

    public int UsageLimit { get; set; }
}

public class DiscountValidationResultDto
{
    public bool Valid { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Percent { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }
}
