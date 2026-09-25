using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class NotificationViewModel
{
    public Guid Tc { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string TargetRole { get; set; } = "All";

    public Guid? TargetUserTc { get; set; }

    public string NotificationType { get; set; } = "Info";

    public bool IsRead { get; set; }

    public string? ActionUrl { get; set; }

    public DateTime RegisterData { get; set; }
}

public class NotificationCreateDto
{
    [Required(ErrorMessage = "عنوان اعلان الزامی است")]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "متن اعلان الزامی است")]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public string TargetRole { get; set; } = "All";

    public Guid? TargetUserTc { get; set; }

    public string NotificationType { get; set; } = "Info";

    public string? ActionUrl { get; set; }
}
