using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس دسته‌بندی خدمات سالن زیبایی
/// </summary>
public class CategoryService : BaseRepository<Tbl_Category>, ICategoryService
{
    public CategoryService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Category> GetCategories()
    {
        return GetAll();
    }

    public Tbl_Category GetCategoryByTC(string tc)
    {
        return GetByTc(tc)!;
    }

    public List<Tbl_Category> SearchCategories(string search)
    {
        return QueryNoTracking
            .Where(x => x.CategoryName.Contains(search))
            .ToList();
    }

    public List<Tbl_Category> GetCategoriesBySalonTC(string salonTC)
    {
        return Guid.TryParse(salonTC, out var g)
            ? QueryNoTracking.Where(x => x.SalonTC == g).ToList()
            : new List<Tbl_Category>();
    }

    public bool AddCategory(Tbl_Category category)
    {
        return Insert(category);
    }

    public bool EditCategory(Tbl_Category category)
    {
        return Modify(category);
    }

    public bool DeleteCategory(string tc)
    {
        return SoftDelete(tc);
    }
}
