namespace BanooPaz.Contracts.Customers;

public sealed class AddCustomerAddressRequest
{
    public string Title { get; set; } = "آدرس جدید";
    public string City { get; set; } = "اندیمشک";
    public string AddressLine { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
}
