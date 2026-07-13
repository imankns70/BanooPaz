using BanooPaz.WPF.Models;
using BanooPaz.WPF.Services.Api;
using BanooPaz.WPF.ViewModels;
using BanooPaz.WPF.Converters;
using BanooPaz.Contracts.Foods;
using BanooPaz.Contracts.Menus;
using BanooPaz.Contracts.Orders;

namespace BanooPaz.Admin.UnitTests;

public sealed class AdminViewModelValidationTests
{
    [Fact]
    public async Task Food_name_is_required_before_api_call()
    {
        var api = new FakeFoodsApiClient();
        var viewModel = new FoodsViewModel(api) { FoodName = "  " };

        await viewModel.SaveFoodCommand.ExecuteAsync(null);

        Assert.Contains("نام غذا", viewModel.ErrorMessage);
        Assert.Equal(0, api.CreateCalls);
    }

    [Fact]
    public async Task Food_save_sends_zero_legacy_default_price()
    {
        var api = new FakeFoodsApiClient();
        var viewModel = new FoodsViewModel(api) { FoodName = "Test" };

        await viewModel.SaveFoodCommand.ExecuteAsync(null);

        Assert.Null(viewModel.ErrorMessage);
        Assert.Equal(1, api.CreateCalls);
        Assert.Equal(0, api.LastCreateRequest?.DefaultPrice);
    }

    [Fact]
    public async Task Daily_menu_prevents_duplicate_food_addition()
    {
        var viewModel = CreateDailyMenuViewModel();
        var food = new FoodDto { Id = 1, Name = "Test" };
        viewModel.AvailableFoods.Add(food);
        viewModel.SelectedFoodToAdd = food;

        await viewModel.SavePopupItemCommand.ExecuteAsync(null);
        await viewModel.SavePopupItemCommand.ExecuteAsync(null);

        Assert.Single(viewModel.Items);
        Assert.Contains("قبلاً", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Daily_menu_adds_selected_food_with_entered_price_and_capacity()
    {
        var menusApi = new FakeDailyMenusApiClient();
        var viewModel = CreateDailyMenuViewModel(menusApi);
        var food = new FoodDto { Id = 1, Name = "قیمه", DefaultPrice = 999 };
        viewModel.AvailableFoods.Add(food);
        viewModel.SelectedFoodToAdd = food;
        viewModel.PriceToAdd = 150_000;
        viewModel.CapacityToAdd = 12;

        await viewModel.SavePopupItemCommand.ExecuteAsync(null);

        var item = Assert.Single(viewModel.Items);
        Assert.Equal(150_000, item.Price);
        Assert.Equal(12, item.CapacityPortions);
        Assert.Equal(1, menusApi.AddCalls);
    }

    [Fact]
    public async Task Daily_menu_add_button_opens_add_popup_and_loads_foods()
    {
        var foodsApi = new FakeFoodsApiClient
        {
            Foods =
            [
                new FoodDto { Id = 1, Name = "فعال", IsActive = true },
                new FoodDto { Id = 2, Name = "غیرفعال", IsActive = false }
            ]
        };
        var viewModel = CreateDailyMenuViewModel(foodsApi: foodsApi);

        await viewModel.OpenAddItemPopupCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsAddItemPopupVisible);
        Assert.Single(viewModel.AvailableFoods);
        Assert.Equal("فعال", viewModel.SelectedFoodToAdd?.Name);
    }

    [Fact]
    public async Task Daily_menu_add_item_closes_popup()
    {
        var menusApi = new FakeDailyMenusApiClient();
        var viewModel = CreateDailyMenuViewModel(menusApi);
        var food = new FoodDto { Id = 1, Name = "قیمه", IsActive = true };
        viewModel.AvailableFoods.Add(food);
        viewModel.SelectedFoodToAdd = food;
        viewModel.PriceToAdd = 150_000;
        viewModel.CapacityToAdd = 12;

        await viewModel.OpenAddItemPopupCommand.ExecuteAsync(null);
        await viewModel.SavePopupItemCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsAddItemPopupVisible);
        Assert.Single(viewModel.Items);
    }

