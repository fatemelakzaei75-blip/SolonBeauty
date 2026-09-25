using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت پرسنل و متخصصین سالن زیبایی
/// </summary>
public class PersonalService : BaseRepository<Tbl_Personal>, IPersonalService
{
    public PersonalService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Personal> GetPersonals()
    {
        return GetAll();
    }

    public Tbl_Personal GetPersonalByToken(string token)
    {
        return GetByTc(token)!;
    }

    public bool IsPersonalExists(string token)
    {
        return Guid.TryParse(token, out var g) && QueryNoTracking.Any(x => x.Tc == g);
    }

    public List<Tbl_Personal> SearchPersonals(string search)
    {
        return Guid.TryParse(search, out var g)
            ? QueryNoTracking.Where(x => x.TC_Service == g || x.salon_Tc == g).ToList()
            : GetAll();
    }

    public bool AddPersonal(Tbl_Personal personal)
    {
        return Insert(personal);
    }

    public bool EditPersonal(Tbl_Personal personal)
    {
        return Modify(personal);
    }

    public bool DeletePersonal(string token)
    {
        return SoftDelete(token);
    }
}
