using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using BanooPaz.Domain.Enums;
using BanooPaz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BanooPaz.Infrastructure.Notifications;

public sealed class NotificationProcessor(
    BanooPazDbContext dbContext,
    ITelegramMessageSender telegramMessageSender,
    IOptions<TelegramNotificationOptions> options) : INotificationProcessor
{
    public async Task<int> ProcessPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var messages = await dbContext.NotificationMessages
            .Where(message =>
                message.Channel == NotificationChannel.Telegram &&
                message.Status == NotificationMessageStatus.Pending &&
                (message.NextAttemptAt == null || message.NextAttemptAt <= now))
            .OrderBy(message => message.CreatedAt)
            .Take(Math.Max(1, batchSize))
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, cancellationToken);
            processed++;
        }

        return processed;
    }

    private async Task ProcessMessageAsync(
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        message.LastAttemptAt = DateTime.UtcNow;
        try
        {
            await telegramMessageSender.SendAsync(message, cancellationToken);
            message.Status = NotificationMessageStatus.Sent;
            message.SentAt = DateTime.UtcNow;
            message.LastError = null;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            RegisterFailure(message, exception.Message);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private void RegisterFailure(NotificationMessage message, string error)
    {
        var notificationOptions = options.Value;
        message.RetryCount++;
        message.LastError = error.Length <= 1000 ? error : error[..1000];
        if (message.RetryCount >= notificationOptions.MaxRetryCount)
        {
            message.Status = NotificationMessageStatus.Failed;
            message.NextAttemptAt = null;
            return;
        }

        var delaySeconds = notificationOptions.InitialRetryDelaySeconds
            * Math.Pow(2, Math.Max(0, message.RetryCount - 1));
        message.Status = NotificationMessageStatus.Pending;
        message.NextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
