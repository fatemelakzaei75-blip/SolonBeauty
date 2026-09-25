using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت مشخصات سالن زیبایی
/// </summary>
public class SalonService : BaseRepository<Tbl_Salon>, ISalonService
{
    public SalonService(DatabaseContext db) : base(db)
    {
    }

    public Tbl_Salon GetSalon()
    {
        return QueryNoTracking.OrderBy(x => x.ID).FirstOrDefault()!;
    }

    public bool AddSalon(Tbl_Salon salon)
    {
        return Insert(salon);
    }

    public bool EditSalon(Tbl_Salon salon)
    {
        return Modify(salon);
    }
}
