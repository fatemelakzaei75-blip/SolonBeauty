using System;
using System.ComponentModel.DataAnnotations;

namespace DataCore.Models.ViewModels;

public class CommentViewModel
{
    public Guid Tc { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string? AuthorMobile { get; set; }

    public string Content { get; set; } = string.Empty;

    public int Rating { get; set; } = 5;

    public Guid? PersonalTC { get; set; }

    public Guid? ServiceTC { get; set; }

    public bool IsApproved { get; set; }

    public DateTime RegisterData { get; set; }
}

public class CommentCreateDto
{
    [Required(ErrorMessage = "نام ارسال‌کننده نظر الزامی است")]
    public string AuthorName { get; set; } = string.Empty;

    public string? AuthorMobile { get; set; }

    [Required(ErrorMessage = "متن نظر الزامی است")]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ ستاره باشد")]
    public int Rating { get; set; } = 5;

    public Guid? PersonalTC { get; set; }

    public Guid? ServiceTC { get; set; }
}
