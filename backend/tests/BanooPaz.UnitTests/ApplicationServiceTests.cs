using BanooPaz.Application.Interfaces;
using BanooPaz.Application.Services;
using BanooPaz.Contracts.Foods;
using BanooPaz.Contracts.Menus;
using BanooPaz.Domain.Entities;

namespace BanooPaz.UnitTests;

public sealed class ApplicationServiceTests
{
    [Fact]
    public async Task FoodService_rejects_negative_default_price()
    {
        var service = new FoodService(new FakeFoodRepository(), new FakeUnitOfWork());

        var action = () => service.CreateAsync(new CreateFoodRequest
        {
            Name = "Test food",
            DefaultPrice = -1
        });

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    [Fact]
    public async Task DailyMenuService_rejects_duplicate_foods()
    {
        var service = CreateDailyMenuService(new FakeDailyMenuRepository());

        var action = () => service.CreateOrUpdateAsync(new CreateOrUpdateDailyMenuRequest
        {
            MenuDate = new DateOnly(2026, 7, 3),
            Items =
            [
                new UpsertDailyMenuItemRequest { FoodId = 1, CapacityPortions = 5 },
                new UpsertDailyMenuItemRequest { FoodId = 1, CapacityPortions = 5 }
            ]
        });

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    [Fact]
    public async Task DailyMenuService_does_not_allow_capacity_below_sold_portions()
    {
        var food = new Food { Id = 1, Name = "Test food" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3),
            Items =
            [
                new DailyMenuItem
                {
                    Id = 1,
                    FoodId = 1,
                    Food = food,
                    CapacityPortions = 5,
                    SoldPortions = 3
                }
            ]
        };
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu), food);

