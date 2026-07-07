using System.Net.Http;
using System.Net.Http.Json;
using BanooPaz.Contracts.Admin;

namespace BanooPaz.WPF.Services.Api;

public sealed class AdminDashboardApiClient(HttpClient httpClient) : IAdminDashboardApiClient
{
    public async Task<AdminDashboardSummaryDto> GetTodayAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            "api/admin/dashboard/today",
            cancellationToken);
        await ApiResponseHandler.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AdminDashboardSummaryDto>(cancellationToken)
            ?? new AdminDashboardSummaryDto();
    }
}
