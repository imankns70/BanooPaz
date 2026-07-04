using BanooPaz.Contracts.Orders;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BanooPaz.Admin.Wpf.ViewModels;

public sealed class OrderDetailsViewModel : ObservableObject
{
    private OrderDto? _order;

    public OrderDto? Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }
}
