namespace BanooPaz.Infrastructure.Identity;

public sealed class TelegramAccount
{
    public int Id { get; init; }
    public int UserId { get; set; }
    public long TelegramUserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? LanguageCode { get; set; }
    public bool AllowsWriteToPm { get; set; }
    public string ChatId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastSeenAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
