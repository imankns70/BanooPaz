using Kafgir.Domain.Enums;

namespace Kafgir.Domain.Entities;

public sealed class Order
{
    public int Id { get; init; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerProfileId { get; set; }
    public int? CustomerAddressId { get; set; }
    public string DeliveryFullName { get; set; } = string.Empty;
    public string DeliveryPhoneNumber { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = "اندیمشک";
    public string DeliveryAddressLine { get; set; } = string.Empty;
    public string? DeliveryAddressDescription { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CustomerNote { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public CustomerProfile CustomerProfile { get; set; } = null!;
    public CustomerAddress? CustomerAddress { get; set; }
    public ICollection<OrderItem> Items { get; init; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistories { get; init; } = new List<OrderStatusHistory>();
}
