using System.Security.Cryptography;
using System.Text;
using BanooPaz.Infrastructure.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BanooPaz.UnitTests;

public sealed class TelegramInitDataValidatorTests
{
    private const string BotToken = "123456:TEST_BOT_TOKEN";

    [Fact]
    public void Validate_accepts_init_data_with_matching_hash()
    {
        var validator = CreateValidator("Production", requireInitData: true);
        var initData = CreateInitData(DateTimeOffset.UtcNow);

        var result = validator.Validate(initData);

        Assert.True(result.IsValid);
        Assert.Equal(123456789, result.UserId);
        Assert.Equal("iman", result.Username);
    }

    [Fact]
    public void Validate_rejects_tampered_init_data()
    {
        var validator = CreateValidator("Production", requireInitData: true);
        var initData = CreateInitData(DateTimeOffset.UtcNow)
            .Replace("username%22%3A%22iman", "username%22%3A%22other");

        var result = validator.Validate(initData);

        Assert.False(result.IsValid);
        Assert.True(result.IsRequired);
        Assert.Equal("Telegram initData hash is invalid.", result.Error);
    }

    [Fact]
    public void Validate_allows_missing_init_data_in_development_when_not_required()
    {
        var validator = CreateValidator("Development", requireInitData: false);

        var result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.False(result.IsRequired);
        Assert.True(result.CanUseDevelopmentFallback);
    }

    [Fact]
    public void Validate_requires_missing_init_data_outside_development()
    {
        var validator = CreateValidator("Production", requireInitData: false);

        var result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.True(result.IsRequired);
        Assert.Equal("Telegram initData is required.", result.Error);
    }

    private static TelegramInitDataValidator CreateValidator(
        string environmentName,
        bool requireInitData)
    {
        return new TelegramInitDataValidator(
            Options.Create(new TelegramOptions
            {
                BotToken = BotToken,
                InitDataMaxAgeMinutes = 1440,
                RequireInitData = requireInitData
            }),
            new FakeHostEnvironment { EnvironmentName = environmentName });
    }

    private static string CreateInitData(DateTimeOffset authDate)
    {
        const string queryId = "AAHdF6IQAAAAAN0XohDhrOrc";
        const string user = "{\"id\":123456789,\"first_name\":\"Iman\",\"username\":\"iman\"}";
        var fields = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["auth_date"] = authDate.ToUnixTimeSeconds().ToString(),
            ["query_id"] = queryId,
            ["user"] = user
        };
        var dataCheckString = string.Join('\n', fields.Select(field => $"{field.Key}={field.Value}"));
        var secretKey = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("WebAppData"),
            Encoding.UTF8.GetBytes(BotToken));
        var hash = Convert.ToHexString(
                HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString)))
            .ToLowerInvariant();

        return string.Join('&', fields.Select(field =>
            $"{Uri.EscapeDataString(field.Key)}={Uri.EscapeDataString(field.Value)}")) + $"&hash={hash}";
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "BanooPaz.UnitTests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
