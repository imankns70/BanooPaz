namespace BanooPaz.Contracts.Menus;

public sealed class CreateOrUpdateDailyMenuRequest
{
    public DateOnly MenuDate { get; set; }
    public bool IsOpen { get; set; } = true;
    public string? Note { get; set; }
    public List<UpsertDailyMenuItemRequest> Items { get; set; } = new();
}
