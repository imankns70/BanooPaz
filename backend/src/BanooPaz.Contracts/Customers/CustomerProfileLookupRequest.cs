namespace BanooPaz.Contracts.Customers;

public sealed class CustomerProfileLookupRequest
{
    public string? TelegramInitData { get; set; }
    public long? TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
}
