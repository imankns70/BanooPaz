using Kafgir.Application.Interfaces;
using Kafgir.Contracts.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Kafgir.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.CreateAsync(request, cancellationToken);
            return Created($"/api/admin/orders/{order.Id}", order);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { error = exception.Message });
        }
    }
}
