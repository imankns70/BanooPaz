using Kafgir.Application.Interfaces;
using Kafgir.Contracts.Admin;
using Kafgir.Domain.Entities;
using Kafgir.Domain.Enums;

namespace Kafgir.Application.Services;

public sealed class AdminDashboardService(
    IOrderRepository orderRepository,
    IDailyMenuRepository dailyMenuRepository) : IAdminDashboardService
{
    public async Task<AdminDashboardSummaryDto> GetTodayAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var orders = await orderRepository.GetByDateAsync(today, null, cancellationToken);
        var menu = await dailyMenuRepository.GetByDateAsync(today, cancellationToken);

        return new AdminDashboardSummaryDto
        {
            Date = today,
            TotalOrders = orders.Count,
            PendingOrders = CountStatus(orders, OrderStatus.PendingConfirmation),
            ConfirmedOrders = CountStatus(orders, OrderStatus.Confirmed),
            PreparingOrders = CountStatus(orders, OrderStatus.Preparing),
            ReadyOrders = CountStatus(orders, OrderStatus.Ready),
            DeliveredOrders = CountStatus(orders, OrderStatus.Delivered),
            CancelledOrders = CountStatus(orders, OrderStatus.Cancelled),
            ActiveOrders = orders.Count(order => order.Status is
                OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.Ready),
            TotalPortions = orders.Sum(order => order.Items.Sum(item => item.Quantity)),
            GrossSales = orders
                .Where(order => order.Status != OrderStatus.Cancelled)
                .Sum(order => order.TotalAmount),
            TodayMenuItems = menu?.Items.Count ?? 0,
            IsTodayMenuOpen = menu?.IsOpen ?? false
        };
    }

    private static int CountStatus(
        IReadOnlyList<Order> orders,
        OrderStatus status) =>
        orders.Count(order => order.Status == status);
}
