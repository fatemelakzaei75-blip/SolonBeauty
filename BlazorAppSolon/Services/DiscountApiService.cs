using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;

namespace BlazorAppSolon.Services;

/// <summary>
/// سرویس کلاینت Blazor جهت اعتبارسنجی کدهای تخفیف با WebApi
/// </summary>
public class DiscountApiService : BaseApiService
{
    public DiscountApiService(HttpClient http) : base(http)
    {
    }

    public async Task<ApiResponse<DiscountValidationResultDto>> ValidateAsync(
        string code,
        decimal amount,
        CancellationToken ct = default)
    {
        return await GetAsync<DiscountValidationResultDto>($"api/discount/validate/{code}?amount={amount}", ct);
    }
}
