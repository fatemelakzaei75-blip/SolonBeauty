using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class SalonViewModel
{
    public Guid Tc { get; set; }

    public string Phone1 { get; set; } = string.Empty;

    public string? Phone2 { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public string? Logo { get; set; }

    public bool IsActive { get; set; }

    public DateTime RegisterData { get; set; }
}

public class SalonCreateOrUpdateDto
{
    [Required(ErrorMessage = "شماره تماس اول الزامی است")]
    public string Phone1 { get; set; } = string.Empty;

    public string? Phone2 { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public string? Logo { get; set; }
}
