using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class CustomerViewModel
{
    public Guid Tc { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? MobileNumber { get; set; }

    public string? CodeMoaref { get; set; }

    public int? BirthdayCustomer { get; set; }

    public int? PicCustomer { get; set; }

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class CustomerCreateDto
{
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
    [MaxLength(100, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد")]
    public string FullName { get; set; } = string.Empty;

    public string? MobileNumber { get; set; }

    [MaxLength(50)]
    public string? CodeMoaref { get; set; }

    public int? BirthdayCustomer { get; set; }

    public int? PicCustomer { get; set; }
}

public class CustomerUpdateDto
{
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
    [MaxLength(100, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد")]
    public string FullName { get; set; } = string.Empty;

    public string? MobileNumber { get; set; }

    [MaxLength(50)]
    public string? CodeMoaref { get; set; }

    public int? BirthdayCustomer { get; set; }

    public int? PicCustomer { get; set; }

    public bool IsActive { get; set; } = true;
}
