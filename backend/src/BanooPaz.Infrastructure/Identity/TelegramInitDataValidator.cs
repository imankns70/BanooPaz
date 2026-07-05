using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BanooPaz.Application.Common;
using BanooPaz.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BanooPaz.Infrastructure.Identity;

public sealed class TelegramInitDataValidator(
    IOptions<TelegramOptions> options,
    IHostEnvironment environment) : ITelegramInitDataValidator
{
    private const string WebAppData = "WebAppData";

    public TelegramInitDataValidationResult Validate(string? initData)
    {
        var telegramOptions = options.Value;
        var isRequired = telegramOptions.RequireInitData || !environment.IsDevelopment();

        if (string.IsNullOrWhiteSpace(initData))
        {
            return isRequired
                ? TelegramInitDataValidationResult.MissingRequired()
                : TelegramInitDataValidationResult.MissingOptional();
        }

        if (string.IsNullOrWhiteSpace(telegramOptions.BotToken))
        {
            return TelegramInitDataValidationResult.Invalid(
                isRequired,
                "Telegram bot token is not configured.");
        }

        var fields = ParseQueryString(initData);
        if (!fields.TryGetValue("hash", out var receivedHash) || string.IsNullOrWhiteSpace(receivedHash))
        {
            return TelegramInitDataValidationResult.Invalid(isRequired, "Telegram initData hash is missing.");
        }

        if (!ValidateAge(fields, telegramOptions.InitDataMaxAgeMinutes, isRequired, out var ageError))
        {
            return TelegramInitDataValidationResult.Invalid(isRequired, ageError);
        }

        var dataCheckString = string.Join(
            '\n',
            fields
                .Where(pair => pair.Key != "hash")
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}={pair.Value}"));

        if (!IsValidHexHash(receivedHash))
        {
            return TelegramInitDataValidationResult.Invalid(isRequired, "Telegram initData hash format is invalid.");
        }

        var expectedHash = ComputeTelegramHash(dataCheckString, telegramOptions.BotToken);
        if (!CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(expectedHash),
            Convert.FromHexString(receivedHash)))
        {
            return TelegramInitDataValidationResult.Invalid(isRequired, "Telegram initData hash is invalid.");
        }

        if (!fields.TryGetValue("user", out var userJson) || string.IsNullOrWhiteSpace(userJson))
        {
            return TelegramInitDataValidationResult.Invalid(isRequired, "Telegram initData user is missing.");
        }

        var user = JsonSerializer.Deserialize<TelegramWebAppUser>(
            userJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (user is null || user.Id <= 0)
        {
            return TelegramInitDataValidationResult.Invalid(isRequired, "Telegram initData user is invalid.");
        }

        return TelegramInitDataValidationResult.Valid(
            user.Id,
            user.Username,
            user.FirstName,
            user.LastName);
    }

    private static Dictionary<string, string> ParseQueryString(string initData)
    {
        return initData
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part =>
            {
                var separatorIndex = part.IndexOf('=');
                if (separatorIndex < 0)
                {
                    return new KeyValuePair<string, string>(
                        Uri.UnescapeDataString(part.Replace('+', ' ')),
                        string.Empty);
                }

                return new KeyValuePair<string, string>(
                    Uri.UnescapeDataString(part[..separatorIndex].Replace('+', ' ')),
                    Uri.UnescapeDataString(part[(separatorIndex + 1)..].Replace('+', ' ')));
            })
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
    }

    private static bool ValidateAge(
        IReadOnlyDictionary<string, string> fields,
        int maxAgeMinutes,
        bool isRequired,
        out string error)
    {
        error = string.Empty;
        if (!fields.TryGetValue("auth_date", out var authDateValue) ||
            !long.TryParse(authDateValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var authDateUnix))
        {
            error = "Telegram initData auth_date is missing or invalid.";
            return false;
        }

        if (maxAgeMinutes <= 0)
        {
            return true;
        }

        var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix);
        var expiresAt = authDate.AddMinutes(maxAgeMinutes);
        if (expiresAt < DateTimeOffset.UtcNow)
        {
            error = "Telegram initData has expired.";
            return false;
        }

        if (isRequired && authDate > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            error = "Telegram initData auth_date is in the future.";
            return false;
        }

        return true;
    }

    private static string ComputeTelegramHash(string dataCheckString, string botToken)
    {
        var secretKey = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(WebAppData),
            Encoding.UTF8.GetBytes(botToken));
        var hash = HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class TelegramWebAppUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
    }

    private static bool IsValidHexHash(string value)
    {
        return value.Length == 64 && value.All(Uri.IsHexDigit);
    }
}
