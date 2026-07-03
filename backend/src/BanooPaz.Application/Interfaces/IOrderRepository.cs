using BanooPaz.Domain.Entities;

namespace BanooPaz.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetTodayAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
