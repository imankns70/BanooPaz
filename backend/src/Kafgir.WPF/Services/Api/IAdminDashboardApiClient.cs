using Kafgir.Contracts.Admin;

namespace Kafgir.WPF.Services.Api;

public interface IAdminDashboardApiClient
{
    Task<AdminDashboardSummaryDto> GetTodayAsync(CancellationToken cancellationToken = default);
}
