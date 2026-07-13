using System.Net.Http;
using System.Text.Json;

namespace Kafgir.WPF.Services.Api;

internal static class ApiResponseHandler
{
    public static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = $"خطای API ({(int)response.StatusCode})";
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                message = error.GetString() ?? message;
            }
        }
        catch (JsonException)
        {
            // Keep the status-code message for a non-JSON response.
        }

        throw new HttpRequestException(message, null, response.StatusCode);
    }
}
