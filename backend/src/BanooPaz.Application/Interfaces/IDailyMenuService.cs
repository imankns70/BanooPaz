using BanooPaz.Contracts.Menus;

namespace BanooPaz.Application.Interfaces;

public interface IDailyMenuService
{
    Task<DailyMenuDto?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyMenuDto> CreateOrUpdateAsync(
        CreateOrUpdateDailyMenuRequest request,
        CancellationToken cancellationToken = default);
    Task<DailyMenuDto> UpdateSettingsAsync(
        DateOnly date,
        UpdateDailyMenuSettingsRequest request,
        CancellationToken cancellationToken = default);
    Task<DailyMenuDto> AddItemAsync(
        DateOnly date,
        UpsertDailyMenuItemRequest request,
        CancellationToken cancellationToken = default);
    Task<DailyMenuDto?> UpdateItemAsync(
        int dailyMenuItemId,
        UpdateDailyMenuItemRequest request,
        CancellationToken cancellationToken = default);
    Task<DailyMenuDto?> RemoveItemAsync(
        int dailyMenuItemId,
        CancellationToken cancellationToken = default);
    Task<bool> SetItemAvailabilityAsync(
        int dailyMenuItemId,
        bool isAvailable,
        CancellationToken cancellationToken = default);
}
