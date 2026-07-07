using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using BanooPaz.WPF.Models;
using BanooPaz.WPF.Services.Api;
using BanooPaz.Contracts.Foods;
using BanooPaz.Contracts.Menus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BanooPaz.WPF.ViewModels;

public sealed class DailyMenuViewModel : ObservableObject
{
    private readonly IDailyMenusApiClient _menusApiClient;
    private readonly IFoodsApiClient _foodsApiClient;
    private DateTime _selectedDate = DateTime.Today;
    private bool _isOpen = true;
    private string? _note;
    private FoodDto? _selectedFoodToAdd;
    private decimal _priceToAdd;
    private int _capacityToAdd;
    private bool _itemIsAvailableForEdit = true;
    private bool _hasUnsavedChanges;
    private int _isSaving;
    private bool _isAddItemPopupVisible;
    private DailyMenuItemEditModel? _editingItem;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;

    public DailyMenuViewModel(IDailyMenusApiClient menusApiClient, IFoodsApiClient foodsApiClient)
    {
        _menusApiClient = menusApiClient;
        _foodsApiClient = foodsApiClient;
        LoadMenuCommand = new AsyncRelayCommand(LoadMenuAsync, () => !IsBusy);
        RefreshCommand = new AsyncRelayCommand(LoadMenuAsync, () => !IsBusy);
        OpenAddItemPopupCommand = new AsyncRelayCommand(OpenAddItemPopupAsync, () => !IsBusy);
        CloseAddItemPopupCommand = new RelayCommand(CloseAddItemPopup, () => !IsBusy);
        SavePopupItemCommand = new AsyncRelayCommand(SavePopupItemAsync, () => SelectedFoodToAdd is not null && !IsBusy);
        EditItemCommand = new RelayCommand<DailyMenuItemEditModel>(OpenEditItemPopup, item => item is not null && !IsBusy);
        SaveMenuCommand = new AsyncRelayCommand(SaveMenuAsync, () => !IsBusy);
        RemoveItemCommand = new AsyncRelayCommand<DailyMenuItemEditModel>(RemoveItemAsync, item => item is not null && !IsBusy);
    }

