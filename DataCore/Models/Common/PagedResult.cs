using System;
using System.Collections.Generic;

namespace DataCore.Models.Common;

/// <summary>
/// مدل استاندارد خروجی صفحه‌بندی شده سمت دیتابیس و بک‌اند
/// </summary>
/// <typeparam name="T">نوع موجودیت یا ViewModel موجود در لیست</typeparam>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult()
    {
    }

    public PagedResult(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize > 0 ? pageSize : 10;
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        TotalPages = (int)Math.Ceiling(count / (double)PageSize);
        Items = items;
    }
}
