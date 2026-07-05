namespace BanooPaz.Domain.Entities;

public sealed class CustomerAddress
{
    public int Id { get; init; }
    public int CustomerProfileId { get; set; }
    public string Title { get; set; } = "آدرس اصلی";
    public string City { get; set; } = "اندیمشک";
    public string AddressLine { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUsedAt { get; set; }

    public CustomerProfile CustomerProfile { get; set; } = null!;
}
