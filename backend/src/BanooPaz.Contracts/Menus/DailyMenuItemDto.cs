namespace BanooPaz.Contracts.Menus;

public sealed class DailyMenuItemDto
{
    public int Id { get; set; }
    public int FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public string? FoodDescription { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int CapacityPortions { get; set; }
    public int SoldPortions { get; set; }
    public int RemainingPortions { get; set; }
    public bool IsAvailable { get; set; }
}
