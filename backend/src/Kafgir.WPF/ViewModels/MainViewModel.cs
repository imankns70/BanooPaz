using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kafgir.WPF.Services.Api;

namespace Kafgir.WPF.ViewModels;

public sealed class MainViewModel(
    LoginViewModel login,
    DashboardViewModel dashboard,
    OrdersViewModel orders,
    ManualOrderViewModel manualOrder,
    FoodsViewModel foods,
    DailyMenuViewModel dailyMenu,
    IAdminSession adminSession) : ObservableObject
{
    private bool _isAuthenticated;
    private int _selectedNavigationIndex;
    private bool _foodsLoaded;
    private IRelayCommand? _logoutCommand;

    public LoginViewModel Login { get; } = login;
    public DashboardViewModel Dashboard { get; } = dashboard;
    public OrdersViewModel Orders { get; } = orders;
    public ManualOrderViewModel ManualOrder { get; } = manualOrder;
    public FoodsViewModel Foods { get; } = foods;
    public DailyMenuViewModel DailyMenu { get; } = dailyMenu;
    public IRelayCommand LogoutCommand => _logoutCommand ??= new RelayCommand(Logout);

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

    private void Logout()
    {
        adminSession.Clear();
        Orders.ResetForLogout();
        ManualOrder.ResetForLogout();
        _foodsLoaded = false;
        IsAuthenticated = false;
        SelectedNavigationIndex = 0;
    }

    private void LoadSelectedPageIfAuthenticated()
    {
        if (!IsAuthenticated)
        {
            return;
        }

        switch (SelectedNavigationIndex)
        {
            case 0:
                Dashboard.RefreshCommand.Execute(null);
                break;
            case 1:
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
