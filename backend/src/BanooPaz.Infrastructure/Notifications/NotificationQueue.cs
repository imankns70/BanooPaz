using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using BanooPaz.Domain.Enums;
using BanooPaz.Infrastructure.Identity;
using BanooPaz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BanooPaz.Infrastructure.Notifications;

public sealed class NotificationQueue(
    BanooPazDbContext dbContext,
    IOptions<TelegramOptions> telegramOptions) : INotificationQueue
{
    public Task EnqueueAdminOrderSubmittedAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        var adminChatId = telegramOptions.Value.AdminChatId;
        if (string.IsNullOrWhiteSpace(adminChatId))
        {
            return Task.CompletedTask;
        }

        dbContext.NotificationMessages.Add(new NotificationMessage
        {
            Type = NotificationMessageType.AdminOrderSubmitted,
            Target = adminChatId.Trim(),
            Text = BuildAdminOrderSubmittedText(order),
            Order = order,
            OrderNumber = order.OrderNumber,
            CreatedAt = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }

    public async Task EnqueueCustomerOrderStatusChangedAsync(
        Order order,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var chatId = await dbContext.TelegramAccounts
            .Where(account => account.UserId == order.CustomerProfile.UserId)
            .Select(account => account.ChatId)
            .SingleOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(chatId))
        {
            return;
        }

        dbContext.NotificationMessages.Add(new NotificationMessage
        {
            Type = NotificationMessageType.CustomerOrderStatusChanged,
            Target = chatId,
            Text = BuildCustomerStatusChangedText(order, newStatus),
            Order = order,
            OrderNumber = order.OrderNumber,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string BuildAdminOrderSubmittedText(Order order)
    {
        return string.Join(
            Environment.NewLine,
            "سفارش جدید بانوپز",
            $"شماره سفارش: {order.OrderNumber}",
            $"مشتری: {order.DeliveryFullName}",
            $"موبایل: {order.DeliveryPhoneNumber}",
            $"مبلغ: {order.TotalAmount:N0} تومان",
            $"آدرس: {order.DeliveryCity}، {order.DeliveryAddressLine}");
    }

    private static string BuildCustomerStatusChangedText(Order order, OrderStatus status)
    {
        var statusText = status switch
        {
            OrderStatus.Confirmed => "تایید شد",
            OrderStatus.Preparing => "در حال آماده‌سازی است",
            OrderStatus.Ready => "آماده تحویل است",
            OrderStatus.Delivered => "تحویل داده شد",
            OrderStatus.Cancelled => "لغو شد",
            _ => "به‌روزرسانی شد"
        };

        return string.Join(
            Environment.NewLine,
            "وضعیت سفارش شما در بانوپز تغییر کرد",
            $"شماره سفارش: {order.OrderNumber}",
            $"وضعیت: {statusText}");
    }
}
