using BanooPaz.Domain.Enums;

namespace BanooPaz.Domain.Entities;

public sealed class OrderStatusHistory
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAt { get; init; }

    public Order Order { get; init; } = null!;
}
