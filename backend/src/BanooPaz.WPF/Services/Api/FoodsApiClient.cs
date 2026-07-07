using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BanooPaz.Contracts.Foods;

namespace BanooPaz.WPF.Services.Api;

public sealed class FoodsApiClient(HttpClient httpClient) : IFoodsApiClient
{
    public async Task<IReadOnlyList<FoodDto>> GetFoodsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/admin/foods", cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<FoodDto>>(cancellationToken) ?? [];
    }

    public async Task<FoodDto?> GetFoodAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/admin/foods/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<FoodDto>(cancellationToken);
    }

    public async Task<FoodDto> CreateFoodAsync(
        CreateFoodRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/admin/foods", request, cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<FoodDto>(cancellationToken)
            ?? throw new HttpRequestException("پاسخ ایجاد غذا خالی بود.");
    }

    public async Task UpdateFoodAsync(
        int id,
        UpdateFoodRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"api/admin/foods/{id}", request, cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetFoodActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PatchAsJsonAsync(
            $"api/admin/foods/{id}/active",
            new SetFoodActiveRequest { IsActive = isActive },
            cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
    }
}
