using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class PortfolioViewModel
{
    public Guid Tc { get; set; }

    public Guid SampleTC { get; set; }

    public Guid PersonalTC { get; set; }

    public string? PersonalName { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }

    public int LikesCount { get; set; }

    public int ViewsCount { get; set; }

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class PortfolioCreateDto
{
    public Guid SampleTC { get; set; }

    public Guid PersonalTC { get; set; }

    [Required(ErrorMessage = "عنوان نمونه‌کار الزامی است")]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }
}

public class PortfolioUpdateDto
{
    public Guid SampleTC { get; set; }

    public Guid PersonalTC { get; set; }

    [Required(ErrorMessage = "عنوان نمونه‌کار الزامی است")]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CategoryPortfolioViewModel
{
    public Guid Tc { get; set; }

    public Guid SalonTC { get; set; }

    public string CategoryNameSample { get; set; } = string.Empty;

    public string SamplePic { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class CategoryPortfolioCreateDto
{
    public Guid SalonTC { get; set; }

    [Required(ErrorMessage = "نام دسته‌بندی نمونه‌کار الزامی است")]
    [MaxLength(150)]
    public string CategoryNameSample { get; set; } = string.Empty;

    [Required(ErrorMessage = "تصویر نمونه الزامی است")]
    public string SamplePic { get; set; } = string.Empty;
}
