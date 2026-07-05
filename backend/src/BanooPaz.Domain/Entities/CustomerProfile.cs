namespace BanooPaz.Domain.Entities;

public sealed class CustomerProfile
{
    public int Id { get; init; }
    public int UserId { get; set; }
    public string PreferredName { get; set; } = string.Empty;
    public string DefaultPhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastOrderAt { get; set; }

    public ICollection<CustomerAddress> Addresses { get; init; } = new List<CustomerAddress>();
    public ICollection<Order> Orders { get; init; } = new List<Order>();
}
