using Kafgir.Application.Common;
using Kafgir.Application.Interfaces;
using Kafgir.Contracts.Foods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kafgir.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.AdminRoleList)]
[Route("api/admin/foods")]
public sealed class FoodsController(IFoodService foodService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FoodDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await foodService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FoodDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var food = await foodService.GetByIdAsync(id, cancellationToken);
        return food is null ? NotFound() : Ok(food);
    }

    [HttpPost]
    public async Task<ActionResult<FoodDto>> Create(
        CreateFoodRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var food = await foodService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = food.Id }, food);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateFoodRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await foodService.UpdateAsync(id, request, cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(
        int id,
        SetFoodActiveRequest request,
        CancellationToken cancellationToken) =>
        await foodService.SetActiveAsync(id, request.IsActive, cancellationToken)
            ? NoContent()
            : NotFound();
}
