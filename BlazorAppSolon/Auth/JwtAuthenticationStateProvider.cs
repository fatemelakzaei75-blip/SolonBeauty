using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorAppSolon.Auth;

/// <summary>
/// وضعیت احراز هویت بر پایه توکن JWT ذخیره شده در localStorage.
/// توکن در هر درخواست به WebApi هم ارسال می شود.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    public const string TokenKey = "solon_token";
    private readonly IJSRuntime _js;
    private readonly HttpClient _http;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public JwtAuthenticationStateProvider(IJSRuntime js, HttpClient http)
    {
        _js = js; _http = http;
    }

    public async Task<string?> GetTokenAsync()
    {
        try { return await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey); }
        catch { return null; }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(_anonymous);

        var claims = ParseClaims(token).ToList();
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (exp is not null && long.TryParse(exp, out var seconds)
            && DateTimeOffset.FromUnixTimeSeconds(seconds) < DateTimeOffset.UtcNow)
        {
            await MarkUserAsLoggedOutAsync();
            return new AuthenticationState(_anonymous);
        }

        _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        _http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    /// <summary>استخراج Claim ها از بدنه توکن JWT</summary>
    public static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) yield break;

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }

        Dictionary<string, JsonElement>? map;
        try { map = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Convert.FromBase64String(payload)); }
        catch { yield break; }
        if (map is null) yield break;

        foreach (var kv in map)
        {
            var type = kv.Key switch
            {
                "role" or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
                "unique_name" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                "nameid" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => ClaimTypes.NameIdentifier,
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone" => ClaimTypes.MobilePhone,
                _ => kv.Key
            };

            if (kv.Value.ValueKind == JsonValueKind.Array)
                foreach (var v in kv.Value.EnumerateArray())
                    yield return new Claim(type, v.ToString());
            else
                yield return new Claim(type, kv.Value.ToString());
        }
    }
}
