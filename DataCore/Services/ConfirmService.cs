using System;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس ارسال و تأیید کدهای یکبار مصرف (OTP)
/// </summary>
public class ConfirmService : BaseRepository<Tbl_Confirm>, IConfirmService
{
    private readonly IMobileService _mobileService;

    public const string DevCode = "12345";

    public ConfirmService(DatabaseContext db, IMobileService mobileService) : base(db)
    {
        _mobileService = mobileService;
    }

    public void SendCode(string mobileNumber)
    {
        var mobile = _mobileService.Create(mobileNumber);
        var code = DevCode;

        Insert(new Tbl_Confirm
        {
            Tc = Guid.NewGuid(),
            Tc_Mobile = mobile.Tc,
            Code = code,
            IpUser = "-",
            SentDate = DateTime.Now,
            SentCount = 1,
            IsActive = true
        });
    }

    public bool VerifyCode(string mobileNumber, string code)
    {
        var mobile = Db.Tbl_Mobile.FirstOrDefault(m => m.MobileNumber == mobileNumber && !m.IsDelete);
        if (mobile is null)
        {
            return false;
        }

        var confirm = Set
            .Where(c => c.Tc_Mobile == mobile.Tc && !c.IsDelete)
            .OrderByDescending(c => c.SentDate)
            .FirstOrDefault();

        if (confirm is null)
        {
            return false;
        }

        confirm.TryCount++;
        if (confirm.Code != code)
        {
            Db.SaveChanges();
            return false;
        }

        confirm.IsConfirmed = true;
        confirm.ConfirmDate = DateTime.Now;
        Db.SaveChanges();
        return true;
    }
}
