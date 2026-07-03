namespace BanooPaz.Domain.Entities;

public sealed class OrderItem
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public int DailyMenuItemId { get; init; }
    public string FoodName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public Order Order { get; init; } = null!;
    public DailyMenuItem DailyMenuItem { get; init; } = null!;
}
