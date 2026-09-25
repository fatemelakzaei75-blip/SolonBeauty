using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class PersonalViewModel
{
    public Guid Tc { get; set; }

    public Guid SalonTc { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string PersonalCode { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public string Specialty { get; set; } = string.Empty;

    public string Line { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public string WorkingDays { get; set; } = string.Empty;

    public string WorkingHours { get; set; } = string.Empty;

    public bool IsApproved { get; set; }

    public DateTime BirthDate { get; set; }

    public Guid TC_Service { get; set; }

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class PersonalCreateDto
{
    public Guid SalonTc { get; set; }

    [Required(ErrorMessage = "نام پرسنل الزامی است")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [MaxLength(15)]
    public string Mobile { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Line { get; set; } = string.Empty;

    public int ExperienceYears { get; set; } = 3;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(100)]
    public string WorkingDays { get; set; } = "شنبه تا چهارشنبه";

    [MaxLength(100)]
    public string WorkingHours { get; set; } = "۰۹:۰۰ الی ۱۸:۰۰";

    public bool IsApproved { get; set; } = true;

    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-25);

    public Guid TC_Service { get; set; }
}

public class PersonalUpdateDto
{
    [Required(ErrorMessage = "نام پرسنل الزامی است")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [MaxLength(15)]
    public string Mobile { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Line { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(100)]
    public string WorkingDays { get; set; } = string.Empty;

    [MaxLength(100)]
    public string WorkingHours { get; set; } = string.Empty;

    public bool IsApproved { get; set; }

    public DateTime BirthDate { get; set; }

    public Guid TC_Service { get; set; }

    public bool IsActive { get; set; } = true;
}
