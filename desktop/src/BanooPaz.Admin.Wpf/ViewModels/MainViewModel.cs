namespace BanooPaz.Admin.Wpf.ViewModels;

public sealed class MainViewModel(
    OrdersViewModel orders,
    FoodsViewModel foods,
    DailyMenuViewModel dailyMenu)
{
    public OrdersViewModel Orders { get; } = orders;
    public FoodsViewModel Foods { get; } = foods;
    public DailyMenuViewModel DailyMenu { get; } = dailyMenu;
}
