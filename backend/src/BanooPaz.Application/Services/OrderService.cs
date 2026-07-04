using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Orders;
using BanooPaz.Domain.Entities;
using BanooPaz.Domain.Enums;

namespace BanooPaz.Application.Services;

public sealed class OrderService(
    IOrderRepository orderRepository,
    IDailyMenuRepository dailyMenuRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IOrderService
{
    private const string DefaultCity = "اندیمشک";

    public async Task<OrderDto> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var customer = await FindCustomerAsync(request, cancellationToken);
        var now = DateTime.UtcNow;
        if (customer is null)
        {
            customer = new Customer
            {
                TelegramUserId = request.TelegramUserId,
                TelegramUsername = NormalizeOptional(request.TelegramUsername),
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                CreatedAt = now,
                LastOrderAt = now
            };
            await customerRepository.AddAsync(customer, cancellationToken);
        }
        else
        {
            customer.TelegramUsername = NormalizeOptional(request.TelegramUsername);
            customer.FullName = request.FullName.Trim();
            customer.PhoneNumber = request.PhoneNumber.Trim();
            customer.LastOrderAt = now;
        }

        var address = new CustomerAddress
        {
            Customer = customer,
            City = string.IsNullOrWhiteSpace(request.City) ? DefaultCity : request.City.Trim(),
            AddressLine = request.AddressLine.Trim(),
            Description = NormalizeOptional(request.AddressDescription),
            IsDefault = customer.Addresses.Count == 0,
            CreatedAt = now
        };
        customer.Addresses.Add(address);

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(now),
            Customer = customer,
            CustomerAddress = address,
            Status = OrderStatus.PendingConfirmation,
            PaymentMethod = request.PaymentMethod,
            DeliveryMethod = request.DeliveryMethod,
            DeliveryFee = 0,
            CustomerNote = NormalizeOptional(request.CustomerNote),
            CreatedAt = now
        };

        foreach (var requestedItem in request.Items)
        {
            var menuItem = await dailyMenuRepository.GetItemByIdAsync(
                requestedItem.DailyMenuItemId,
                cancellationToken)
                ?? throw new ArgumentException(
                    $"Daily menu item with id {requestedItem.DailyMenuItemId} was not found.");

            if (!menuItem.IsAvailable)
            {
                throw new ArgumentException($"{menuItem.Food.Name} is not available.");
            }

            if (!menuItem.DailyMenu.IsOpen)
            {
                throw new ArgumentException(
                    $"The daily menu for {menuItem.DailyMenu.MenuDate:yyyy-MM-dd} is closed.");
            }

            order.Items.Add(new OrderItem
            {
                Order = order,
                DailyMenuItemId = menuItem.Id,
                DailyMenuItem = menuItem,
                FoodName = menuItem.Food.Name,
                UnitPrice = menuItem.Price,
                Quantity = requestedItem.Quantity,
                TotalPrice = menuItem.Price * requestedItem.Quantity
            });
        }

        order.SubtotalAmount = order.Items.Sum(item => item.TotalPrice);
        order.TotalAmount = order.SubtotalAmount + order.DeliveryFee;
        order.StatusHistories.Add(new OrderStatusHistory
        {
            Order = order,
            FromStatus = OrderStatus.PendingConfirmation,
            ToStatus = OrderStatus.PendingConfirmation,
            Note = "Order created",
            ChangedAt = now
        });

        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetByDateAsync(
        DateOnly date,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        if (date == default)
        {
            throw new ArgumentException("Date is required.");
        }

        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ArgumentException("Order status is invalid.");
        }

        var orders = await orderRepository.GetByDateAsync(date, status, cancellationToken);
        return orders.Select(MapSummary).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (order is null)
        {
            return false;
        }

        if (!IsAllowedTransition(order.Status, request.NewStatus))
        {
            throw new ArgumentException(
                $"Order status cannot change from {order.Status} to {request.NewStatus}.");
        }

        var previousStatus = order.Status;
        var now = DateTime.UtcNow;

        if (request.NewStatus == OrderStatus.Confirmed)
        {
            Confirm(order, now);
        }
        else if (request.NewStatus == OrderStatus.Cancelled)
        {
            Cancel(order, now);
        }
        else if (request.NewStatus == OrderStatus.Delivered)
        {
            order.DeliveredAt = now;
        }

        order.Status = request.NewStatus;
        if (request.AdminNote is not null)
        {
            order.AdminNote = NormalizeOptional(request.AdminNote);
        }

        order.StatusHistories.Add(new OrderStatusHistory
        {
            Order = order,
            FromStatus = previousStatus,
            ToStatus = request.NewStatus,
            Note = NormalizeOptional(request.StatusNote),
            ChangedAt = now
        });

        // TODO: Add explicit transaction support when payment/order workflows become more complex.
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Customer?> FindCustomerAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TelegramUserId.HasValue)
        {
            return await customerRepository.GetByTelegramUserIdAsync(
                request.TelegramUserId.Value,
                cancellationToken);
        }

        return await customerRepository.GetByPhoneNumberAsync(
            request.PhoneNumber.Trim(),
            cancellationToken);
    }

    private static void Confirm(Order order, DateTime now)
    {
        foreach (var item in order.Items)
        {
            if (item.DailyMenuItem.RemainingPortions < item.Quantity)
            {
                throw new ArgumentException(
                    $"Not enough remaining portions for {item.FoodName}. " +
                    $"Requested: {item.Quantity}, remaining: {item.DailyMenuItem.RemainingPortions}.");
            }
        }

        foreach (var item in order.Items)
        {
            item.DailyMenuItem.SoldPortions += item.Quantity;
        }

        order.ConfirmedAt = now;
    }

    private static void Cancel(Order order, DateTime now)
    {
        if (order.Status is OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.Ready)
        {
            foreach (var item in order.Items)
            {
                item.DailyMenuItem.SoldPortions = Math.Max(
                    0,
                    item.DailyMenuItem.SoldPortions - item.Quantity);
            }
        }

        order.CancelledAt = now;
    }

    private static bool IsAllowedTransition(OrderStatus current, OrderStatus next) =>
        (current, next) switch
        {
            (OrderStatus.PendingConfirmation, OrderStatus.Confirmed) => true,
            (OrderStatus.PendingConfirmation, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Preparing, OrderStatus.Ready) => true,
            (OrderStatus.Preparing, OrderStatus.Cancelled) => true,
            (OrderStatus.Ready, OrderStatus.Delivered) => true,
            (OrderStatus.Ready, OrderStatus.Cancelled) => true,
            _ => false
        };

    private static void ValidateCreateRequest(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            throw new ArgumentException("Phone number is required.");
        }

        if (!Enum.IsDefined(request.PaymentMethod))
        {
            throw new ArgumentException("Payment method is invalid.");
        }

        if (!Enum.IsDefined(request.DeliveryMethod))
        {
            throw new ArgumentException("Delivery method is invalid.");
        }

        if (request.DeliveryMethod == DeliveryMethod.Delivery &&
            string.IsNullOrWhiteSpace(request.AddressLine))
        {
            throw new ArgumentException("Address line is required for delivery orders.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("At least one order item is required.");
        }

        var duplicateItemId = request.Items
            .GroupBy(item => item.DailyMenuItemId)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (duplicateItemId.HasValue)
        {
            throw new ArgumentException(
                $"Daily menu item id {duplicateItemId.Value} appears more than once.");
        }

        foreach (var item in request.Items)
        {
            if (item.DailyMenuItemId <= 0)
            {
                throw new ArgumentException("Daily menu item id must be greater than zero.");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException("Order item quantity must be greater than zero.");
            }
        }
    }

    private static string GenerateOrderNumber(DateTime createdAt) =>
        $"BP-{createdAt:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..31];

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static OrderSummaryDto MapSummary(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CustomerFullName = order.Customer.FullName,
        CustomerPhoneNumber = order.Customer.PhoneNumber,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        DeliveryMethod = order.DeliveryMethod,
        CreatedAt = order.CreatedAt,
        TotalQuantity = order.Items.Sum(item => item.Quantity)
    };

    private static OrderDto Map(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CustomerId = order.CustomerId,
        CustomerFullName = order.Customer.FullName,
        CustomerPhoneNumber = order.Customer.PhoneNumber,
        AddressLine = order.CustomerAddress?.AddressLine,
        AddressDescription = order.CustomerAddress?.Description,
        Status = order.Status,
        PaymentMethod = order.PaymentMethod,
        DeliveryMethod = order.DeliveryMethod,
        SubtotalAmount = order.SubtotalAmount,
        DeliveryFee = order.DeliveryFee,
        TotalAmount = order.TotalAmount,
        CustomerNote = order.CustomerNote,
        AdminNote = order.AdminNote,
        CreatedAt = order.CreatedAt,
        ConfirmedAt = order.ConfirmedAt,
        DeliveredAt = order.DeliveredAt,
        CancelledAt = order.CancelledAt,
        Items = order.Items
            .OrderBy(item => item.Id)
            .Select(item => new OrderItemDto
            {
                Id = item.Id,
                DailyMenuItemId = item.DailyMenuItemId,
                FoodName = item.FoodName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            })
            .ToList(),
        StatusHistories = order.StatusHistories
            .OrderBy(history => history.ChangedAt)
            .Select(history => new OrderStatusHistoryDto
            {
                FromStatus = history.FromStatus,
                ToStatus = history.ToStatus,
                Note = history.Note,
                ChangedAt = history.ChangedAt
            })
            .ToList()
    };
}
