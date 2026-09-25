using System.Collections.Generic;
using DataCore.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>
/// کنترلر پایه برای تمام کنترلرهای RESTful سامانه سالن با پشتیبانی از استانداردهای HATEOAS و پاسخ یکپارچه
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> SuccessResponse<T>(
        T data,
        string message = "عملیات با موفقیت انجام شد",
        Dictionary<string, HateoasLink>? links = null)
    {
        var response = ApiResponse<T>.Ok(data, message);
        if (links is not null)
        {
            foreach (var (key, value) in links)
            {
                response.Links[key] = value;
            }
        }

        return Ok(response);
    }

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(
        string routeName,
        object? routeValues,
        T data,
        string message = "رکورد با موفقیت ایجاد شد",
        Dictionary<string, HateoasLink>? links = null)
    {
        var response = ApiResponse<T>.Created(data, message);
        if (links is not null)
        {
            foreach (var (key, value) in links)
            {
                response.Links[key] = value;
            }
        }

        return CreatedAtAction(routeName, routeValues, response);
    }

    protected ActionResult<ApiResponse<T>> ErrorResponse<T>(
        string message,
        int statusCode = 400,
        object? errors = null)
    {
        var response = ApiResponse<T>.Fail(message, errors);
        return StatusCode(statusCode, response);
    }

    protected ActionResult<ApiResponse<PagedResult<T>>> PagedResponse<T>(
        PagedResult<T> pagedResult,
        string endpointPath)
    {
        var response = ApiResponse<PagedResult<T>>.Ok(pagedResult, "فهرست با موفقیت دریافت شد");

        response.AddLink("self", $"{endpointPath}?pageNumber={pagedResult.PageNumber}&pageSize={pagedResult.PageSize}", "GET");

        if (pagedResult.HasNextPage)
        {
            response.AddLink("next", $"{endpointPath}?pageNumber={pagedResult.PageNumber + 1}&pageSize={pagedResult.PageSize}", "GET");
        }

        if (pagedResult.HasPreviousPage)
        {
            response.AddLink("prev", $"{endpointPath}?pageNumber={pagedResult.PageNumber - 1}&pageSize={pagedResult.PageSize}", "GET");
        }

        return Ok(response);
    }
}
