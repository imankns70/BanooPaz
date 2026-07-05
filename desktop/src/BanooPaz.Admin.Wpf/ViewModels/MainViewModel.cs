using CommunityToolkit.Mvvm.ComponentModel;

namespace BanooPaz.Admin.Wpf.ViewModels;

public sealed class MainViewModel(
    LoginViewModel login,
    OrdersViewModel orders,
    FoodsViewModel foods,
    DailyMenuViewModel dailyMenu) : ObservableObject
{
    private bool _isAuthenticated;

    public LoginViewModel Login { get; } = login;
    public OrdersViewModel Orders { get; } = orders;
    public FoodsViewModel Foods { get; } = foods;
    public DailyMenuViewModel DailyMenu { get; } = dailyMenu;

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set => SetProperty(ref _isAuthenticated, value);
    }

    public void MarkAuthenticated()
    {
        IsAuthenticated = true;
    }
}
