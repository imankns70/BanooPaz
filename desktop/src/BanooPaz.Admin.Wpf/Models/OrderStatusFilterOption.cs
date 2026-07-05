using BanooPaz.Contracts.Orders;

namespace BanooPaz.Admin.Wpf.Models;

public sealed record OrderStatusFilterOption(string DisplayName, OrderStatus? Value);
