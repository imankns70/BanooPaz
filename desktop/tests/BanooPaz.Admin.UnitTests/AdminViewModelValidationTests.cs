using BanooPaz.Admin.Wpf.Models;
using BanooPaz.Admin.Wpf.Services.Api;
using BanooPaz.Admin.Wpf.ViewModels;
using BanooPaz.Contracts.Foods;
using BanooPaz.Contracts.Menus;

namespace BanooPaz.Admin.UnitTests;

public sealed class AdminViewModelValidationTests
{
    [Fact]
    public async Task Food_name_is_required_before_api_call()
    {
        var api = new FakeFoodsApiClient();
        var viewModel = new FoodsViewModel(api) { FoodName = "  ", DefaultPrice = 10 };

        await viewModel.SaveFoodCommand.ExecuteAsync(null);

        Assert.Contains("نام غذا", viewModel.ErrorMessage);
        Assert.Equal(0, api.CreateCalls);
    }

    [Fact]
    public async Task Food_price_cannot_be_negative()
    {
        var api = new FakeFoodsApiClient();
        var viewModel = new FoodsViewModel(api) { FoodName = "Test", DefaultPrice = -1 };

        await viewModel.SaveFoodCommand.ExecuteAsync(null);

        Assert.Contains("منفی", viewModel.ErrorMessage);
        Assert.Equal(0, api.CreateCalls);
    }

    [Fact]
    public void Daily_menu_prevents_duplicate_food_addition()
    {
        var viewModel = CreateDailyMenuViewModel();
        var food = new FoodDto { Id = 1, Name = "Test" };
        viewModel.AvailableFoods.Add(food);
        viewModel.SelectedFoodToAdd = food;

        viewModel.AddSelectedFoodCommand.Execute(null);
        viewModel.AddSelectedFoodCommand.Execute(null);

        Assert.Single(viewModel.Items);
        Assert.Contains("قبلاً", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Daily_menu_capacity_cannot_be_below_sold_portions()
    {
        var menusApi = new FakeDailyMenusApiClient();
        var viewModel = CreateDailyMenuViewModel(menusApi);
        viewModel.Items.Add(new DailyMenuItemEditModel
        {
            Id = 5, FoodId = 1, FoodName = "Test", SoldPortions = 3, CapacityPortions = 2
        });

        await viewModel.SaveMenuCommand.ExecuteAsync(null);

        Assert.Contains("فروخته", viewModel.ErrorMessage);
        Assert.Equal(0, menusApi.SaveCalls);
    }

    private static DailyMenuViewModel CreateDailyMenuViewModel(FakeDailyMenusApiClient? menusApi = null) =>
        new(menusApi ?? new FakeDailyMenusApiClient(), new FakeFoodsApiClient());

    private sealed class FakeFoodsApiClient : IFoodsApiClient
    {
        public int CreateCalls { get; private set; }
        public Task<IReadOnlyList<FoodDto>> GetFoodsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FoodDto>>([]);
        public Task<FoodDto?> GetFoodAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<FoodDto?>(null);
        public Task<FoodDto> CreateFoodAsync(CreateFoodRequest request, CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            return Task.FromResult(new FoodDto { Id = 1, Name = request.Name });
        }
        public Task UpdateFoodAsync(int id, UpdateFoodRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetFoodActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeDailyMenusApiClient : IDailyMenusApiClient
    {
        public int SaveCalls { get; private set; }
        public Task<DailyMenuDto?> GetMenuByDateAsync(DateOnly date, CancellationToken cancellationToken = default) => Task.FromResult<DailyMenuDto?>(null);
        public Task<DailyMenuDto> CreateOrUpdateMenuAsync(CreateOrUpdateDailyMenuRequest request, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(new DailyMenuDto { MenuDate = request.MenuDate });
        }
        public Task SetMenuItemAvailabilityAsync(int dailyMenuItemId, bool isAvailable, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
