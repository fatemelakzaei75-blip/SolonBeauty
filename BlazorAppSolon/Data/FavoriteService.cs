using System.Security.Claims;
using BlazorAppSolon.Auth;
using DataLayer.Entity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorAppSolon.Data;

/// <summary>
/// مدیریت علاقه مندی های مشتری (جدول Tbl_Like).
/// در صورت در دسترس بودن WebApi روی سرویس ذخیره می شود، در غیر این صورت روی داده نمونه محلی.
/// </summary>
public class FavoriteService
{
    private readonly IDataStore _store;
    private readonly AuthenticationStateProvider _auth;
    private HashSet<Guid> _cache = new();
    private bool _loaded;

    public event Action? Changed;

    public FavoriteService(IDataStore store, AuthenticationStateProvider auth)
    {
        _store = store; _auth = auth;
    }

    /// <summary>TC مشتری جاری (در صورت ورود)</summary>
    public async Task<Guid?> CurrentCustomerTcAsync()
    {
        var state = await _auth.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated != true) return null;
        var id = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? state.User.FindFirst("nameid")?.Value;
        return Guid.TryParse(id, out var g) ? g : null;
    }

    public async Task<HashSet<Guid>> LoadAsync(bool force = false)
    {
        if (_loaded && !force) return _cache;

        var customer = await CurrentCustomerTcAsync();
        if (customer is null) { _cache = new(); _loaded = true; return _cache; }

        var likes = await _store.AllAsync<Tbl_Like>();
        _cache = likes.Where(l => l.CustomerTC == customer).Select(l => l.ProfileTC).ToHashSet();
        _loaded = true;
        return _cache;
    }

    public bool IsFavorite(Guid portfolioTc) => _cache.Contains(portfolioTc);

    /// <summary>افزودن/حذف علاقه مندی — اگر کاربر وارد نشده باشد false برمی گرداند</summary>
    public async Task<bool> ToggleAsync(Guid portfolioTc)
    {
        var customer = await CurrentCustomerTcAsync();
        if (customer is null) return false;

        await LoadAsync();

        if (_cache.Contains(portfolioTc))
        {
            var likes = await _store.AllAsync<Tbl_Like>();
            var like = likes.FirstOrDefault(l => l.CustomerTC == customer && l.ProfileTC == portfolioTc);
            if (like is not null) await _store.DeleteAsync<Tbl_Like>(like.Tc);
            _cache.Remove(portfolioTc);
        }
        else
        {
            await _store.AddAsync(new Tbl_Like
            {
                Tc = Guid.NewGuid(),
                CustomerTC = customer.Value,
                ProfileTC = portfolioTc,
                IsActive = true,
                RegisterData = DateTime.Now
            });
            _cache.Add(portfolioTc);
        }

        Changed?.Invoke();
        return true;
    }

    /// <summary>نمونه کارهای مورد علاقه مشتری جاری</summary>
    public async Task<List<PortfolioItem>> FavoriteItemsAsync()
    {
        await LoadAsync(force: true);
        return PortfolioCatalog.All().Where(i => _cache.Contains(i.Tc)).ToList();
    }
}
