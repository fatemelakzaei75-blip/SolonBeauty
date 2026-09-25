using System.Net.Http.Json;
using BlazorAppSolon.Auth;
using DataLayer.Entity.BaseEntity;
using Microsoft.JSInterop;

namespace BlazorAppSolon.Data;

/// <summary>
/// پیاده سازی دسترسی به داده از طریق WebApi (کنترلرهای CRUD با احراز هویت JWT).
/// اگر سرویس در دسترس نباشد، به صورت خودکار به داده نمونه محلی (MockDb) برمی گردد
/// تا پنل ها همیشه قابل استفاده بمانند.
/// </summary>
public class ApiDataStore : IDataStore
{
    private readonly HttpClient _http;
    private readonly MockDb _mock;
    private readonly JwtAuthenticationStateProvider _auth;

    private bool _online = true;
    public bool IsOnline => _online;

    public ApiDataStore(HttpClient http, MockDb mock, JwtAuthenticationStateProvider auth)
    {
        _http = http; _mock = mock; _auth = auth;
    }

    /// <summary>مسیر API هر موجودیت (بر اساس EntityRegistry)</summary>
    private static string Route<T>() => EntityRegistry.ApiRouteFor(typeof(T));

    private async Task EnsureTokenAsync()
    {
        if (_http.DefaultRequestHeaders.Authorization is not null) return;
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    public async Task<List<T>> AllAsync<T>() where T : Tbl_BaseEntity
    {
        try
        {
            await EnsureTokenAsync();
            var url = $"{AuthService.ApiBase}/api/{Route<T>()}";
            var data = await _http.GetFromJsonAsync<List<T>>(url);
            if (data is not null) { _online = true; return data; }
        }
        catch { _online = false; }

        return _mock.All<T>();
    }

    public async Task<T?> FindAsync<T>(Guid tc) where T : Tbl_BaseEntity
    {
        try
        {
            await EnsureTokenAsync();
            var item = await _http.GetFromJsonAsync<T>($"{AuthService.ApiBase}/api/{Route<T>()}/{tc}");
            if (item is not null) { _online = true; return item; }
        }
        catch { _online = false; }

        return _mock.Find<T>(tc);
    }

    public async Task<bool> AddAsync<T>(T entity) where T : Tbl_BaseEntity
    {
        try
        {
            await EnsureTokenAsync();
            var res = await _http.PostAsJsonAsync($"{AuthService.ApiBase}/api/{Route<T>()}", entity);
            if (res.IsSuccessStatusCode) { _online = true; return true; }
        }
        catch { _online = false; }

        _mock.Add(entity);
        return true;
    }

    public async Task<bool> UpdateAsync<T>(T entity) where T : Tbl_BaseEntity
    {
        try
        {
            await EnsureTokenAsync();
            var res = await _http.PutAsJsonAsync($"{AuthService.ApiBase}/api/{Route<T>()}/{entity.Tc}", entity);
            if (res.IsSuccessStatusCode) { _online = true; return true; }
        }
        catch { _online = false; }

        _mock.Update(entity);
        return true;
    }

    public async Task<bool> DeleteAsync<T>(Guid tc) where T : Tbl_BaseEntity
    {
        try
        {
            await EnsureTokenAsync();
            var res = await _http.DeleteAsync($"{AuthService.ApiBase}/api/{Route<T>()}/{tc}");
            if (res.IsSuccessStatusCode) { _online = true; return true; }
        }
        catch { _online = false; }

        _mock.Delete<T>(tc);
        return true;
    }

    public async Task<int> CountAsync<T>() where T : Tbl_BaseEntity => (await AllAsync<T>()).Count;
}
