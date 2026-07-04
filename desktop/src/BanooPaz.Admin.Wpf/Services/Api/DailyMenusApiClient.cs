using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BanooPaz.Contracts.Menus;

namespace BanooPaz.Admin.Wpf.Services.Api;

public sealed class DailyMenusApiClient(HttpClient httpClient) : IDailyMenusApiClient
{
    public async Task<DailyMenuDto?> GetMenuByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/admin/daily-menus/by-date/{date:yyyy-MM-dd}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<DailyMenuDto>(cancellationToken);
    }

    public async Task<DailyMenuDto> CreateOrUpdateMenuAsync(
        CreateOrUpdateDailyMenuRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/admin/daily-menus", request, cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<DailyMenuDto>(cancellationToken)
            ?? throw new HttpRequestException("پاسخ ذخیره منوی روزانه خالی بود.");
    }

    public async Task SetMenuItemAvailabilityAsync(
        int dailyMenuItemId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PatchAsJsonAsync(
            $"api/admin/daily-menus/items/{dailyMenuItemId}/availability",
            new UpdateDailyMenuItemAvailabilityRequest { IsAvailable = isAvailable },
            cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
    }
}
