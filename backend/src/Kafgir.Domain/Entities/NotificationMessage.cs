using Kafgir.Domain.Enums;

namespace Kafgir.Domain.Entities;

public sealed class NotificationMessage
{
    public int Id { get; init; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.Telegram;
    public NotificationMessageType Type { get; set; }
    public NotificationMessageStatus Status { get; set; } = NotificationMessageStatus.Pending;
    public string Target { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? LastError { get; set; }

    public Order? Order { get; set; }
}
