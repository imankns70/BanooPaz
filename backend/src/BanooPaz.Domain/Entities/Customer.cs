namespace BanooPaz.Domain.Entities;

public sealed class Customer
{
    public int Id { get; init; }
    public long? TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastOrderAt { get; set; }

    public ICollection<CustomerAddress> Addresses { get; init; } = new List<CustomerAddress>();
    public ICollection<Order> Orders { get; init; } = new List<Order>();
}
