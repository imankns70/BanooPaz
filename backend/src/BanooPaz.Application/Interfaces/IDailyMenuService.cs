using BanooPaz.Contracts.Menus;

namespace BanooPaz.Application.Interfaces;

public interface IDailyMenuService
{
    Task<DailyMenuDto?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> CreateOrUpdateAsync(
        CreateOrUpdateDailyMenuRequest request,
        CancellationToken cancellationToken = default);
    Task<bool> SetItemAvailabilityAsync(
        int dailyMenuItemId,
        bool isAvailable,
        CancellationToken cancellationToken = default);
}
