using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class WaitingListViewModel
{
    public Guid Tc { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string? PersonalName { get; set; }

    public string? ServiceName { get; set; }

    public DateTime PreferredDate { get; set; }

    public string PreferredTimeShift { get; set; } = "10:00";

    public bool IsNotified { get; set; }

    public DateTime RegisterData { get; set; }
}

public class WaitingListCreateDto
{
    [Required(ErrorMessage = "نام متقاضی الزامی است")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره همراه الزامی است")]
    public string MobileNumber { get; set; } = string.Empty;

    public string? PersonalName { get; set; }

    public string? ServiceName { get; set; }

    [Required(ErrorMessage = "تاریخ مورد نظر الزامی است")]
    public DateTime PreferredDate { get; set; }

    public string PreferredTimeShift { get; set; } = "10:00";
}
