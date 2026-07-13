using Kafgir.Contracts.Admin;

namespace Kafgir.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummaryDto> GetTodayAsync(CancellationToken cancellationToken = default);
}
