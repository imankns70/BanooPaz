using BanooPaz.Contracts.Admin;

namespace BanooPaz.WPF.Services.Api;

public interface IAdminDashboardApiClient
{
    Task<AdminDashboardSummaryDto> GetTodayAsync(CancellationToken cancellationToken = default);
}
