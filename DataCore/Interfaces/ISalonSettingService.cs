using System;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace DataCore.Interfaces;

public interface ISalonSettingService
{
    Task<Tbl_SalonSetting> GetSettingsAsync(CancellationToken ct = default);

    Task<bool> UpdateSettingsAsync(SalonSettingViewModel model, CancellationToken ct = default);
}