        var action = () => service.CreateOrUpdateAsync(new CreateOrUpdateDailyMenuRequest
        {
            MenuDate = menu.MenuDate,
            Items =
            [
                new UpsertDailyMenuItemRequest { Id = 1, FoodId = 1, CapacityPortions = 2 }
            ]
        });

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal(3, menu.Items.Single().SoldPortions);
    }

    [Fact]
    public async Task DailyMenuService_removes_unsold_items_missing_from_update()
    {
        var keptFood = new Food { Id = 1, Name = "Kept" };
        var removedFood = new Food { Id = 2, Name = "Removed" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3),
            Items =
            [
                new DailyMenuItem { Id = 1, FoodId = 1, Food = keptFood, CapacityPortions = 5 },
                new DailyMenuItem { Id = 2, FoodId = 2, Food = removedFood, CapacityPortions = 5 }
            ]
        };
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu), keptFood, removedFood);

        await service.CreateOrUpdateAsync(new CreateOrUpdateDailyMenuRequest
        {
            MenuDate = menu.MenuDate,
            Items = [new UpsertDailyMenuItemRequest { Id = 1, FoodId = 1, CapacityPortions = 5 }]
        });

        Assert.Single(menu.Items);
        Assert.Equal(1, menu.Items.Single().FoodId);
    }

    [Fact]
    public async Task DailyMenuService_rejects_empty_update_for_existing_menu_with_items()
    {
        var food = new Food { Id = 1, Name = "Existing" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3),
            Items =
            [
                new DailyMenuItem { Id = 1, FoodId = 1, Food = food, CapacityPortions = 5 }
            ]
        };
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu), food);

        var action = () => service.CreateOrUpdateAsync(new CreateOrUpdateDailyMenuRequest
        {
            MenuDate = menu.MenuDate,
            Items = []
        });

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Single(menu.Items);
    }

    [Fact]
    public async Task DailyMenuService_update_settings_does_not_clear_items()
    {
        var food = new Food { Id = 1, Name = "Existing" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3),
            IsOpen = true,
            Items =
            [
                new DailyMenuItem { Id = 1, FoodId = 1, Food = food, CapacityPortions = 5 }
            ]
        };
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu), food);

        var result = await service.UpdateSettingsAsync(
            menu.MenuDate,
            new UpdateDailyMenuSettingsRequest
            {
                IsOpen = false,
                Note = "closed"
            });

        Assert.False(result.IsOpen);
        Assert.Equal("closed", result.Note);
        Assert.Single(result.Items);
        Assert.Single(menu.Items);
    }

    [Fact]
    public async Task DailyMenuService_add_item_creates_or_updates_menu_immediately()
    {
        var food = new Food { Id = 1, Name = "قیمه" };
        var repository = new FakeDailyMenuRepository();
        var service = CreateDailyMenuService(repository, food);

        var result = await service.AddItemAsync(
            new DateOnly(2026, 7, 3),
            new UpsertDailyMenuItemRequest
            {
                FoodId = food.Id,
                Price = 150_000,
                CapacityPortions = 12,
                IsAvailable = true
            });

        var item = Assert.Single(result.Items);
        Assert.Equal(food.Id, item.FoodId);
        Assert.Equal(150_000, item.Price);
        Assert.Equal(12, item.CapacityPortions);
    }

    [Fact]
    public async Task DailyMenuService_add_item_rejects_duplicate_food()
    {
        var food = new Food { Id = 1, Name = "قیمه" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3),
            Items =
            [
                new DailyMenuItem { Id = 1, FoodId = food.Id, Food = food, CapacityPortions = 5 }
            ]
        };
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu), food);

        var action = () => service.AddItemAsync(
            menu.MenuDate,
            new UpsertDailyMenuItemRequest
            {
                FoodId = food.Id,
                Price = 150_000,
                CapacityPortions = 12
            });

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Single(menu.Items);
    }

    [Fact]
    public async Task DailyMenuService_remove_item_removes_only_requested_unsold_item()
    {
        var keptFood = new Food { Id = 1, Name = "Kept" };
        var removedFood = new Food { Id = 2, Name = "Removed" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3)
        };
        menu.Items.Add(new DailyMenuItem
        {
            Id = 1,
            DailyMenu = menu,
            FoodId = 1,
            Food = keptFood,
            CapacityPortions = 5
        });
        menu.Items.Add(new DailyMenuItem
        {
            Id = 2,
            DailyMenu = menu,
            FoodId = 2,
            Food = removedFood,
            CapacityPortions = 5
        });
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu), keptFood, removedFood);

        var result = await service.RemoveItemAsync(2);

        Assert.NotNull(result);
        Assert.Single(menu.Items);
        Assert.Equal(1, menu.Items.Single().FoodId);
    }

    [Fact]
    public async Task DailyMenuService_remove_item_rejects_item_used_by_any_order()
    {
        var food = new Food { Id = 1, Name = "Booked" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3)
        };
        menu.Items.Add(new DailyMenuItem
        {
            Id = 1,
            DailyMenu = menu,
            FoodId = 1,
            Food = food,
            CapacityPortions = 5,
            SoldPortions = 0
        });
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu) { BookedItemIds = [1] }, food);

        var action = () => service.RemoveItemAsync(1);

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Single(menu.Items);
    }

    [Fact]
    public async Task DailyMenuService_update_item_updates_price_capacity_and_availability()
    {
        var food = new Food { Id = 1, Name = "Editable" };
        var menu = new DailyMenu
        {
            Id = 1,
            MenuDate = new DateOnly(2026, 7, 3)
        };
        menu.Items.Add(new DailyMenuItem
        {
            Id = 1,
            DailyMenu = menu,
            FoodId = 1,
            Food = food,
            Price = 100_000,
            CapacityPortions = 5,
            IsAvailable = true
        });
        var service = CreateDailyMenuService(new FakeDailyMenuRepository(menu), food);

        var result = await service.UpdateItemAsync(
            1,
            new UpdateDailyMenuItemRequest
            {
                Price = 120_000,
                CapacityPortions = 7,
                IsAvailable = false
            });

        Assert.NotNull(result);
        var item = Assert.Single(result.Items);
        Assert.Equal(120_000, item.Price);
        Assert.Equal(7, item.CapacityPortions);
        Assert.False(item.IsAvailable);
    }

    private static DailyMenuService CreateDailyMenuService(
        FakeDailyMenuRepository menuRepository,
        params Food[] foods) =>
        new(menuRepository, new FakeFoodRepository(foods), new FakeUnitOfWork());

    private sealed class FakeFoodRepository(params Food[] foods) : IFoodRepository
    {
        private readonly List<Food> _foods = [.. foods];

        public Task<IReadOnlyList<Food>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Food>>(_foods);

        public Task<IReadOnlyList<Food>> GetActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Food>>(_foods.Where(food => food.IsActive).ToList());

        public Task<Food?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_foods.SingleOrDefault(food => food.Id == id));

        public Task AddAsync(Food food, CancellationToken cancellationToken = default)
        {
            _foods.Add(food);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDailyMenuRepository(DailyMenu? menu = null) : IDailyMenuRepository
    {
        private DailyMenu? _menu = menu;
        public HashSet<int> BookedItemIds { get; init; } = [];

        public Task<DailyMenu?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
            Task.FromResult(_menu?.MenuDate == date ? _menu : null);

        public Task<DailyMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_menu?.Id == id ? _menu : null);

        public Task<DailyMenuItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_menu?.Items.SingleOrDefault(item => item.Id == id));

        public Task<bool> IsItemBookedAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(BookedItemIds.Contains(id));

        public Task AddAsync(DailyMenu dailyMenu, CancellationToken cancellationToken = default)
        {
            _menu = dailyMenu;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);
    }
}
