namespace BanooPaz.Contracts.Menus;

public sealed class DailyMenuDto
{
    public int Id { get; set; }
    public DateOnly MenuDate { get; set; }
    public bool IsOpen { get; set; }
    public string? Note { get; set; }
    public List<DailyMenuItemDto> Items { get; set; } = new();
}
