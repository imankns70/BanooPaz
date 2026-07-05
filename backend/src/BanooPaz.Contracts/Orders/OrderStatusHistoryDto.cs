namespace BanooPaz.Contracts.Orders;

public sealed class OrderStatusHistoryDto
{
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; }
}
