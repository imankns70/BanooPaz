using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Threading;
using Kafgir.WPF.Models;
using Kafgir.WPF.Services.Api;
using Kafgir.Contracts.Orders;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Kafgir.WPF.ViewModels;

public sealed class OrdersViewModel : ObservableObject, IDisposable
{
    private readonly IOrdersApiClient _apiClient;
    private readonly DispatcherTimer _pollTimer;
    private DateTime _selectedDate = DateTime.Today;
    private OrderStatusFilterOption _selectedStatusFilter;
    private OrderSummaryDto? _selectedOrder;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;

    public OrdersViewModel(IOrdersApiClient apiClient)
    {
        _apiClient = apiClient;
        StatusFilters =
        [
            new("همه وضعیت‌ها", null),
            new("در انتظار تایید", OrderStatus.PendingConfirmation),
            new("تایید شده", OrderStatus.Confirmed),
            new("در حال آماده‌سازی", OrderStatus.Preparing),
            new("آماده تحویل", OrderStatus.Ready),
            new("تحویل شده", OrderStatus.Delivered),
            new("لغو شده", OrderStatus.Cancelled)
        ];
        _selectedStatusFilter = StatusFilters[0];

        LoadOrdersCommand = new AsyncRelayCommand(LoadOrdersAsync, () => !IsBusy);
        RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync, () => !IsBusy);
        LoadOrderDetailsCommand = new AsyncRelayCommand(LoadOrderDetailsAsync, CanUseSelectedOrder);
        ConfirmOrderCommand = CreateStatusCommand(OrderStatus.Confirmed);
        SetPreparingCommand = CreateStatusCommand(OrderStatus.Preparing);
        SetReadyCommand = CreateStatusCommand(OrderStatus.Ready);
        SetDeliveredCommand = CreateStatusCommand(OrderStatus.Delivered);
        CancelOrderCommand = CreateStatusCommand(OrderStatus.Cancelled);

        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        _pollTimer.Tick += PollTimerOnTick;
    }

    public ObservableCollection<OrderSummaryDto> Orders { get; } = [];
    public IReadOnlyList<OrderStatusFilterOption> StatusFilters { get; }
    public OrderDetailsViewModel Details { get; } = new();

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public OrderStatusFilterOption SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set => SetProperty(ref _selectedStatusFilter, value);
    }

    public OrderSummaryDto? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (!SetProperty(ref _selectedOrder, value))
            {
                return;
            }

            NotifyCommandStates();
            if (value is not null)
            {
                LoadOrderDetailsCommand.Execute(null);
            }
            else
            {
                Details.Order = null;
            }
        }
    }

    public OrderDto? SelectedOrderDetails => Details.Order;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                NotifyCommandStates();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public IAsyncRelayCommand LoadOrdersCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand LoadOrderDetailsCommand { get; }
    public IAsyncRelayCommand ConfirmOrderCommand { get; }
    public IAsyncRelayCommand SetPreparingCommand { get; }
    public IAsyncRelayCommand SetReadyCommand { get; }
    public IAsyncRelayCommand SetDeliveredCommand { get; }
    public IAsyncRelayCommand CancelOrderCommand { get; }

    public void Dispose()
    {
        _pollTimer.Stop();
        _pollTimer.Tick -= PollTimerOnTick;
    }

    public void StartPolling()
    {
        if (!_pollTimer.IsEnabled)
        {
            _pollTimer.Start();
        }
    }

    private IAsyncRelayCommand CreateStatusCommand(OrderStatus status) =>
        new AsyncRelayCommand(() => UpdateStatusAsync(status), CanUseSelectedOrder);

    private async Task LoadOrdersAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var selectedId = SelectedOrder?.Id;
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var orders = await _apiClient.GetOrdersAsync(
                DateOnly.FromDateTime(SelectedDate),
                SelectedStatusFilter.Value);

            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }

            var refreshedSelection = selectedId.HasValue
                ? Orders.FirstOrDefault(order => order.Id == selectedId.Value)
                : null;

            if (!ReferenceEquals(_selectedOrder, refreshedSelection))
            {
                _selectedOrder = refreshedSelection;
                OnPropertyChanged(nameof(SelectedOrder));
                NotifyCommandStates();
            }

            if (refreshedSelection is null && selectedId.HasValue)
            {
                Details.Order = null;
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"ارتباط با API برقرار نشد: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadOrderDetailsAsync()
    {
        if (SelectedOrder is null || IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Details.Order = await _apiClient.GetOrderAsync(SelectedOrder.Id);
            OnPropertyChanged(nameof(SelectedOrderDetails));
            if (Details.Order is null)
            {
                ErrorMessage = "سفارش پیدا نشد.";
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"دریافت جزئیات سفارش ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateStatusAsync(OrderStatus status)
    {
        if (SelectedOrder is null || IsBusy)
        {
            return;
        }

        var orderId = SelectedOrder.Id;
        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await _apiClient.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest
            {
                NewStatus = status
            });
            SuccessMessage = "وضعیت سفارش با موفقیت به‌روزرسانی شد.";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"تغییر وضعیت سفارش ناموفق بود: {exception.Message}";
            return;
        }
        finally
        {
            IsBusy = false;
        }

        await LoadOrdersAsync();
        if (SelectedOrder?.Id == orderId)
        {
            await LoadOrderDetailsAsync();
        }
    }

    private bool CanUseSelectedOrder() => SelectedOrder is not null && !IsBusy;

    private void NotifyCommandStates()
    {
        LoadOrdersCommand?.NotifyCanExecuteChanged();
        RefreshCommand?.NotifyCanExecuteChanged();
        LoadOrderDetailsCommand?.NotifyCanExecuteChanged();
        ConfirmOrderCommand?.NotifyCanExecuteChanged();
        SetPreparingCommand?.NotifyCanExecuteChanged();
        SetReadyCommand?.NotifyCanExecuteChanged();
        SetDeliveredCommand?.NotifyCanExecuteChanged();
        CancelOrderCommand?.NotifyCanExecuteChanged();
    }

    private void PollTimerOnTick(object? sender, EventArgs e)
    {
        if (!IsBusy)
        {
            RefreshCommand.Execute(null);
        }
    }
}
