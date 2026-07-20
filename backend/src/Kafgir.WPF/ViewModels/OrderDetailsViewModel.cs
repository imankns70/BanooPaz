using Kafgir.Contracts.Orders;
using CommunityToolkit.Mvvm.ComponentModel;
using Kafgir.WPF.Models;

namespace Kafgir.WPF.ViewModels;

public sealed class OrderDetailsViewModel : ObservableObject
{
    private OrderDto? _order;

    public OrderDto? Order
    {
        get => _order;
        set
        {
            if (SetProperty(ref _order, value))
            {
                ItemsPagination.SetItems(value?.Items);
                StatusHistoriesPagination.SetItems(value?.StatusHistories);
            }
        }
    }

    public PaginationViewModel<OrderItemDto> ItemsPagination { get; } = new(5);
    public PaginationViewModel<OrderStatusHistoryDto> StatusHistoriesPagination { get; } = new(5);
}
