using BanooPaz.Domain.Enums;

namespace BanooPaz.Admin.Wpf.Models;

public sealed record OrderStatusFilterOption(string DisplayName, OrderStatus? Value);
