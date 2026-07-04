namespace BanooPaz.Contracts.Orders;

public sealed class CreateOrderItemRequest
{
    public int DailyMenuItemId { get; set; }
    public int Quantity { get; set; }
}
