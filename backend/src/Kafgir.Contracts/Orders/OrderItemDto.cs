namespace Kafgir.Contracts.Orders;

public sealed class OrderItemDto
{
    public int Id { get; set; }
    public int DailyMenuItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
