using BanooPaz.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BanooPaz.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<int>
{
    public long? TelegramUserId { get; set; }
    public string? TelegramFirstName { get; set; }
    public string? TelegramLastName { get; set; }
    public string? TelegramLanguageCode { get; set; }
    public bool AllowsWriteToPm { get; set; }
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? LastOrderAt { get; set; }

    public CustomerProfile? CustomerProfile { get; set; }
}
