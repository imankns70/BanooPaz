using Kafgir.Contracts.Orders;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kafgir.WPF.ViewModels;

public sealed class OrderDetailsViewModel : ObservableObject
{
    private OrderDto? _order;

    public OrderDto? Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }
}
