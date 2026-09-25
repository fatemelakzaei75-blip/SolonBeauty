using System.Collections.Generic;

namespace DataCore.Models.Common;

/// <summary>
/// مدل استاندارد و یکپارچه پاسخ API برای تمام لایه‌های پروژه
/// </summary>
/// <typeparam name="T">نوع داده بازگشتی در فیلد Data</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public object? Errors { get; set; }

    public Dictionary<string, HateoasLink> Links { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message = "عملیات با موفقیت انجام شد")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Created(T data, string message = "رکورد با موفقیت ایجاد شد")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(string message, object? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors
        };
    }

    public ApiResponse<T> AddLink(string rel, string href, string method = "GET")
    {
        Links[rel] = new HateoasLink(href, rel, method);
        return this;
    }
}

/// <summary>
/// مدل استاندارد پاسخ بدون داده اختصاصی
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResult(string message = "عملیات با موفقیت انجام شد")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Data = null,
            Errors = null
        };
    }

    public static ApiResponse ErrorResult(string message, object? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors
        };
    }
}
