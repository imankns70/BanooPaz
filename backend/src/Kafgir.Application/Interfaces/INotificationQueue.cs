using Kafgir.Domain.Entities;
using Kafgir.Domain.Enums;

namespace Kafgir.Application.Interfaces;

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
