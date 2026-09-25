using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس نمونه‌کارهای متخصصین
/// </summary>
public class PortfolioService : BaseRepository<Tbl_Portfoilo>, IPortfolioService
{
    public PortfolioService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Portfoilo> GetPortfolios()
    {
        return GetAll();
    }

    public Tbl_Portfoilo GetPortfolioByTC(string tc)
    {
        return GetByTc(tc)!;
    }

    public Tbl_Portfoilo GetPortfolioByPortfolioSampleTC(string portfolioSampleTC)
    {
        return Guid.TryParse(portfolioSampleTC, out var g)
            ? QueryNoTracking.FirstOrDefault(x => x.SampleTC == g)!
            : null!;
    }

    public List<Tbl_Portfoilo> SearchPortfolios(string search)
    {
        return QueryNoTracking
            .Where(x => x.Title.Contains(search) || (x.Description ?? "").Contains(search))
            .ToList();
    }

    public List<Tbl_Portfoilo> GetMostLikedPortfolios()
    {
        return (from p in QueryNoTracking
                let likes = Db.Tbl_Like.Count(l => l.ProfileTC == p.Tc && !l.IsDelete)
                orderby likes descending
                select p).Take(12).ToList();
    }

    public bool AddPortfolio(Tbl_Portfoilo portfolio)
    {
        return Insert(portfolio);
    }

    public bool EditPortfolio(Tbl_Portfoilo portfolio)
    {
        return Modify(portfolio);
    }

    public bool DeletePortfolio(string tc)
    {
        return SoftDelete(tc);
    }
}
