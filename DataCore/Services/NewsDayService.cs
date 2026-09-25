using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس اخبار روز و اطلاعیه‌های سالن
/// </summary>
public class NewsDayService : BaseRepository<Tbl_NewsDay>, INewsDayService
{
    public NewsDayService(DatabaseContext db) : base(db)
    {
    }

    public Tbl_NewsDay GetNewsDay()
    {
        return QueryNoTracking
            .OrderByDescending(x => x.RegisterData)
            .FirstOrDefault()!;
    }

    public bool AddNewsDay(Tbl_NewsDay newsDay)
    {
        return Insert(newsDay);
    }

    public bool EditNewsDay(Tbl_NewsDay newsDay)
    {
        return Modify(newsDay);
    }
}
