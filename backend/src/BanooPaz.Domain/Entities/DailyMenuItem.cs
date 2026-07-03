namespace BanooPaz.Domain.Entities;

public sealed class DailyMenuItem
{
    public int Id { get; init; }
    public int DailyMenuId { get; init; }
    public int FoodId { get; init; }
    public decimal Price { get; set; }
    public int CapacityPortions { get; set; }
    public int SoldPortions { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; init; }

    public int RemainingPortions => CapacityPortions - SoldPortions;

    public DailyMenu DailyMenu { get; init; } = null!;
    public Food Food { get; init; } = null!;
    public ICollection<OrderItem> OrderItems { get; init; } = new List<OrderItem>();
}
