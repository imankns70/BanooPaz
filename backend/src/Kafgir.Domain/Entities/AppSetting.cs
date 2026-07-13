namespace Kafgir.Domain.Entities;

public sealed class AppSetting
{
    public int Id { get; init; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
