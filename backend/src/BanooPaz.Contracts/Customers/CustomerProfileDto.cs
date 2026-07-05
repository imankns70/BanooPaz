namespace BanooPaz.Contracts.Customers;

public sealed class CustomerProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PreferredName { get; set; } = string.Empty;
    public string DefaultPhoneNumber { get; set; } = string.Empty;
    public List<CustomerAddressDto> Addresses { get; set; } = new();
}
