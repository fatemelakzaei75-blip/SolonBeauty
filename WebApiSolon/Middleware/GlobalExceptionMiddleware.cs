using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DataCore.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApiSolon.Middleware;

/// <summary>
/// میدل‌ور متمرکز مدیریت خطاهای سراسری (Global Exception Handling)
/// جلوگیری از افشای اطلاعات حساس سیستم و تولید پاسخ‌های استاندارد ApiResponse
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate _next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        this._next = _next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای مدیریت نشده در مسیر {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ArgumentNullException or ArgumentException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            KeyNotFoundException => HttpStatusCode.NotFound,
            InvalidOperationException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var userMessage = statusCode switch
        {
            HttpStatusCode.BadRequest => "درخواست ارسال شده نامعتبر است.",
            HttpStatusCode.Unauthorized => "عدم دسترسی احراز هویت.",
            HttpStatusCode.NotFound => "رکورد یا منبع مورد نظر یافت نشد.",
            HttpStatusCode.Conflict => "تداخل در انجام عملیات درخواستی.",
            _ => "خطای داخلی در سرور رخ داده است. لطفاً بعداً تلاش فرمایید."
        };

        var errors = _env.IsDevelopment() ? exception.Message : null;

        var response = ApiResponse.ErrorResult(userMessage, errors);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
