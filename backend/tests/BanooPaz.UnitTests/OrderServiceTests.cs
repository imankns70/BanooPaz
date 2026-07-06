using BanooPaz.Application.Interfaces;
using BanooPaz.Application.Services;
using BanooPaz.Contracts.Orders;
using BanooPaz.Domain.Entities;
using DomainOrderStatus = BanooPaz.Domain.Enums.OrderStatus;

namespace BanooPaz.UnitTests;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task Confirm_increases_sold_portions_and_adds_history()
    {
        var menuItem = CreateMenuItem(5, 1);
        var order = CreateOrder(menuItem, 2);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateStatusService(order, unitOfWork);

        var updated = await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest
        {
            NewStatus = OrderStatus.Confirmed,
            StatusNote = "Confirmed by admin"
        });

        Assert.True(updated);
        Assert.Equal(DomainOrderStatus.Confirmed, order.Status);
        Assert.Equal(3, menuItem.SoldPortions);
        Assert.NotNull(order.ConfirmedAt);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Confirm_rejects_insufficient_capacity_without_changes()
    {
        var menuItem = CreateMenuItem(2, 1);
        var order = CreateOrder(menuItem, 2);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateStatusService(order, unitOfWork);

        var action = () => service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest
        {
            NewStatus = OrderStatus.Confirmed
        });

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal(DomainOrderStatus.PendingConfirmation, order.Status);
        Assert.Equal(1, menuItem.SoldPortions);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancelling_confirmed_order_restores_capacity()
    {
        var menuItem = CreateMenuItem(5, 3);
        var order = CreateOrder(menuItem, 2, DomainOrderStatus.Confirmed);
        var service = CreateStatusService(order, new FakeUnitOfWork());

        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest
        {
            NewStatus = OrderStatus.Cancelled
        });

        Assert.Equal(1, menuItem.SoldPortions);
        Assert.Equal(DomainOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task Create_saves_first_new_address_and_snapshots_delivery_data()
    {
        var profile = new CustomerProfile { Id = 12 };
        var orderRepository = new FakeOrderRepository();
        var service = CreateOrderService(profile, orderRepository);

        var result = await service.CreateAsync(CreateRequest());

        var address = Assert.Single(profile.Addresses);
        Assert.True(address.IsDefault);
        Assert.Equal("خانه", address.Title);
        Assert.Equal("خیابان یک", result.AddressLine);
        Assert.Equal("خیابان یک", orderRepository.Added!.DeliveryAddressLine);
    }

    [Fact]
    public async Task Create_uses_existing_address_and_keeps_independent_snapshot()
    {
        var address = new CustomerAddress
        {
            Id = 4,
            Title = "خانه",
            City = "اندیمشک",
            AddressLine = "آدرس ذخیره‌شده",
            IsActive = true
        };
        var profile = new CustomerProfile { Id = 12, Addresses = [address] };
        address.CustomerProfile = profile;
        var orderRepository = new FakeOrderRepository();
        var service = CreateOrderService(profile, orderRepository);
        var request = CreateRequest();
        request.CustomerAddressId = address.Id;
        request.AddressLine = null;

        await service.CreateAsync(request);
        address.AddressLine = "آدرس ویرایش‌شده";

        Assert.NotNull(address.LastUsedAt);
        Assert.Equal("آدرس ذخیره‌شده", orderRepository.Added!.DeliveryAddressLine);
    }

    private static CreateOrderRequest CreateRequest() => new()
    {
        TelegramUserId = 123,
        FullName = "Test Customer",
        PhoneNumber = "09160000000",
        NewAddressTitle = "خانه",
        City = "اندیمشک",
        AddressLine = "خیابان یک",
        SaveAddress = true,
        DeliveryMethod = DeliveryMethod.Delivery,
        Items = [new CreateOrderItemRequest { DailyMenuItemId = 7, Quantity = 1 }]
    };

    private static OrderService CreateOrderService(
        CustomerProfile profile,
        FakeOrderRepository repository) =>
        new(repository, new FakeDailyMenuRepository(CreateMenuItem(5, 0)),
            new FakeCustomerIdentityService(profile), new FakeTelegramInitDataValidator(),
            new FakeNotificationQueue(), new FakeUnitOfWork());

    private static OrderService CreateStatusService(Order order, FakeUnitOfWork unitOfWork) =>
        new(new FakeOrderRepository(order), new FakeDailyMenuRepository(),
            new FakeCustomerIdentityService(order.CustomerProfile), new FakeTelegramInitDataValidator(),
            new FakeNotificationQueue(), unitOfWork);

    private static DailyMenuItem CreateMenuItem(int capacity, int sold) => new()
    {
        Id = 7,
        Price = 100,
        CapacityPortions = capacity,
        SoldPortions = sold,
        IsAvailable = true,
        Food = new Food { Name = "Test food" },
        DailyMenu = new DailyMenu { MenuDate = new DateOnly(2026, 7, 4), IsOpen = true }
    };

    private static Order CreateOrder(
        DailyMenuItem menuItem,
        int quantity,
        DomainOrderStatus status = DomainOrderStatus.PendingConfirmation)
    {
        var profile = new CustomerProfile { Id = 2, PreferredName = "Test", DefaultPhoneNumber = "09160000000" };
        var order = new Order
        {
            Id = 3,
            CustomerProfile = profile,
            CustomerProfileId = profile.Id,
            DeliveryFullName = profile.PreferredName,
            DeliveryPhoneNumber = profile.DefaultPhoneNumber,
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

    private sealed class FakeOrderRepository(Order? order = null) : IOrderRepository
    {
        public Order? Added { get; private set; }
        public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(order);
        public Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(order);
        public Task<IReadOnlyList<Order>> GetByDateAsync(DateOnly date, DomainOrderStatus? status = null, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Order>>(order is null ? [] : [order]);
        public Task AddAsync(Order newOrder, CancellationToken cancellationToken = default)
        {
            Added = newOrder;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDailyMenuRepository(DailyMenuItem? item = null) : IDailyMenuRepository
    {
        public Task<DailyMenu?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) => Task.FromResult<DailyMenu?>(null);
        public Task<DailyMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<DailyMenu?>(null);
        public Task<DailyMenuItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(item);
        public Task AddAsync(DailyMenu menu, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCustomerIdentityService(CustomerProfile profile) : ICustomerIdentityService
    {
        public Task<CustomerProfile> ResolveAsync(long? telegramUserId, string? telegramUsername, string? telegramFirstName, string? telegramLastName, string fullName, string phoneNumber, DateTime now, CancellationToken cancellationToken = default)
        {
            profile.PreferredName = fullName;
            profile.DefaultPhoneNumber = phoneNumber;
            profile.LastOrderAt = now;
            return Task.FromResult(profile);
        }
    }

    private sealed class FakeTelegramInitDataValidator : ITelegramInitDataValidator
    {
        public BanooPaz.Application.Common.TelegramInitDataValidationResult Validate(string? initData) =>
            BanooPaz.Application.Common.TelegramInitDataValidationResult.MissingOptional();
    }

    private sealed class FakeNotificationQueue : INotificationQueue
    {
        public Task EnqueueAdminOrderSubmittedAsync(Order order, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task EnqueueCustomerOrderStatusChangedAsync(
            Order order,
            DomainOrderStatus newStatus,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
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
