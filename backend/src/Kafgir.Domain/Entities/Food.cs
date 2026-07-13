namespace Kafgir.Domain.Entities;

public sealed class Food
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; }

    public ICollection<DailyMenuItem> DailyMenuItems { get; init; } = new List<DailyMenuItem>();
}
