namespace Kafgir.Contracts.Menus;

public sealed class UpdateDailyMenuItemRequest
{
    public decimal Price { get; set; }
    public int CapacityPortions { get; set; }
    public bool IsAvailable { get; set; } = true;
}
