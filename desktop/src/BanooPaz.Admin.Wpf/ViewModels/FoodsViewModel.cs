using System.Collections.ObjectModel;
using System.Net.Http;
using BanooPaz.Admin.Wpf.Services.Api;
using BanooPaz.Contracts.Foods;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BanooPaz.Admin.Wpf.ViewModels;

public sealed class FoodsViewModel : ObservableObject
{
    private readonly IFoodsApiClient _apiClient;
    private FoodDto? _selectedFood;
    private string _foodName = string.Empty;
    private string? _foodDescription;
    private decimal _defaultPrice;
    private string? _imageUrl;
    private bool _isActive = true;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;

    public FoodsViewModel(IFoodsApiClient apiClient)
    {
        _apiClient = apiClient;
        LoadFoodsCommand = new AsyncRelayCommand(LoadFoodsAsync, () => !IsBusy);
        RefreshCommand = new AsyncRelayCommand(LoadFoodsAsync, () => !IsBusy);
        NewFoodCommand = new RelayCommand(NewFood, () => !IsBusy);
        SaveFoodCommand = new AsyncRelayCommand(SaveFoodAsync, () => !IsBusy);
        ToggleActiveCommand = new AsyncRelayCommand(ToggleActiveAsync, () => SelectedFood is not null && !IsBusy);
    }

    public ObservableCollection<FoodDto> Foods { get; } = [];
    public FoodDto? SelectedFood
    {
        get => _selectedFood;
        set
        {
            if (!SetProperty(ref _selectedFood, value)) return;
            if (value is not null)
            {
                FoodName = value.Name;
                FoodDescription = value.Description;
                DefaultPrice = value.DefaultPrice;
                ImageUrl = value.ImageUrl;
                IsActive = value.IsActive;
            }
            NotifyCommands();
        }
    }
    public string FoodName { get => _foodName; set => SetProperty(ref _foodName, value); }
    public string? FoodDescription { get => _foodDescription; set => SetProperty(ref _foodDescription, value); }
    public decimal DefaultPrice { get => _defaultPrice; set => SetProperty(ref _defaultPrice, value); }
    public string? ImageUrl { get => _imageUrl; set => SetProperty(ref _imageUrl, value); }
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
    public bool IsBusy
    {
        get => _isBusy;
        private set { if (SetProperty(ref _isBusy, value)) NotifyCommands(); }
    }
    public string? ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string? SuccessMessage { get => _successMessage; private set => SetProperty(ref _successMessage, value); }

    public IAsyncRelayCommand LoadFoodsCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand NewFoodCommand { get; }
    public IAsyncRelayCommand SaveFoodCommand { get; }
    public IAsyncRelayCommand ToggleActiveCommand { get; }

    private async Task LoadFoodsAsync() => await LoadFoodsAsync(SelectedFood?.Id);

    private async Task LoadFoodsAsync(int? selectId)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var foods = await _apiClient.GetFoodsAsync();
            Foods.Clear();
            foreach (var food in foods) Foods.Add(food);
            SelectedFood = selectId.HasValue ? Foods.FirstOrDefault(food => food.Id == selectId) : null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"دریافت غذاها ناموفق بود: {exception.Message}";
        }
        finally { IsBusy = false; }
    }

    private void NewFood()
    {
        SelectedFood = null;
        FoodName = string.Empty;
        FoodDescription = null;
        DefaultPrice = 0;
        ImageUrl = null;
        IsActive = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private async Task SaveFoodAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(FoodName))
        {
            ErrorMessage = "نام غذا الزامی است.";
            return;
        }
        if (DefaultPrice < 0)
        {
            ErrorMessage = "قیمت پیش‌فرض نمی‌تواند منفی باشد.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;
        int? selectId = SelectedFood?.Id;
        try
        {
            if (SelectedFood is null)
            {
                var created = await _apiClient.CreateFoodAsync(new CreateFoodRequest
                {
                    Name = FoodName.Trim(), Description = FoodDescription,
                    DefaultPrice = DefaultPrice, ImageUrl = ImageUrl
                });
                selectId = created.Id;
            }
            else
            {
                await _apiClient.UpdateFoodAsync(SelectedFood.Id, new UpdateFoodRequest
                {
                    Name = FoodName.Trim(), Description = FoodDescription,
                    DefaultPrice = DefaultPrice, ImageUrl = ImageUrl, IsActive = IsActive
                });
            }
            SuccessMessage = "اطلاعات غذا با موفقیت ذخیره شد.";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"ذخیره غذا ناموفق بود: {exception.Message}";
            return;
        }
        finally { IsBusy = false; }

        await LoadFoodsAsync(selectId);
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedFood is null || IsBusy) return;
        var id = SelectedFood.Id;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _apiClient.SetFoodActiveAsync(id, !SelectedFood.IsActive);
            SuccessMessage = "وضعیت غذا به‌روزرسانی شد.";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"تغییر وضعیت غذا ناموفق بود: {exception.Message}";
            return;
        }
        finally { IsBusy = false; }
        await LoadFoodsAsync(id);
    }

    private void NotifyCommands()
    {
        LoadFoodsCommand?.NotifyCanExecuteChanged(); RefreshCommand?.NotifyCanExecuteChanged();
        NewFoodCommand?.NotifyCanExecuteChanged(); SaveFoodCommand?.NotifyCanExecuteChanged();
        ToggleActiveCommand?.NotifyCanExecuteChanged();
    }
}
