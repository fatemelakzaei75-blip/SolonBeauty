using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس لاین‌ها و خدمات سالن
/// </summary>
public class SalonServiceService : BaseRepository<Tbl_SalonSerice>, ISalonServiceService
{
    public SalonServiceService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_SalonSerice> GetServices()
    {
        return GetAll();
    }

    public Tbl_SalonSerice GetServiceByTC(string tc)
    {
        return GetByTc(tc)!;
    }

    public List<Tbl_SalonSerice> GetServicesByCategoryTC(string categoryTC)
    {
        return Guid.TryParse(categoryTC, out var g)
            ? QueryNoTracking.Where(x => x.CategoryTC == g).ToList()
            : new List<Tbl_SalonSerice>();
    }

    public List<Tbl_SalonSerice> SearchServices(string search)
    {
        return QueryNoTracking
            .Where(x => x.ServiceName.Contains(search) || (x.Description ?? "").Contains(search))
            .ToList();
    }

    public bool AddService(Tbl_SalonSerice service)
    {
        return Insert(service);
    }

    public bool EditService(Tbl_SalonSerice service)
    {
        return Modify(service);
    }

    public bool DeleteService(string tc)
    {
        return SoftDelete(tc);
    }
}
