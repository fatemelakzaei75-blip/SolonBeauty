using System.Collections.Generic;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس شبکه‌های اجتماعی سالن
/// </summary>
public class SocialService : BaseRepository<Tbl_Social>, ISocialService
{
    public SocialService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Social> GetSocialMedias()
    {
        return GetAll();
    }

    public bool AddSocialMedia(Tbl_Social socialMedia)
    {
        return Insert(socialMedia);
    }

    public bool EditSocialMedia(Tbl_Social socialMedia)
    {
        return Modify(socialMedia);
    }
}
