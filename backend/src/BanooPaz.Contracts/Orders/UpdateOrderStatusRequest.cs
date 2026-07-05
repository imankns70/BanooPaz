namespace BanooPaz.Contracts.Orders;

public sealed class UpdateOrderStatusRequest
{
    public OrderStatus NewStatus { get; set; }
    public string? AdminNote { get; set; }
    public string? StatusNote { get; set; }
}
