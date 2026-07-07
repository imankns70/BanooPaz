using BanooPaz.Application.Common;
using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanooPaz.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.AdminRoleList)]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController(IAdminDashboardService dashboardService) : ControllerBase
{
    [HttpGet("today")]
    public async Task<ActionResult<AdminDashboardSummaryDto>> GetToday(
        CancellationToken cancellationToken) =>
        Ok(await dashboardService.GetTodayAsync(cancellationToken));
}