    [Fact]
    public async Task Daily_menu_edit_button_opens_popup_and_updates_item()
    {
        var menusApi = new FakeDailyMenusApiClient
        {
            MenuAfterUpdate = new DailyMenuDto
            {
                MenuDate = new DateOnly(2026, 7, 7),
                Items =
                [
                    new DailyMenuItemDto
                    {
                        Id = 7,
                        FoodId = 1,
                        FoodName = "قیمه",
                        Price = 180_000,
                        CapacityPortions = 9,
                        IsAvailable = true
                    }
                ]
            }
        };
        var viewModel = CreateDailyMenuViewModel(menusApi);
        viewModel.Items.Add(new DailyMenuItemEditModel
        {
            Id = 7,
            FoodId = 1,
            FoodName = "قیمه",
            Price = 150_000,
            CapacityPortions = 8,
            IsAvailable = true
        });

        viewModel.EditItemCommand.Execute(viewModel.Items.Single());
        viewModel.PriceToAdd = 180_000;
        viewModel.CapacityToAdd = 9;
        viewModel.ItemIsAvailableForEdit = false;
        await viewModel.SavePopupItemCommand.ExecuteAsync(null);

        Assert.Equal(1, menusApi.UpdateCalls);
        Assert.Equal(180_000, menusApi.LastUpdateRequest?.Price);
        Assert.Equal(9, menusApi.LastUpdateRequest?.CapacityPortions);
        Assert.False(menusApi.LastUpdateRequest?.IsAvailable);
        Assert.False(viewModel.IsAddItemPopupVisible);
        var item = Assert.Single(viewModel.Items);
        Assert.Equal(180_000, item.Price);
        Assert.Equal(9, item.CapacityPortions);
    }

    [Fact]
    public async Task Daily_menu_does_not_delete_sold_items_locally()
    {
        var menusApi = new FakeDailyMenusApiClient();
        var viewModel = CreateDailyMenuViewModel(menusApi);
        viewModel.Items.Add(new DailyMenuItemEditModel
        {
            Id = 5, FoodId = 1, FoodName = "Test", SoldPortions = 3, CapacityPortions = 5
        });

        await viewModel.RemoveItemCommand.ExecuteAsync(viewModel.Items.Single());

        Assert.Single(viewModel.Items);
        Assert.Contains("فروش", viewModel.ErrorMessage);
        Assert.Equal(0, menusApi.DeleteCalls);
    }

    [Fact]
    public async Task Daily_menu_save_updates_settings_without_using_replacement_save()
    {
        var menusApi = new FakeDailyMenusApiClient
        {
            MenuAfterSave = new DailyMenuDto
            {
                MenuDate = new DateOnly(2026, 7, 7),
                IsOpen = false,
                Note = "closed today",
                Items =
                [
                    new DailyMenuItemDto
                    {
                        Id = 11,
                        FoodId = 2,
                        FoodName = "قیمه",
                        Price = 150_000,
                        CapacityPortions = 8,
                        IsAvailable = true
                    }
                ]
            }
        };
        var viewModel = CreateDailyMenuViewModel(menusApi);
        viewModel.IsOpen = false;
        viewModel.Note = "closed today";

        await viewModel.SaveMenuCommand.ExecuteAsync(null);

        Assert.Equal(0, menusApi.SaveCalls);
        Assert.Equal(1, menusApi.SettingsCalls);
        Assert.False(menusApi.LastSettingsRequest?.IsOpen);
        Assert.Equal("closed today", menusApi.LastSettingsRequest?.Note);
        var item = Assert.Single(viewModel.Items);
        Assert.Equal(11, item.Id);
        Assert.Equal("قیمه", item.FoodName);
    }

    [Fact]
    public async Task Daily_menu_settings_save_does_not_clear_existing_items()
    {
        var menusApi = new FakeDailyMenusApiClient
        {
            MenuAfterSave = new DailyMenuDto
            {
                MenuDate = new DateOnly(2026, 7, 7),
                Items =
                [
                    new DailyMenuItemDto
                    {
                        Id = 11,
                        FoodId = 2,
                        FoodName = "قیمه",
                        Price = 150_000,
                        CapacityPortions = 8,
                        IsAvailable = true
                    }
                ]
            }
        };
        var viewModel = CreateDailyMenuViewModel(menusApi);

        await viewModel.LoadMenuCommand.ExecuteAsync(null);
        viewModel.Items.Clear();
        await viewModel.SaveMenuCommand.ExecuteAsync(null);

        Assert.Equal(0, menusApi.SaveCalls);
        Assert.Equal(1, menusApi.SettingsCalls);
        Assert.Null(viewModel.ErrorMessage);
        Assert.Single(viewModel.Items);
    }

