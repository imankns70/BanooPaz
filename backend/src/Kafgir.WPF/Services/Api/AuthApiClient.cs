using System.Net.Http;
using System.Net.Http.Json;
using Kafgir.Contracts.Auth;

namespace Kafgir.WPF.Services.Api;

public sealed class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    public async Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/admin/login",
            request,
            cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AdminLoginResponse>(cancellationToken)
            ?? throw new HttpRequestException("پاسخ ورود مدیر خالی بود.");
    }
}
