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

        public Task<DailyMenu?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
            Task.FromResult(_menu?.MenuDate == date ? _menu : null);

        public Task<DailyMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_menu?.Id == id ? _menu : null);

        public Task<DailyMenuItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_menu?.Items.SingleOrDefault(item => item.Id == id));

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
