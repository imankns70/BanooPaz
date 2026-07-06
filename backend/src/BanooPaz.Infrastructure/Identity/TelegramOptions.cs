namespace BanooPaz.Infrastructure.Identity;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    public string BotToken { get; set; } = string.Empty;
    public string AdminChatId { get; set; } = string.Empty;
    public int InitDataMaxAgeMinutes { get; set; } = 1440;
    public bool RequireInitData { get; set; }
}
