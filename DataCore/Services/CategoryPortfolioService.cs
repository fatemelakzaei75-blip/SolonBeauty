using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس دسته‌بندی نمونه‌کارهای سالن
/// </summary>
public class CategoryPortfolioService : BaseRepository<Tbl_CategoryPortfolio>, ICategoryPortfolioService
{
    public CategoryPortfolioService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_CategoryPortfolio> GetCategoryPortfolios()
    {
        return GetAll();
    }

    public Tbl_CategoryPortfolio GetCategoryPortfolioByTC(string tc)
    {
        return GetByTc(tc)!;
    }

    public List<Tbl_CategoryPortfolio> GetCategoryPortfoliosBySalonTC(string salonTC)
    {
        return Guid.TryParse(salonTC, out var g)
            ? QueryNoTracking.Where(x => x.SalonTC == g).ToList()
            : new List<Tbl_CategoryPortfolio>();
    }

    public List<Tbl_CategoryPortfolio> GetCategoryPortfoliosByCategoryTC(string categoryTC)
    {
        return Guid.TryParse(categoryTC, out var g)
            ? QueryNoTracking.Where(x => x.Tc == g).ToList()
            : new List<Tbl_CategoryPortfolio>();
    }

    public bool AddCategoryPortfolio(Tbl_CategoryPortfolio categoryPortfolio)
    {
        return Insert(categoryPortfolio);
    }

    public bool EditCategoryPortfolio(Tbl_CategoryPortfolio categoryPortfolio)
    {
        return Modify(categoryPortfolio);
    }

    public bool DeleteCategoryPortfolio(string tc)
    {
        return SoftDelete(tc);
    }
}
