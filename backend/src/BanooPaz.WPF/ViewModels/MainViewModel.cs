using CommunityToolkit.Mvvm.ComponentModel;

namespace BanooPaz.WPF.ViewModels;

public sealed class MainViewModel(
    LoginViewModel login,
    DashboardViewModel dashboard,
    OrdersViewModel orders,
    ManualOrderViewModel manualOrder,
    FoodsViewModel foods,
    DailyMenuViewModel dailyMenu) : ObservableObject
{
    private bool _isAuthenticated;
    private int _selectedNavigationIndex;
    private bool _dashboardLoaded;
    private bool _ordersLoaded;
    private bool _foodsLoaded;

    public LoginViewModel Login { get; } = login;
    public DashboardViewModel Dashboard { get; } = dashboard;
    public OrdersViewModel Orders { get; } = orders;
    public ManualOrderViewModel ManualOrder { get; } = manualOrder;
    public FoodsViewModel Foods { get; } = foods;
    public DailyMenuViewModel DailyMenu { get; } = dailyMenu;

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set => SetProperty(ref _isAuthenticated, value);
    }

    public int SelectedNavigationIndex
    {
        get => _selectedNavigationIndex;
        set
        {
            if (SetProperty(ref _selectedNavigationIndex, value))
            {
                LoadSelectedPageIfAuthenticated();
            }
        }
    }

    public void MarkAuthenticated()
    {
        IsAuthenticated = true;
        Orders.StartPolling();
        LoadSelectedPageIfAuthenticated();
    }

    private void LoadSelectedPageIfAuthenticated()
    {
        if (!IsAuthenticated)
        {
            return;
        }

        switch (SelectedNavigationIndex)
        {
            case 0 when !_dashboardLoaded:
                _dashboardLoaded = true;
                Dashboard.LoadCommand.Execute(null);
                break;
            case 1 when !_ordersLoaded:
                _ordersLoaded = true;
                Orders.RefreshCommand.Execute(null);
                break;
            case 2:
                ManualOrder.LoadMenuCommand.Execute(null);
                break;
            case 3 when !_foodsLoaded:
                _foodsLoaded = true;
                Foods.LoadFoodsCommand.Execute(null);
                break;
            case 4:
                DailyMenu.LoadMenuCommand.Execute(null);
                break;
        }
    }
}
