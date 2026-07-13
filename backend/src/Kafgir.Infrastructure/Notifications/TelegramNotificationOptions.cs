namespace Kafgir.Infrastructure.Notifications;

public sealed class TelegramNotificationOptions
{
    public const string SectionName = "TelegramNotifications";

    public int MaxRetryCount { get; set; } = 5;
    public int InitialRetryDelaySeconds { get; set; } = 60;
}
