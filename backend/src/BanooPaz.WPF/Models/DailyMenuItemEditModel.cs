using CommunityToolkit.Mvvm.ComponentModel;

namespace BanooPaz.WPF.Models;

public sealed class DailyMenuItemEditModel : ObservableObject
{
    private decimal _price;
    private int _capacityPortions;
    private int _soldPortions;
    private int _remainingPortions;
    private bool _isAvailable;

    public int? Id { get; set; }
    public int FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public decimal Price { get => _price; set => SetProperty(ref _price, value); }
    public int CapacityPortions
    {
        get => _capacityPortions;
        set { if (SetProperty(ref _capacityPortions, value)) RemainingPortions = value - SoldPortions; }
    }
    public int SoldPortions
    {
        get => _soldPortions;
        set { if (SetProperty(ref _soldPortions, value)) RemainingPortions = CapacityPortions - value; }
    }
    public int RemainingPortions { get => _remainingPortions; set => SetProperty(ref _remainingPortions, value); }
    public bool IsAvailable { get => _isAvailable; set => SetProperty(ref _isAvailable, value); }
}
