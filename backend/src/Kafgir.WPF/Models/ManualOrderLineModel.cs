using CommunityToolkit.Mvvm.ComponentModel;

namespace Kafgir.WPF.Models;

public sealed class ManualOrderLineModel : ObservableObject
{
    private int _quantity;

    public int DailyMenuItemId { get; init; }
    public string FoodName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public decimal TotalPrice => UnitPrice * Quantity;
}
