using BanooPaz.Domain.Enums;

namespace BanooPaz.Domain.Entities;

public sealed class Order
{
    public int Id { get; init; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; init; }
    public int? CustomerAddressId { get; set; }
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

    public Customer Customer { get; init; } = null!;
    public CustomerAddress? CustomerAddress { get; init; }
    public ICollection<OrderItem> Items { get; init; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistories { get; init; } = new List<OrderStatusHistory>();
}
