using Kafgir.Contracts.Menus;

namespace Kafgir.WPF.Services.Api;

public interface IDailyMenusApiClient
{
    Task<DailyMenuDto?> GetMenuByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> CreateOrUpdateMenuAsync(CreateOrUpdateDailyMenuRequest request, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> UpdateMenuSettingsAsync(DateOnly date, UpdateDailyMenuSettingsRequest request, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> AddMenuItemAsync(DateOnly date, UpsertDailyMenuItemRequest request, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> UpdateMenuItemAsync(int dailyMenuItemId, UpdateDailyMenuItemRequest request, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> DeleteMenuItemAsync(int dailyMenuItemId, CancellationToken cancellationToken = default);
    Task SetMenuItemAvailabilityAsync(int dailyMenuItemId, bool isAvailable, CancellationToken cancellationToken = default);
}
