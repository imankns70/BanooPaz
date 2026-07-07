using System.Collections.ObjectModel;
using System.Net.Http;
using BanooPaz.Contracts.Menus;
using BanooPaz.Contracts.Orders;
using BanooPaz.WPF.Models;
using BanooPaz.WPF.Services.Api;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BanooPaz.WPF.ViewModels;

public sealed class ManualOrderViewModel : ObservableObject
{
    private const string DefaultCity = "اندیمشک";

    private readonly IDailyMenusApiClient _menusApiClient;
    private readonly IOrdersApiClient _ordersApiClient;
    private DateTime _selectedDate = DateTime.Today;
    private SelectOption<DailyMenuItemDto>? _selectedMenuItem;
    private int _quantityToAdd = 1;
    private string _fullName = string.Empty;
    private string _phoneNumber = string.Empty;
    private string? _addressLine;
    private string? _customerNote;
    private SelectOption<PaymentMethod> _selectedPaymentMethod;
    private SelectOption<DeliveryMethod> _selectedDeliveryMethod;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;

    public ManualOrderViewModel(
        IDailyMenusApiClient menusApiClient,
        IOrdersApiClient ordersApiClient)
    {
        _menusApiClient = menusApiClient;
        _ordersApiClient = ordersApiClient;
        PaymentMethods =
        [
            new(PaymentMethod.CardToCard, "کارت به کارت"),
            new(PaymentMethod.Cash, "نقدی"),
            new(PaymentMethod.Online, "آنلاین")
        ];
        DeliveryMethods =
        [
            new(DeliveryMethod.Pickup, "تحویل حضوری"),
            new(DeliveryMethod.Delivery, "ارسال")
        ];
        _selectedPaymentMethod = PaymentMethods[0];
        _selectedDeliveryMethod = DeliveryMethods[0];

        LoadMenuCommand = new AsyncRelayCommand(LoadMenuAsync, () => !IsBusy);
        AddItemCommand = new RelayCommand(AddItem, () => SelectedMenuItem is not null && QuantityToAdd > 0 && !IsBusy);
        RemoveItemCommand = new RelayCommand<ManualOrderLineModel>(RemoveItem, item => item is not null && !IsBusy);
        SubmitOrderCommand = new AsyncRelayCommand(SubmitOrderAsync, () => !IsBusy);
        NewOrderCommand = new RelayCommand(ClearOrder, () => !IsBusy);
    }

    public ObservableCollection<SelectOption<DailyMenuItemDto>> MenuItems { get; } = [];
    public ObservableCollection<ManualOrderLineModel> Lines { get; } = [];
    public IReadOnlyList<SelectOption<PaymentMethod>> PaymentMethods { get; }
    public IReadOnlyList<SelectOption<DeliveryMethod>> DeliveryMethods { get; }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public SelectOption<DailyMenuItemDto>? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            if (SetProperty(ref _selectedMenuItem, value))
            {
                AddItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public int QuantityToAdd
    {
        get => _quantityToAdd;
        set
        {
            if (SetProperty(ref _quantityToAdd, value))
            {
                AddItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }
    public string? AddressLine { get => _addressLine; set => SetProperty(ref _addressLine, value); }
    public string? CustomerNote { get => _customerNote; set => SetProperty(ref _customerNote, value); }
    public SelectOption<PaymentMethod> SelectedPaymentMethod { get => _selectedPaymentMethod; set => SetProperty(ref _selectedPaymentMethod, value); }
    public SelectOption<DeliveryMethod> SelectedDeliveryMethod { get => _selectedDeliveryMethod; set => SetProperty(ref _selectedDeliveryMethod, value); }

    public decimal TotalAmount => Lines.Sum(line => line.TotalPrice);

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                NotifyCommands();
            }
        }
    }

    public string? ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string? SuccessMessage { get => _successMessage; private set => SetProperty(ref _successMessage, value); }

    public IAsyncRelayCommand LoadMenuCommand { get; }
    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand<ManualOrderLineModel> RemoveItemCommand { get; }
    public IAsyncRelayCommand SubmitOrderCommand { get; }
    public IRelayCommand NewOrderCommand { get; }

    private async Task LoadMenuAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var menu = await _menusApiClient.GetMenuByDateAsync(DateOnly.FromDateTime(SelectedDate));
            MenuItems.Clear();
            foreach (var item in menu?.Items.Where(item => item.IsAvailable) ?? [])
            {
                MenuItems.Add(new SelectOption<DailyMenuItemDto>(
                    item,
                    $"{item.FoodName} - {item.Price:N0} تومان - باقی‌مانده {item.RemainingPortions:N0}"));
            }

