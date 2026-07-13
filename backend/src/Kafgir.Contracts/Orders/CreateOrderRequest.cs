namespace Kafgir.Contracts.Orders;

public sealed class CreateOrderRequest
{
    public string? TelegramInitData { get; set; }
    public long? TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int? CustomerAddressId { get; set; }
    public string? NewAddressTitle { get; set; }
    public string City { get; set; } = "اندیمشک";
    public string? AddressLine { get; set; }
    public string? AddressDescription { get; set; }
    public bool SaveAddress { get; set; } = true;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CardToCard;
    public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.Delivery;
    public string? CustomerNote { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
