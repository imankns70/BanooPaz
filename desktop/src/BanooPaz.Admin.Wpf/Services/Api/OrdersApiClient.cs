using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BanooPaz.Contracts.Orders;
using BanooPaz.Domain.Enums;

namespace BanooPaz.Admin.Wpf.Services.Api;

public sealed class OrdersApiClient(HttpClient httpClient) : IOrdersApiClient
{
    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(
        DateOnly date,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var route = $"api/admin/orders?date={date:yyyy-MM-dd}";
        if (status.HasValue)
        {
            route += $"&status={(int)status.Value}";
        }

        using var response = await httpClient.GetAsync(route, cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<OrderSummaryDto>>(cancellationToken) ?? [];
    }

    public async Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/admin/orders/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PatchAsJsonAsync(
            $"api/admin/orders/{id}/status", request, cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
    }
}
