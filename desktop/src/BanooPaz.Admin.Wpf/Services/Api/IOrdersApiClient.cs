using BanooPaz.Contracts.Orders;

namespace BanooPaz.Admin.Wpf.Services.Api;

public interface IOrdersApiClient
{
    Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(
        DateOnly date,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default);
}
