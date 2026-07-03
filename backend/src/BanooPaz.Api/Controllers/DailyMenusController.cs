using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Menus;
using Microsoft.AspNetCore.Mvc;

namespace BanooPaz.Api.Controllers;

[ApiController]
[Route("api/admin/daily-menus")]
public sealed class DailyMenusController(IDailyMenuService dailyMenuService) : ControllerBase
{
    [HttpGet("by-date/{date}")]
    public async Task<ActionResult<DailyMenuDto>> GetByDate(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var menu = await dailyMenuService.GetByDateAsync(date, cancellationToken);
        return menu is null ? NotFound() : Ok(menu);
    }

    [HttpPost]
    public async Task<ActionResult<DailyMenuDto>> CreateOrUpdate(
        CreateOrUpdateDailyMenuRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await dailyMenuService.CreateOrUpdateAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPatch("items/{dailyMenuItemId:int}/availability")]
    public async Task<IActionResult> SetItemAvailability(
        int dailyMenuItemId,
        UpdateDailyMenuItemAvailabilityRequest request,
        CancellationToken cancellationToken) =>
        await dailyMenuService.SetItemAvailabilityAsync(
            dailyMenuItemId,
            request.IsAvailable,
            cancellationToken)
            ? NoContent()
            : NotFound();
}
