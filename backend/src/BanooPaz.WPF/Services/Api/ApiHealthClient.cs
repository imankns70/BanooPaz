using System.Net.Http;

namespace BanooPaz.WPF.Services.Api;

public sealed class ApiHealthClient(HttpClient httpClient) : IApiHealthClient
{
    public async Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync("api/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }
}
