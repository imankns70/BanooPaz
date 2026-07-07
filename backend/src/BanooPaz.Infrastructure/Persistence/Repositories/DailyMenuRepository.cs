using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Persistence.Repositories;

public sealed class DailyMenuRepository(BanooPazDbContext dbContext) : IDailyMenuRepository
{
    public Task<DailyMenu?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        dbContext.DailyMenus
            .Include(menu => menu.Items)
            .ThenInclude(item => item.Food)
            .SingleOrDefaultAsync(menu => menu.MenuDate == date, cancellationToken);

    public Task<DailyMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.DailyMenus
            .Include(menu => menu.Items)
            .ThenInclude(item => item.Food)
            .SingleOrDefaultAsync(menu => menu.Id == id, cancellationToken);

    public Task<DailyMenuItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.DailyMenuItems
            .Include(item => item.DailyMenu)
            .Include(item => item.Food)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<bool> IsItemBookedAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.OrderItems.AnyAsync(item => item.DailyMenuItemId == id, cancellationToken);

    public async Task AddAsync(DailyMenu dailyMenu, CancellationToken cancellationToken = default) =>
        await dbContext.DailyMenus.AddAsync(dailyMenu, cancellationToken);
}
