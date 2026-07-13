using Kafgir.Application.Interfaces;
using Kafgir.Contracts.Menus;
using Microsoft.AspNetCore.Mvc;

namespace Kafgir.Api.Controllers;

[ApiController]
[Route("api/menus")]
public sealed class MenusController(IDailyMenuService dailyMenuService) : ControllerBase
{
    [HttpGet("today")]
    public async Task<ActionResult<DailyMenuDto>> GetToday(CancellationToken cancellationToken)
    {
        var menu = await dailyMenuService.GetByDateAsync(
            DateOnly.FromDateTime(DateTime.Today),
            cancellationToken);

        return menu is null ? NotFound() : Ok(menu);
    }
}
