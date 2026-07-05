namespace BanooPaz.Contracts.Customers;

public sealed class CustomerAddressDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string City { get; set; } = "اندیمشک";
    public string AddressLine { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
}
