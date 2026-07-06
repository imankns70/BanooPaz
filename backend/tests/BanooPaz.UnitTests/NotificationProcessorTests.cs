using BanooPaz.Domain.Entities;
using BanooPaz.Domain.Enums;
using BanooPaz.Infrastructure.Notifications;
using BanooPaz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BanooPaz.UnitTests;

public sealed class NotificationProcessorTests
{
    [Fact]
    public async Task ProcessPending_marks_message_sent_when_sender_succeeds()
    {
        await using var dbContext = CreateDbContext();
        var message = CreateMessage();
        dbContext.NotificationMessages.Add(message);
        await dbContext.SaveChangesAsync();
        var processor = CreateProcessor(dbContext, new FakeTelegramMessageSender());

        var processed = await processor.ProcessPendingAsync(10);

        Assert.Equal(1, processed);
        Assert.Equal(NotificationMessageStatus.Sent, message.Status);
        Assert.NotNull(message.SentAt);
        Assert.Null(message.LastError);
    }

    [Fact]
    public async Task ProcessPending_retries_message_when_sender_fails()
    {
        await using var dbContext = CreateDbContext();
        var message = CreateMessage();
        dbContext.NotificationMessages.Add(message);
        await dbContext.SaveChangesAsync();
        var processor = CreateProcessor(dbContext, new FakeTelegramMessageSender("Telegram is down"));

        var processed = await processor.ProcessPendingAsync(10);

        Assert.Equal(1, processed);
        Assert.Equal(NotificationMessageStatus.Pending, message.Status);
        Assert.Equal(1, message.RetryCount);
        Assert.NotNull(message.NextAttemptAt);
        Assert.Equal("Telegram is down", message.LastError);
    }

    [Fact]
    public async Task ProcessPending_marks_message_failed_after_max_retries()
    {
        await using var dbContext = CreateDbContext();
        var message = CreateMessage();
        message.RetryCount = 1;
        dbContext.NotificationMessages.Add(message);
        await dbContext.SaveChangesAsync();
        var processor = CreateProcessor(
            dbContext,
            new FakeTelegramMessageSender("Still down"),
            maxRetryCount: 2);

        await processor.ProcessPendingAsync(10);

        Assert.Equal(NotificationMessageStatus.Failed, message.Status);
        Assert.Equal(2, message.RetryCount);
        Assert.Null(message.NextAttemptAt);
    }

    private static NotificationProcessor CreateProcessor(
        BanooPazDbContext dbContext,
        ITelegramMessageSender sender,
        int maxRetryCount = 5)
    {
        return new NotificationProcessor(
            dbContext,
            sender,
            Options.Create(new TelegramNotificationOptions
            {
                MaxRetryCount = maxRetryCount,
                InitialRetryDelaySeconds = 1
            }));
    }

    private static BanooPazDbContext CreateDbContext()
    {
        return new BanooPazDbContext(new DbContextOptionsBuilder<BanooPazDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);
    }

    private static NotificationMessage CreateMessage() => new()
    {
        Type = NotificationMessageType.CustomerOrderStatusChanged,
        Target = "123456789",
        Text = "Test notification",
        CreatedAt = DateTime.UtcNow
    };

    private sealed class FakeTelegramMessageSender(string? error = null) : ITelegramMessageSender
    {
        public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            if (error is not null)
            {
                throw new HttpRequestException(error);
            }

            return Task.CompletedTask;
        }
    }
}
