using Kafgir.Domain.Entities;

namespace Kafgir.Application.Interfaces;

public interface IDailyMenuRepository
{
    Task<DailyMenu?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DailyMenuItem?> GetItemByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> IsItemBookedAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(DailyMenu dailyMenu, CancellationToken cancellationToken = default);
}
