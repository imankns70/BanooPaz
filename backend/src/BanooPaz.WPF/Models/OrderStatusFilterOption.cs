using BanooPaz.Contracts.Orders;

namespace BanooPaz.WPF.Models;

public sealed record OrderStatusFilterOption(string DisplayName, OrderStatus? Value);
