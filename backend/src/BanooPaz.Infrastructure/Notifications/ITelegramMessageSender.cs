using BanooPaz.Domain.Entities;

namespace BanooPaz.Infrastructure.Notifications;

public interface ITelegramMessageSender
{
    Task SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default);
}
