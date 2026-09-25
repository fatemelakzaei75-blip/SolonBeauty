using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس لایک نمونه‌کارها توسط مشتریان
/// </summary>
public class LikeService : BaseRepository<Tbl_Like>, ILikeService
{
    public LikeService(DatabaseContext db) : base(db)
    {
    }

    public bool IsLiked(string customerTC, string portfolioTC)
    {
        return Guid.TryParse(customerTC, out var c) &&
               Guid.TryParse(portfolioTC, out var p) &&
               QueryNoTracking.Any(x => x.CustomerTC == c && x.ProfileTC == p);
    }

    public bool AddLike(string customerTC, string portfolioTC)
    {
        if (!Guid.TryParse(customerTC, out var c) || !Guid.TryParse(portfolioTC, out var p))
        {
            return false;
        }

        if (IsLiked(customerTC, portfolioTC))
        {
            return true;
        }

        return Insert(new Tbl_Like
        {
            Tc = Guid.NewGuid(),
            CustomerTC = c,
            ProfileTC = p,
            IsActive = true,
            RegisterData = DateTime.Now
        });
    }

    public bool RemoveLike(string customerTC, string portfolioTC)
    {
        if (!Guid.TryParse(customerTC, out var c) || !Guid.TryParse(portfolioTC, out var p))
        {
            return false;
        }

        var like = Set.FirstOrDefault(x => x.CustomerTC == c && x.ProfileTC == p && !x.IsDelete);
        return like is not null && SoftDelete(like.Tc);
    }

    public int GetLikeCount(string portfolioTC)
    {
        return Guid.TryParse(portfolioTC, out var p)
            ? QueryNoTracking.Count(x => x.ProfileTC == p)
            : 0;
    }

    public List<Tbl_Like> GetLikesByCustomerTC(string customerTC)
    {
        return Guid.TryParse(customerTC, out var c)
            ? QueryNoTracking.Where(x => x.CustomerTC == c).ToList()
            : new List<Tbl_Like>();
    }

    public List<Tbl_Like> GetLikesByPortfolioTC(string portfolioTC)
    {
        return Guid.TryParse(portfolioTC, out var p)
            ? QueryNoTracking.Where(x => x.ProfileTC == p).ToList()
            : new List<Tbl_Like>();
    }
}
