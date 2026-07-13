using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Kafgir.Domain.Entities;
using Kafgir.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace Kafgir.Infrastructure.Notifications;

public sealed class TelegramMessageSender(
    HttpClient httpClient,
    IOptions<TelegramOptions> options) : ITelegramMessageSender
{
    public async Task SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        var botToken = options.Value.BotToken;
        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new InvalidOperationException("Telegram bot token is not configured.");
        }

        using var response = await httpClient.PostAsJsonAsync(
            $"bot{botToken}/sendMessage",
            new SendMessageRequest(message.Target, message.Text),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Telegram sendMessage failed with HTTP {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<TelegramApiResponse>(cancellationToken);
        if (result is not { Ok: true })
        {
            throw new HttpRequestException(
                $"Telegram sendMessage failed: {result?.Description ?? "empty response"}");
        }
    }

    private sealed record SendMessageRequest(
        [property: JsonPropertyName("chat_id")] string ChatId,
        [property: JsonPropertyName("text")] string Text);

    private sealed class TelegramApiResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
