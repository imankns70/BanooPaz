using Kafgir.Domain.Entities;

namespace Kafgir.Infrastructure.Notifications;

public interface ITelegramMessageSender
{
    Task SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default);
}
