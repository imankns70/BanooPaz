using BanooPaz.Domain.Enums;

namespace BanooPaz.Contracts.Orders;

public sealed class CreateOrderRequest
{
    public long? TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string City { get; set; } = "اندیمشک";
    public string AddressLine { get; set; } = string.Empty;
    public string? AddressDescription { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CardToCard;
    public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.Delivery;
    public string? CustomerNote { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
