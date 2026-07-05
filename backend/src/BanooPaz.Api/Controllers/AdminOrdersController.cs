using BanooPaz.Application.Common;
using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanooPaz.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.AdminRoleList)]
[Route("api/admin/orders")]
public sealed class AdminOrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetByDate(
        [FromQuery] DateOnly? date,
        [FromQuery] OrderStatus? status,
        CancellationToken cancellationToken)
    {
        if (!date.HasValue)
        {
            return BadRequest(new { error = "The date query parameter is required (yyyy-MM-dd)." });
        }

        try
        {
            return Ok(await orderService.GetByDateAsync(
                date.Value,
                status,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await orderService.UpdateStatusAsync(id, request, cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}
