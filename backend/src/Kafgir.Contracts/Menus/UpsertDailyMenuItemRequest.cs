namespace Kafgir.Contracts.Menus;

public sealed class UpsertDailyMenuItemRequest
{
    public int? Id { get; set; }
    public int FoodId { get; set; }
    public decimal Price { get; set; }
    public int CapacityPortions { get; set; }
    public bool IsAvailable { get; set; } = true;
}
