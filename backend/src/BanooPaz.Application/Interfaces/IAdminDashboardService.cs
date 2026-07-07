using BanooPaz.Contracts.Admin;

namespace BanooPaz.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummaryDto> GetTodayAsync(CancellationToken cancellationToken = default);
}
