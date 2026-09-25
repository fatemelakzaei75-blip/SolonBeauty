using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.ViewModels;
using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس تنظیمات عمومی و پیکربندی‌های سالن زیبایی
/// </summary>
public class SalonSettingService : BaseRepository<Tbl_SalonSetting>, ISalonSettingService
{
    public SalonSettingService(DatabaseContext db) : base(db)
    {
    }

    public async Task<Tbl_SalonSetting> GetSettingsAsync(CancellationToken ct = default)
    {
        var settings = await QueryNoTracking.FirstOrDefaultAsync(ct);
        if (settings is not null)
        {
            return settings;
        }

        var defaultSettings = new Tbl_SalonSetting
        {
            Tc = Guid.NewGuid(),
            SalonName = "سالن زیبایی حدیث",
            SalonLatinName = "Hadis Beauty Lounge",
            HeroSubtitle = "تجربه‌ای آرام، لوکس و حرفه‌ای از خدمات زیبایی",
            IntroText = "ارائه‌دهنده تخصصی‌ترین خدمات زیبایی، مراقبت و سلامت پوست و مو",
            AboutText = "ما در سالن زیبایی حدیث باور داریم که زیبایی طبیعی هر فرد شایسته درخشش است.",
            Theme = "luxury",
            PhoneLandline = "021-12345678",
            PhoneMobile = "09120000001",
            Email = "info@hadisbeauty.salon",
            Address = "تهران، زعفرانیه، خیابان مقدس اردبیلی، پلاک ۲۴",
            WorkingHoursDisplay = "شنبه تا پنج‌شنبه: ۹:۰۰ الی ۲۱:۰۰",
            InstagramUrl = "https://instagram.com/hadisbeauty.salon",
            TelegramUrl = "https://t.me/hadisbeauty",
            WhatsAppUrl = "https://wa.me/989120000001",
            DomesticSocialUrl = "https://eitaa.com/hadisbeauty",
            OpeningHour = new TimeSpan(9, 0, 0),
            ClosingHour = new TimeSpan(21, 0, 0),
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        await InsertAsync(defaultSettings, ct);
        return defaultSettings;
    }

    public async Task<bool> UpdateSettingsAsync(SalonSettingViewModel model, CancellationToken ct = default)
    {
        var settings = await Set.FirstOrDefaultAsync(s => !s.IsDelete, ct);
        if (settings is null)
        {
            settings = new Tbl_SalonSetting
            {
                Tc = Guid.NewGuid(),
                IsActive = true,
                RegisterData = DateTime.Now
            };
            Set.Add(settings);
        }

        settings.SalonName = model.SalonName;
        settings.SalonLatinName = model.EnglishName;
        settings.HeroSubtitle = model.Slogan;
        settings.AboutText = model.AboutText;
        settings.WorkingHoursDisplay = $"{model.WorkingDays} - {model.WorkingHours}";
        settings.PhoneLandline = model.PhonePrimary;
        settings.PhoneMobile = model.PhoneSecondary;
        settings.Address = model.Address;
        settings.InstagramUrl = model.InstagramUrl;
        settings.TelegramUrl = model.TelegramUrl;
        settings.WhatsAppUrl = model.WhatsAppNumber;
        settings.Theme = model.ActiveTheme;

        return await Db.SaveChangesAsync(ct) > 0;
    }
}
