namespace Kafgir.Contracts.Foods;

public sealed class FoodDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
