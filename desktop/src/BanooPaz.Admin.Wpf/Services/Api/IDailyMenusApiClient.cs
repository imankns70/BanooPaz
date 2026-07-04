using BanooPaz.Contracts.Menus;

namespace BanooPaz.Admin.Wpf.Services.Api;

public interface IDailyMenusApiClient
{
    Task<DailyMenuDto?> GetMenuByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> CreateOrUpdateMenuAsync(CreateOrUpdateDailyMenuRequest request, CancellationToken cancellationToken = default);
    Task SetMenuItemAvailabilityAsync(int dailyMenuItemId, bool isAvailable, CancellationToken cancellationToken = default);
}
