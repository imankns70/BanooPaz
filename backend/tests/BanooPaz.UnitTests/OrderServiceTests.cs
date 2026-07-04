using BanooPaz.Application.Interfaces;
using BanooPaz.Application.Services;
using BanooPaz.Contracts.Orders;
using BanooPaz.Domain.Entities;
using BanooPaz.Domain.Enums;

namespace BanooPaz.UnitTests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task Confirm_increases_sold_portions_and_adds_history()
    {
        var menuItem = CreateMenuItem(5, 1);
        var order = CreateOrder(menuItem, 2);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(order, unitOfWork);

        var updated = await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest
        {
            NewStatus = OrderStatus.Confirmed,
            StatusNote = "Confirmed by admin"
        });

        Assert.True(updated);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(3, menuItem.SoldPortions);
        Assert.NotNull(order.ConfirmedAt);
        Assert.Contains(order.StatusHistories, history =>
            history.FromStatus == OrderStatus.PendingConfirmation &&
            history.ToStatus == OrderStatus.Confirmed);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Confirm_rejects_insufficient_capacity_without_changes()
    {
        var menuItem = CreateMenuItem(2, 1);
        var order = CreateOrder(menuItem, 2);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(order, unitOfWork);

        var action = () => service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest
        {
            NewStatus = OrderStatus.Confirmed
        });

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal(OrderStatus.PendingConfirmation, order.Status);
        Assert.Equal(1, menuItem.SoldPortions);
        Assert.Null(order.ConfirmedAt);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancelling_confirmed_order_restores_capacity()
    {
        var menuItem = CreateMenuItem(5, 3);
        var order = CreateOrder(menuItem, 2, OrderStatus.Confirmed);
        var service = CreateService(order, new FakeUnitOfWork());

        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest
        {
            NewStatus = OrderStatus.Cancelled
        });

        Assert.Equal(1, menuItem.SoldPortions);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.CancelledAt);
    }

    private static OrderService CreateService(Order order, FakeUnitOfWork unitOfWork) =>
        new(new FakeOrderRepository(order), new EmptyDailyMenuRepository(),
            new EmptyCustomerRepository(), unitOfWork);

    private static DailyMenuItem CreateMenuItem(int capacity, int sold) => new()
    {
        Id = 7,
        CapacityPortions = capacity,
        SoldPortions = sold,
        Food = new Food { Name = "Test food" },
        DailyMenu = new DailyMenu { MenuDate = new DateOnly(2026, 7, 3), IsOpen = true }
    };

    private static Order CreateOrder(
        DailyMenuItem menuItem,
        int quantity,
        OrderStatus status = OrderStatus.PendingConfirmation)
    {
        var order = new Order
        {
            Id = 3,
            Customer = new Customer { Id = 2, FullName = "Test", PhoneNumber = "09160000000" },
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        order.Items.Add(new OrderItem
        {
            Order = order,
            DailyMenuItem = menuItem,
            DailyMenuItemId = menuItem.Id,
            FoodName = menuItem.Food.Name,
            Quantity = quantity
        });
        return order;
    }

    private sealed class FakeOrderRepository(Order order) : IOrderRepository
    {
        public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Order?>(id == order.Id ? order : null);
        public Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
            GetByIdAsync(id, cancellationToken);
        public Task<IReadOnlyList<Order>> GetByDateAsync(DateOnly date, OrderStatus? status = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Order>>([order]);
        public Task AddAsync(Order newOrder, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class EmptyDailyMenuRepository : IDailyMenuRepository
    {
        public Task<DailyMenu?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) => Task.FromResult<DailyMenu?>(null);
        public Task<DailyMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<DailyMenu?>(null);
        public Task<DailyMenuItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<DailyMenuItem?>(null);
        public Task AddAsync(DailyMenu menu, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class EmptyCustomerRepository : ICustomerRepository
    {
        public Task<Customer?> GetByTelegramUserIdAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult<Customer?>(null);
        public Task<Customer?> GetByPhoneNumberAsync(string phone, CancellationToken cancellationToken = default) => Task.FromResult<Customer?>(null);
        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
