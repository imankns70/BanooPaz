namespace BanooPaz.Contracts.Menus;

public sealed class UpdateDailyMenuSettingsRequest
{
    public bool IsOpen { get; set; } = true;
    public string? Note { get; set; }
}
