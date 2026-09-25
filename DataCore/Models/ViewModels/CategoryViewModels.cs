using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class CategoryViewModel
{
    public Guid Tc { get; set; }

    public Guid SalonTc { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class CategoryCreateDto
{
    public Guid SalonTc { get; set; }

    [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
    [MaxLength(100, ErrorMessage = "نام دسته‌بندی نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد")]
    public string CategoryName { get; set; } = string.Empty;
}

public class CategoryUpdateDto
{
    [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
    [MaxLength(100, ErrorMessage = "نام دسته‌بندی نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد")]
    public string CategoryName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
