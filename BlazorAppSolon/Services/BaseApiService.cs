using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;

namespace BlazorAppSolon.Services;

/// <summary>
/// سرویس پایه برای ارتباط استاندارد کلاینت Blazor با WebApi
/// دریافت و پردازش متمرکز پاسخ‌های ApiResponse و PagedResult
/// </summary>
public abstract class BaseApiService
{
    protected readonly HttpClient Http;

    protected BaseApiService(HttpClient http)
    {
        Http = http;
    }

    protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.GetFromJsonAsync<ApiResponse<T>>(endpoint, ct);
            return response ?? ApiResponse<T>.Fail("پاسخی از سرور دریافت نشد.");
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    protected async Task<ApiResponse<PagedResult<T>>> GetPagedAsync<T>(
        string endpoint,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        try
        {
            var query = $"?pageNumber={pagination.PageNumber}&pageSize={pagination.PageSize}";
            if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
            {
                query += $"&searchTerm={Uri.EscapeDataString(pagination.SearchTerm)}";
            }

            var fullUrl = endpoint.Contains('?') ? $"{endpoint}&{query.TrimStart('?')}" : $"{endpoint}{query}";
            var response = await Http.GetFromJsonAsync<ApiResponse<PagedResult<T>>>(fullUrl, ct);

            return response ?? ApiResponse<PagedResult<T>>.Fail("پاسخی از سرور دریافت نشد.");
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<T>>.Fail(ex.Message);
        }
    }

    protected async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var res = await Http.PostAsJsonAsync(endpoint, request, ct);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken: ct);

            return result ?? ApiResponse<TResponse>.Fail($"خطا در ارتباط با سرور (کد: {res.StatusCode})");
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail(ex.Message);
        }
    }

    protected async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var res = await Http.PutAsJsonAsync(endpoint, request, ct);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken: ct);

            return result ?? ApiResponse<TResponse>.Fail($"خطا در ارتباط با سرور (کد: {res.StatusCode})");
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail(ex.Message);
        }
    }

    protected async Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var res = await Http.DeleteAsync(endpoint, ct);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: ct);

            return result ?? ApiResponse<bool>.Fail($"خطا در ارتباط با سرور (کد: {res.StatusCode})");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail(ex.Message);
        }
    }
}
