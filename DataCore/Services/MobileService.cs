using System;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس ذخیره و بازیابی شماره‌های همراه کاربران
/// </summary>
public class MobileService : BaseRepository<Tbl_Mobile>, IMobileService
{
    public MobileService(DatabaseContext db) : base(db)
    {
    }

    public bool IsMobileExists(string mobileNumber)
    {
        return QueryNoTracking.Any(x => x.MobileNumber == mobileNumber);
    }

    public Tbl_Mobile Create(string mobileNumber)
    {
        var existing = Query.FirstOrDefault(x => x.MobileNumber == mobileNumber);
        if (existing is not null)
        {
            return existing;
        }

        var mobile = new Tbl_Mobile
        {
            Tc = Guid.NewGuid(),
            MobileNumber = mobileNumber,
            TcPersonal = string.Empty,
            IsActive = true,
            RegisterData = DateTime.Now
        };

        Insert(mobile);
        return mobile;
    }
}