    public ObservableCollection<FoodDto> AvailableFoods { get; } = [];
    public ObservableCollection<DailyMenuItemEditModel> Items { get; } = [];
    public DateTime SelectedDate { get => _selectedDate; set => SetProperty(ref _selectedDate, value); }
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (SetProperty(ref _isOpen, value))
            {
                MarkUnsaved();
            }
        }
    }
    public string? Note
    {
        get => _note;
        set
        {
            if (SetProperty(ref _note, value))
            {
                MarkUnsaved();
            }
        }
    }
    public decimal PriceToAdd { get => _priceToAdd; set => SetProperty(ref _priceToAdd, value); }
    public int CapacityToAdd { get => _capacityToAdd; set => SetProperty(ref _capacityToAdd, value); }
    public bool ItemIsAvailableForEdit { get => _itemIsAvailableForEdit; set => SetProperty(ref _itemIsAvailableForEdit, value); }
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
    public bool HasUnsavedChanges { get => _hasUnsavedChanges; private set => SetProperty(ref _hasUnsavedChanges, value); }
    public bool IsEditingItem => _editingItem is not null;
    public bool IsFoodSelectionEnabled => !IsEditingItem;
    public string PopupTitle => IsEditingItem ? "ویرایش آیتم منوی روزانه" : "افزودن غذا به منوی روزانه";
    public string PopupSubmitText => IsEditingItem ? "ذخیره" : "ثبت";
    public bool IsAddItemPopupVisible
    {
        get => _isAddItemPopupVisible;
        private set
        {
            SetProperty(ref _isAddItemPopupVisible, value);
        }
    }

    public IAsyncRelayCommand LoadMenuCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand OpenAddItemPopupCommand { get; }
    public IRelayCommand CloseAddItemPopupCommand { get; }
    public IAsyncRelayCommand SavePopupItemCommand { get; }
    public IRelayCommand<DailyMenuItemEditModel> EditItemCommand { get; }
    public IAsyncRelayCommand<DailyMenuItemEditModel> RemoveItemCommand { get; }
    public IAsyncRelayCommand SaveMenuCommand { get; }

    private async Task LoadMenuAsync()
    {
        if (IsBusy) return;
        if (HasUnsavedChanges)
        {
            ErrorMessage = "تغییرات ذخیره‌نشده دارید. ابتدا منو را ذخیره کنید؛ سپس دوباره بارگذاری کنید.";
            return;
        }

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
            if (AvailableFoods.Count == 0)
            {
                ErrorMessage = "هیچ غذای فعالی برای افزودن به منو وجود ندارد. ابتدا از صفحه غذاها، غذا ثبت یا فعال کنید.";
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"بارگذاری منوی روزانه ناموفق بود: {exception.Message}";
        }
        finally { IsBusy = false; }
    }

    private async Task OpenAddItemPopupAsync()
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = null;
        SuccessMessage = null;

        if (AvailableFoods.Count == 0)
        {
            await LoadActiveFoodsAsync();
        }

        _editingItem = null;
        OnPopupModeChanged();
        PriceToAdd = 0;
        CapacityToAdd = 0;
        ItemIsAvailableForEdit = true;
        SelectedFoodToAdd ??= AvailableFoods.FirstOrDefault();
        IsAddItemPopupVisible = true;
    }

    private void CloseAddItemPopup()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        _editingItem = null;
        OnPopupModeChanged();
        IsAddItemPopupVisible = false;
    }

    private void OpenEditItemPopup(DailyMenuItemEditModel? item)
    {
        if (item is null || IsBusy)
        {
            return;
        }

        _editingItem = item;
        OnPopupModeChanged();
        SelectedFoodToAdd = AvailableFoods.FirstOrDefault(food => food.Id == item.FoodId);
        if (SelectedFoodToAdd is null)
        {
            SelectedFoodToAdd = new FoodDto { Id = item.FoodId, Name = item.FoodName, IsActive = true };
            AvailableFoods.Add(SelectedFoodToAdd);
        }
        PriceToAdd = item.Price;
        CapacityToAdd = item.CapacityPortions;
        ItemIsAvailableForEdit = item.IsAvailable;
        ErrorMessage = null;
        SuccessMessage = null;
        IsAddItemPopupVisible = true;
    }

    private void ApplyMenu(DailyMenuDto? menu)
    {
        ClearMenuItems();
        SetProperty(ref _isOpen, menu?.IsOpen ?? true, nameof(IsOpen));
        SetProperty(ref _note, menu?.Note, nameof(Note));
        if (menu is null)
        {
            HasUnsavedChanges = false;
            return;
        }
        foreach (var item in menu.Items)
        {
            AddMenuItem(new DailyMenuItemEditModel
            {
                Id = item.Id, FoodId = item.FoodId, FoodName = item.FoodName,
                Price = item.Price, SoldPortions = item.SoldPortions,
                CapacityPortions = item.CapacityPortions, IsAvailable = item.IsAvailable
            }, markUnsaved: false);
        }

        HasUnsavedChanges = false;
    }

    private async Task SavePopupItemAsync()
    {
        if (_editingItem is not null)
        {
            await UpdateSelectedItemAsync(_editingItem);
            return;
        }

        await AddSelectedFoodAsync();
    }

    private async Task AddSelectedFoodAsync()
    {
        if (SelectedFoodToAdd is null) return;
        if (IsBusy) return;
        if (HasUnsavedChanges)
        {
            ErrorMessage = "ابتدا تغییرات ذخیره‌نشده را ذخیره کنید؛ سپس غذای جدید به منو اضافه کنید.";
            return;
        }

        if (Items.Any(item => item.FoodId == SelectedFoodToAdd.Id))
        {
            ErrorMessage = "این غذا قبلاً به منوی روز اضافه شده است.";
            return;
        }
        if (PriceToAdd < 0)
        {
            ErrorMessage = "قیمت امروز نمی‌تواند منفی باشد.";
            return;
        }
        if (CapacityToAdd < 0)
        {
            ErrorMessage = "ظرفیت پرس نمی‌تواند منفی باشد.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            var saved = await _menusApiClient.AddMenuItemAsync(
                DateOnly.FromDateTime(SelectedDate),
                new UpsertDailyMenuItemRequest
                {
                    FoodId = SelectedFoodToAdd.Id,
                    Price = PriceToAdd,
                    CapacityPortions = CapacityToAdd,
                    IsAvailable = true
                });
            ApplyMenu(saved);
            PriceToAdd = 0;
            CapacityToAdd = 0;
            IsAddItemPopupVisible = false;
            SuccessMessage = "غذا به منوی روزانه اضافه و ذخیره شد.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"افزودن غذا به منوی روزانه ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateSelectedItemAsync(DailyMenuItemEditModel item)
    {
        if (!item.Id.HasValue || IsBusy)
        {
            return;
        }

        if (PriceToAdd < 0)
        {
            ErrorMessage = "قیمت امروز نمی‌تواند منفی باشد.";
            return;
        }

        if (CapacityToAdd < 0)
        {
            ErrorMessage = "ظرفیت پرس نمی‌تواند منفی باشد.";
            return;
        }

        if (CapacityToAdd < item.SoldPortions)
        {
            ErrorMessage = "ظرفیت نمی‌تواند کمتر از تعداد فروخته‌شده باشد.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            var saved = await _menusApiClient.UpdateMenuItemAsync(
                item.Id.Value,
                new UpdateDailyMenuItemRequest
                {
                    Price = PriceToAdd,
                    CapacityPortions = CapacityToAdd,
                    IsAvailable = ItemIsAvailableForEdit
                });
            ApplyMenu(saved);
            _editingItem = null;
            OnPopupModeChanged();
            IsAddItemPopupVisible = false;
            SuccessMessage = "آیتم منوی روزانه ویرایش شد.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"ویرایش آیتم منوی روزانه ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveItemAsync(DailyMenuItemEditModel? item)
    {
        if (item is null || IsBusy)
        {
            return;
        }

        if (item.SoldPortions > 0)
        {
            ErrorMessage = "آیتمی که فروش داشته باشد قابل حذف از منو نیست. می‌توانید آن را غیرفعال کنید.";
            return;
        }

        if (!item.Id.HasValue)
        {
            item.PropertyChanged -= MenuItemOnPropertyChanged;
            Items.Remove(item);
            HasUnsavedChanges = false;
            ErrorMessage = null;
            SuccessMessage = "آیتم حذف شد.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            var saved = await _menusApiClient.DeleteMenuItemAsync(item.Id.Value);
            ApplyMenu(saved);
            SuccessMessage = "آیتم منوی روزانه حذف شد.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"حذف آیتم منوی روزانه ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveMenuAsync()
    {
        if (IsBusy || Interlocked.Exchange(ref _isSaving, 1) == 1)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var saved = await _menusApiClient.UpdateMenuSettingsAsync(
                DateOnly.FromDateTime(SelectedDate),
                new UpdateDailyMenuSettingsRequest
            {
                IsOpen = IsOpen,
                Note = Note
            });
            ApplyMenu(saved);
            HasUnsavedChanges = false;
            SuccessMessage = "تنظیمات منوی روزانه ذخیره شد.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"ذخیره تنظیمات منوی روزانه ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
            Interlocked.Exchange(ref _isSaving, 0);
        }
    }

    private void NotifyCommands()
    {
        LoadMenuCommand?.NotifyCanExecuteChanged(); RefreshCommand?.NotifyCanExecuteChanged();
        OpenAddItemPopupCommand?.NotifyCanExecuteChanged(); CloseAddItemPopupCommand?.NotifyCanExecuteChanged();
        SavePopupItemCommand?.NotifyCanExecuteChanged(); EditItemCommand?.NotifyCanExecuteChanged();
        RemoveItemCommand?.NotifyCanExecuteChanged(); SaveMenuCommand?.NotifyCanExecuteChanged();
    }

    private void OnPopupModeChanged()
    {
        OnPropertyChanged(nameof(IsEditingItem));
        OnPropertyChanged(nameof(IsFoodSelectionEnabled));
        OnPropertyChanged(nameof(PopupTitle));
        OnPropertyChanged(nameof(PopupSubmitText));
        NotifyCommands();
    }

    private async Task LoadActiveFoodsAsync()
    {
        IsBusy = true;
        try
        {
            AvailableFoods.Clear();
            foreach (var food in (await _foodsApiClient.GetFoodsAsync()).Where(food => food.IsActive))
            {
                AvailableFoods.Add(food);
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"بارگذاری فهرست غذاها ناموفق بود: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddMenuItem(DailyMenuItemEditModel item, bool markUnsaved)
    {
        item.PropertyChanged += MenuItemOnPropertyChanged;
        Items.Add(item);
        if (markUnsaved)
        {
            MarkUnsaved();
        }
    }

    private void ClearMenuItems()
    {
        foreach (var item in Items)
        {
            item.PropertyChanged -= MenuItemOnPropertyChanged;
        }

        Items.Clear();
    }

    private void MenuItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DailyMenuItemEditModel.RemainingPortions))
        {
            return;
        }

        MarkUnsaved();
    }

    private void MarkUnsaved()
    {
        HasUnsavedChanges = true;
        SuccessMessage = null;
    }
}
