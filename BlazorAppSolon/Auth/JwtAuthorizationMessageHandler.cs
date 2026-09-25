using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorAppSolon.Auth;

/// <summary>
/// هندلر متمرکز HTTP برای تزریق خودکار توکن احراز هویت JWT (Bearer) به سرآیند تمام درخواست‌ها
/// بدون نیاز به کد تکراری در تک‌تک سرویس‌ها
/// </summary>
public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly JwtAuthenticationStateProvider _authProvider;

    public JwtAuthorizationMessageHandler(JwtAuthenticationStateProvider authProvider)
    {
        _authProvider = authProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _authProvider.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