    [Fact]
    public async Task Daily_menu_ignores_repeated_save_while_save_is_running()
    {
        var menusApi = new FakeDailyMenusApiClient { HoldSave = true };
        var viewModel = CreateDailyMenuViewModel(menusApi);
        viewModel.Items.Add(new DailyMenuItemEditModel
        {
            FoodId = 2,
            FoodName = "قیمه",
            Price = 150_000,
            CapacityPortions = 8,
            IsAvailable = true
        });

        var firstSave = viewModel.SaveMenuCommand.ExecuteAsync(null);
        await menusApi.WaitForSaveStartedAsync();
        var secondSave = viewModel.SaveMenuCommand.ExecuteAsync(null);
        menusApi.ReleaseSave();
        await Task.WhenAll(firstSave, secondSave);

        Assert.Equal(1, menusApi.SettingsCalls);
    }

    [Fact]
    public async Task Daily_menu_save_exception_is_shown_as_error_message()
    {
        var menusApi = new FakeDailyMenusApiClient
        {
            SettingsException = new InvalidOperationException("Save failed")
        };
        var viewModel = CreateDailyMenuViewModel(menusApi);

        await viewModel.SaveMenuCommand.ExecuteAsync(null);

        Assert.Contains("Save failed", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Daily_menu_does_not_reload_over_unsaved_edited_items()
    {
        var menusApi = new FakeDailyMenusApiClient
        {
            MenuAfterSave = new DailyMenuDto
            {
                MenuDate = new DateOnly(2026, 7, 7),
                Items =
                [
                    new DailyMenuItemDto
                    {
                        Id = 11,
                        FoodId = 1,
                        FoodName = "قیمه",
                        Price = 150_000,
                        CapacityPortions = 12,
                        IsAvailable = true
                    }
                ]
            }
        };
        var viewModel = CreateDailyMenuViewModel(menusApi);

        await viewModel.LoadMenuCommand.ExecuteAsync(null);
        viewModel.Items.Single().Price = 160_000;
        await viewModel.LoadMenuCommand.ExecuteAsync(null);

        var item = Assert.Single(viewModel.Items);
        Assert.Equal(160_000, item.Price);
        Assert.Contains("ذخیره‌نشده", viewModel.ErrorMessage);
        Assert.Equal(1, menusApi.GetCalls);
    }

    [Fact]
    public void Thousands_separator_converter_uses_comma_grouping()
    {
        var converter = new ThousandsSeparatorConverter();

        var text = converter.Convert(150_000m, typeof(string), null, new("fa-IR"));

        Assert.Equal("150,000", text);
    }

    [Fact]
    public void Manual_order_add_does_not_increase_existing_food_quantity()
    {
        var viewModel = new ManualOrderViewModel(new FakeDailyMenusApiClient(), new FakeOrdersApiClient());
        var menuItem = new DailyMenuItemDto
        {
            Id = 10,
            FoodId = 1,
            FoodName = "قیمه",
            Price = 150_000,
            RemainingPortions = 10,
            IsAvailable = true
        };
        viewModel.MenuItems.Add(new SelectOption<DailyMenuItemDto>(menuItem, "قیمه"));
        viewModel.SelectedMenuItem = viewModel.MenuItems.Single();
        viewModel.QuantityToAdd = 2;

        viewModel.AddItemCommand.Execute(null);
        viewModel.QuantityToAdd = 3;
        viewModel.AddItemCommand.Execute(null);

        var line = Assert.Single(viewModel.Lines);
        Assert.Equal(2, line.Quantity);
        Assert.Contains("قبلاً", viewModel.ErrorMessage);
    }

    private static DailyMenuViewModel CreateDailyMenuViewModel(
        FakeDailyMenusApiClient? menusApi = null,
        FakeFoodsApiClient? foodsApi = null) =>
        new(menusApi ?? new FakeDailyMenusApiClient(), foodsApi ?? new FakeFoodsApiClient());

    private sealed class FakeFoodsApiClient : IFoodsApiClient
    {
        public int CreateCalls { get; private set; }
        public CreateFoodRequest? LastCreateRequest { get; private set; }
        public IReadOnlyList<FoodDto> Foods { get; init; } = [];
        public Task<IReadOnlyList<FoodDto>> GetFoodsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Foods);
        public Task<FoodDto?> GetFoodAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<FoodDto?>(null);
        public Task<FoodDto> CreateFoodAsync(CreateFoodRequest request, CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            LastCreateRequest = request;
            return Task.FromResult(new FoodDto { Id = 1, Name = request.Name });
        }
        public Task UpdateFoodAsync(int id, UpdateFoodRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetFoodActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeDailyMenusApiClient : IDailyMenusApiClient
    {
        private readonly TaskCompletionSource _saveStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _releaseSave = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int GetCalls { get; private set; }
        public int SaveCalls { get; private set; }
        public int SettingsCalls { get; private set; }
        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public int DeleteCalls { get; private set; }
        public bool HoldSave { get; set; }
        public Exception? SaveException { get; set; }
        public Exception? SettingsException { get; set; }
        public DailyMenuDto? MenuAfterSave { get; set; }
        public DailyMenuDto? MenuAfterUpdate { get; set; }
        public UpdateDailyMenuSettingsRequest? LastSettingsRequest { get; private set; }
        public UpdateDailyMenuItemRequest? LastUpdateRequest { get; private set; }
        public Task<DailyMenuDto?> GetMenuByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            GetCalls++;
            return Task.FromResult(MenuAfterSave);
        }

        public Task<DailyMenuDto> AddMenuItemAsync(DateOnly date, UpsertDailyMenuItemRequest request, CancellationToken cancellationToken = default)
        {
            AddCalls++;
            return Task.FromResult(MenuAfterSave ?? new DailyMenuDto
            {
                MenuDate = date,
                Items =
                [
                    new DailyMenuItemDto
                    {
                        Id = 10 + AddCalls,
                        FoodId = request.FoodId,
                        FoodName = request.FoodId == 1 ? "قیمه" : "Test",
                        Price = request.Price,
                        CapacityPortions = request.CapacityPortions,
                        IsAvailable = request.IsAvailable
                    }
                ]
            });
        }

        public async Task<DailyMenuDto> CreateOrUpdateMenuAsync(CreateOrUpdateDailyMenuRequest request, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            _saveStarted.TrySetResult();
            if (HoldSave)
            {
                await _releaseSave.Task.WaitAsync(cancellationToken);
            }

            if (SaveException is not null)
            {
                throw SaveException;
            }

            return new DailyMenuDto { MenuDate = request.MenuDate };
        }

        public async Task<DailyMenuDto> UpdateMenuSettingsAsync(DateOnly date, UpdateDailyMenuSettingsRequest request, CancellationToken cancellationToken = default)
        {
            SettingsCalls++;
            LastSettingsRequest = request;
            _saveStarted.TrySetResult();
            if (HoldSave)
            {
                await _releaseSave.Task.WaitAsync(cancellationToken);
            }

            if (SettingsException is not null)
            {
                throw SettingsException;
            }

            return MenuAfterSave ?? new DailyMenuDto
            {
                MenuDate = date,
                IsOpen = request.IsOpen,
                Note = request.Note
            };
        }

        public Task WaitForSaveStartedAsync() => _saveStarted.Task;
        public void ReleaseSave() => _releaseSave.TrySetResult();
        public Task<DailyMenuDto> UpdateMenuItemAsync(int dailyMenuItemId, UpdateDailyMenuItemRequest request, CancellationToken cancellationToken = default)
        {
            UpdateCalls++;
            LastUpdateRequest = request;
            return Task.FromResult(MenuAfterUpdate ?? new DailyMenuDto());
        }

        public Task<DailyMenuDto> DeleteMenuItemAsync(int dailyMenuItemId, CancellationToken cancellationToken = default)
        {
            DeleteCalls++;
            return Task.FromResult(MenuAfterSave ?? new DailyMenuDto());
        }

        public Task SetMenuItemAvailabilityAsync(int dailyMenuItemId, bool isAvailable, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeOrdersApiClient : IOrdersApiClient
    {
        public Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(
            DateOnly date,
            OrderStatus? status = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OrderSummaryDto>>([]);

        public Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<OrderDto?>(null);

        public Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new OrderDto { Id = 1, OrderNumber = "TEST-1" });

        public Task UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
