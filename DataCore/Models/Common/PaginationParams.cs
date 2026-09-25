using System;
using System.Collections.Generic;

namespace DataCore.Models.Common;

/// <summary>
/// پارامترهای استاندارد درخواست صفحه‌بندی سمت بک‌اند
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;

    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
    }

    public string? SearchTerm { get; set; }

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;
}
