using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(BanooPazDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.CustomerAddress)
            .Include(order => order.Items)
            .Include(order => order.StatusHistories)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetTodayAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        return await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Items)
            .Where(order => order.CreatedAt >= start && order.CreatedAt < end)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await dbContext.Orders.AddAsync(order, cancellationToken);
}
