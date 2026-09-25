using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class SalonServiceViewModel
{
    public Guid Tc { get; set; }

    public Guid SalonTC { get; set; }

    public Guid CategoryTC { get; set; }

    public string? CategoryName { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool Main { get; set; }

    public string? PortfolioSample { get; set; }

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class SalonServiceCreateDto
{
    public Guid SalonTC { get; set; }

    [Required(ErrorMessage = "شناسه دسته‌بندی الزامی است")]
    public Guid CategoryTC { get; set; }

    [Required(ErrorMessage = "نام سرویس الزامی است")]
    [MaxLength(150, ErrorMessage = "نام سرویس نمی‌تواند بیشتر از ۱۵۰ کاراکتر باشد")]
    public string ServiceName { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "قیمت الزامی است")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
    public decimal Price { get; set; }

    public bool Main { get; set; }

    public string? PortfolioSample { get; set; }
}

public class SalonServiceUpdateDto
{
    [Required(ErrorMessage = "شناسه دسته‌بندی الزامی است")]
    public Guid CategoryTC { get; set; }

    [Required(ErrorMessage = "نام سرویس الزامی است")]
    [MaxLength(150, ErrorMessage = "نام سرویس نمی‌تواند بیشتر از ۱۵۰ کاراکتر باشد")]
    public string ServiceName { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "قیمت الزامی است")]
    [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
    public decimal Price { get; set; }

    public bool Main { get; set; }

    public string? PortfolioSample { get; set; }

    public bool IsActive { get; set; } = true;
}
