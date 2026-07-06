using BanooPaz.Domain.Entities;
using BanooPaz.Domain.Enums;

namespace BanooPaz.Application.Interfaces;

public interface INotificationQueue
{
    Task EnqueueAdminOrderSubmittedAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task EnqueueCustomerOrderStatusChangedAsync(
        Order order,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default);
}
