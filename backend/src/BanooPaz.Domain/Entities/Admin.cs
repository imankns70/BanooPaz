using BanooPaz.Domain.Enums;

namespace BanooPaz.Domain.Entities;

public sealed class Admin
{
    public int Id { get; init; }
    public long? TelegramUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public AdminRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; }
}
