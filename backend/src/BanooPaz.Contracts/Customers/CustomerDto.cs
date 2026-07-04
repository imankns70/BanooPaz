namespace BanooPaz.Contracts.Customers;

public sealed class CustomerDto
{
    public int Id { get; set; }
    public long? TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
