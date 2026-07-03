namespace BanooPaz.Domain.Entities;

public sealed class CustomerAddress
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public string City { get; set; } = "اندیمشک";
    public string AddressLine { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; init; }

    public Customer Customer { get; init; } = null!;
}
