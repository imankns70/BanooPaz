using System.Collections.ObjectModel;
using System.Net.Http;
using BanooPaz.Admin.Wpf.Models;
using BanooPaz.Admin.Wpf.Services.Api;
using BanooPaz.Contracts.Foods;
using BanooPaz.Contracts.Menus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BanooPaz.Admin.Wpf.ViewModels;

public sealed class DailyMenuViewModel : ObservableObject
{
    private readonly IDailyMenusApiClient _menusApiClient;
    private readonly IFoodsApiClient _foodsApiClient;
    private DateTime _selectedDate = DateTime.Today;
    private bool _isOpen = true;
    private string? _note;
    private FoodDto? _selectedFoodToAdd;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;

    public DailyMenuViewModel(IDailyMenusApiClient menusApiClient, IFoodsApiClient foodsApiClient)
    {
        _menusApiClient = menusApiClient;
        _foodsApiClient = foodsApiClient;
        LoadMenuCommand = new AsyncRelayCommand(LoadMenuAsync, () => !IsBusy);
        RefreshCommand = new AsyncRelayCommand(LoadMenuAsync, () => !IsBusy);
        AddSelectedFoodCommand = new RelayCommand(AddSelectedFood, () => SelectedFoodToAdd is not null && !IsBusy);
        SaveMenuCommand = new AsyncRelayCommand(SaveMenuAsync, () => !IsBusy);
        ToggleItemAvailabilityCommand = new AsyncRelayCommand<DailyMenuItemEditModel>(ToggleAvailabilityAsync, item => item is not null && !IsBusy);
    }

    public ObservableCollection<FoodDto> AvailableFoods { get; } = [];
    public ObservableCollection<DailyMenuItemEditModel> Items { get; } = [];
    public DateTime SelectedDate { get => _selectedDate; set => SetProperty(ref _selectedDate, value); }
    public bool IsOpen { get => _isOpen; set => SetProperty(ref _isOpen, value); }
    public string? Note { get => _note; set => SetProperty(ref _note, value); }
    public FoodDto? SelectedFoodToAdd
    {
        get => _selectedFoodToAdd;
        set { if (SetProperty(ref _selectedFoodToAdd, value)) NotifyCommands(); }
    }
    public bool IsBusy
    {
        get => _isBusy;
        private set { if (SetProperty(ref _isBusy, value)) NotifyCommands(); }
    }
    public string? ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string? SuccessMessage { get => _successMessage; private set => SetProperty(ref _successMessage, value); }

    public IAsyncRelayCommand LoadMenuCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand AddSelectedFoodCommand { get; }
    public IAsyncRelayCommand SaveMenuCommand { get; }
    public IAsyncRelayCommand<DailyMenuItemEditModel> ToggleItemAvailabilityCommand { get; }

    private async Task LoadMenuAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            var foodsTask = _foodsApiClient.GetFoodsAsync();
            var menuTask = _menusApiClient.GetMenuByDateAsync(DateOnly.FromDateTime(SelectedDate));
            await Task.WhenAll(foodsTask, menuTask);

            AvailableFoods.Clear();
            foreach (var food in (await foodsTask).Where(food => food.IsActive)) AvailableFoods.Add(food);
            SelectedFoodToAdd = AvailableFoods.FirstOrDefault();

            ApplyMenu(await menuTask);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"بارگذاری منوی روزانه ناموفق بود: {exception.Message}";
        }
        finally { IsBusy = false; }
    }

    private void ApplyMenu(DailyMenuDto? menu)
    {
        Items.Clear();
        IsOpen = menu?.IsOpen ?? true;
        Note = menu?.Note;
        if (menu is null) return;
        foreach (var item in menu.Items)
        {
            Items.Add(new DailyMenuItemEditModel
            {
                Id = item.Id, FoodId = item.FoodId, FoodName = item.FoodName,
                Price = item.Price, SoldPortions = item.SoldPortions,
                CapacityPortions = item.CapacityPortions, IsAvailable = item.IsAvailable
            });
        }
    }

    private void AddSelectedFood()
    {
        if (SelectedFoodToAdd is null) return;
        if (Items.Any(item => item.FoodId == SelectedFoodToAdd.Id))
        {
            ErrorMessage = "این غذا قبلاً به منوی روز اضافه شده است.";
            return;
        }
        Items.Add(new DailyMenuItemEditModel
        {
            FoodId = SelectedFoodToAdd.Id, FoodName = SelectedFoodToAdd.Name,
            Price = SelectedFoodToAdd.DefaultPrice, CapacityPortions = 0,
            SoldPortions = 0, IsAvailable = true
        });
        ErrorMessage = null;
    }

    private async Task SaveMenuAsync()
    {
        if (IsBusy || !ValidateItems()) return;
        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            var saved = await _menusApiClient.CreateOrUpdateMenuAsync(new CreateOrUpdateDailyMenuRequest
            {
                MenuDate = DateOnly.FromDateTime(SelectedDate), IsOpen = IsOpen, Note = Note,
                Items = Items.Select(item => new UpsertDailyMenuItemRequest
                {
                    Id = item.Id, FoodId = item.FoodId, Price = item.Price,
                    CapacityPortions = item.CapacityPortions, IsAvailable = item.IsAvailable
                }).ToList()
            });
            ApplyMenu(saved);
            SuccessMessage = "منوی روزانه با موفقیت ذخیره شد.";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"ذخیره منوی روزانه ناموفق بود: {exception.Message}";
        }
        finally { IsBusy = false; }
    }

    private bool ValidateItems()
    {
        if (Items.GroupBy(item => item.FoodId).Any(group => group.Count() > 1))
        {
            ErrorMessage = "هر غذا فقط یک بار می‌تواند در منوی روز باشد.";
            return false;
        }
        var invalidPrice = Items.FirstOrDefault(item => item.Price < 0);
        if (invalidPrice is not null)
        {
            ErrorMessage = $"قیمت {invalidPrice.FoodName} نمی‌تواند منفی باشد.";
            return false;
        }
        var invalidCapacity = Items.FirstOrDefault(item => item.CapacityPortions < 0);
        if (invalidCapacity is not null)
        {
            ErrorMessage = $"ظرفیت {invalidCapacity.FoodName} نمی‌تواند منفی باشد.";
            return false;
        }
        var belowSold = Items.FirstOrDefault(item => item.Id.HasValue && item.CapacityPortions < item.SoldPortions);
        if (belowSold is not null)
        {
            ErrorMessage = $"ظرفیت {belowSold.FoodName} نمی‌تواند کمتر از تعداد فروخته‌شده باشد.";
            return false;
        }
        return true;
    }

    private async Task ToggleAvailabilityAsync(DailyMenuItemEditModel? item)
    {
        if (item is null || IsBusy) return;
        var newValue = !item.IsAvailable;
        if (!item.Id.HasValue)
        {
            item.IsAvailable = newValue;
            return;
        }
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _menusApiClient.SetMenuItemAvailabilityAsync(item.Id.Value, newValue);
            item.IsAvailable = newValue;
            SuccessMessage = "وضعیت آیتم منو به‌روزرسانی شد.";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"تغییر وضعیت آیتم ناموفق بود: {exception.Message}";
        }
        finally { IsBusy = false; }
    }

    private void NotifyCommands()
    {
        LoadMenuCommand?.NotifyCanExecuteChanged(); RefreshCommand?.NotifyCanExecuteChanged();
        AddSelectedFoodCommand?.NotifyCanExecuteChanged(); SaveMenuCommand?.NotifyCanExecuteChanged();
        ToggleItemAvailabilityCommand?.NotifyCanExecuteChanged();
    }
}