            SelectedMenuItem = MenuItems.FirstOrDefault();
            if (menu is null)
            {
                ErrorMessage = "برای این تاریخ منویی ثبت نشده است.";
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"بارگذاری منو برای ثبت سفارش ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddItem()
    {
        if (SelectedMenuItem is null || QuantityToAdd <= 0)
        {
            return;
        }

        var selectedItem = SelectedMenuItem.Value;
        var requestedQuantity = Lines
            .Where(line => line.DailyMenuItemId == selectedItem.Id)
            .Sum(line => line.Quantity) + QuantityToAdd;

        if (selectedItem.RemainingPortions <= 0)
        {
            ErrorMessage = "این غذا ظرفیت باقی‌مانده ندارد.";
            return;
        }

        if (requestedQuantity > selectedItem.RemainingPortions)
        {
            ErrorMessage = $"تعداد درخواستی از ظرفیت باقی‌مانده ({selectedItem.RemainingPortions:N0}) بیشتر است.";
            return;
        }

        var existing = Lines.SingleOrDefault(line => line.DailyMenuItemId == selectedItem.Id);
        if (existing is null)
        {
            var line = new ManualOrderLineModel
            {
                DailyMenuItemId = selectedItem.Id,
                FoodName = selectedItem.FoodName,
                UnitPrice = selectedItem.Price,
                Quantity = QuantityToAdd
            };
            line.PropertyChanged += (_, _) => OnPropertyChanged(nameof(TotalAmount));
            Lines.Add(line);
        }
        else
        {
            existing.Quantity += QuantityToAdd;
        }

        OnPropertyChanged(nameof(TotalAmount));
        ErrorMessage = null;
    }

    private void RemoveItem(ManualOrderLineModel? item)
    {
        if (item is null)
        {
            return;
        }

        Lines.Remove(item);
        OnPropertyChanged(nameof(TotalAmount));
    }

    private async Task SubmitOrderAsync()
    {
        if (!Validate())
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var order = await _ordersApiClient.CreateOrderAsync(new CreateOrderRequest
            {
                FullName = FullName.Trim(),
                PhoneNumber = PhoneNumber.Trim(),
                City = DefaultCity,
                AddressLine = SelectedDeliveryMethod.Value == DeliveryMethod.Delivery ? AddressLine?.Trim() : null,
                PaymentMethod = SelectedPaymentMethod.Value,
                DeliveryMethod = SelectedDeliveryMethod.Value,
                CustomerNote = CustomerNote,
                SaveAddress = false,
                Items = Lines.Select(line => new CreateOrderItemRequest
                {
                    DailyMenuItemId = line.DailyMenuItemId,
                    Quantity = line.Quantity
                }).ToList()
            });

            ClearOrder(keepSuccessMessage: false);
            SuccessMessage = $"سفارش {order.OrderNumber} با موفقیت ثبت شد.";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"ثبت سفارش ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            ErrorMessage = "نام مشتری الزامی است.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ErrorMessage = "شماره تماس الزامی است.";
            return false;
        }

        if (SelectedDeliveryMethod.Value == DeliveryMethod.Delivery && string.IsNullOrWhiteSpace(AddressLine))
        {
            ErrorMessage = "برای سفارش ارسالی، آدرس الزامی است.";
            return false;
        }

        if (Lines.Count == 0)
        {
            ErrorMessage = "حداقل یک آیتم به سفارش اضافه کنید.";
            return false;
        }

        if (Lines.Any(line => line.Quantity <= 0))
        {
            ErrorMessage = "تعداد آیتم‌ها باید بیشتر از صفر باشد.";
            return false;
        }

        return true;
    }

    private void ClearOrder() => ClearOrder(keepSuccessMessage: false);

    private void ClearOrder(bool keepSuccessMessage)
    {
        FullName = string.Empty;
        PhoneNumber = string.Empty;
        AddressLine = null;
        CustomerNote = null;
        Lines.Clear();
        OnPropertyChanged(nameof(TotalAmount));
        ErrorMessage = null;
        if (!keepSuccessMessage)
        {
            SuccessMessage = null;
        }
    }

    private void NotifyCommands()
    {
        LoadMenuCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
        SubmitOrderCommand.NotifyCanExecuteChanged();
        NewOrderCommand.NotifyCanExecuteChanged();
    }
}
