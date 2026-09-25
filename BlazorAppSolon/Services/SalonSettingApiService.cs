using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace BlazorAppSolon.Services;

/// <summary>
/// سرویس کلاینت Blazor جهت دریافت و ذخیره تنظیمات عمومی سالن
/// </summary>
public class SalonSettingApiService : BaseApiService
{
    public SalonSettingApiService(HttpClient http) : base(http)
    {
    }

    public async Task<ApiResponse<Tbl_SalonSetting>> GetSettingsAsync(CancellationToken ct = default)
    {
        return await GetAsync<Tbl_SalonSetting>("api/salonsetting", ct);
    }

    public async Task<ApiResponse<bool>> UpdateSettingsAsync(SalonSettingViewModel model, CancellationToken ct = default)
    {
        return await PutAsync<SalonSettingViewModel, bool>("api/salonsetting", model, ct);
    }
}
