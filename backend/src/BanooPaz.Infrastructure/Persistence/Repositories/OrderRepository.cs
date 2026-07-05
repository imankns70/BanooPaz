using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using BanooPaz.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(BanooPazDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Orders
            .AsNoTracking()
            .Include(order => order.CustomerProfile)
            .Include(order => order.CustomerAddress)
            .Include(order => order.Items)
            .Include(order => order.StatusHistories)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<Order?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.Orders
            .Include(order => order.CustomerProfile)
            .Include(order => order.CustomerAddress)
            .Include(order => order.Items)
            .ThenInclude(item => item.DailyMenuItem)
            .Include(order => order.StatusHistories)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByDateAsync(
        DateOnly date,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var query = dbContext.Orders
            .AsNoTracking()
            .Include(order => order.CustomerProfile)
            .Include(order => order.Items)
            .Where(order => order.CreatedAt >= start && order.CreatedAt < end);

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        return await query
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await dbContext.Orders.AddAsync(order, cancellationToken);
}
