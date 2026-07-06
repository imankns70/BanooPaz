using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Customers;
using Microsoft.AspNetCore.Mvc;

namespace BanooPaz.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(
    ITelegramInitDataValidator telegramInitDataValidator,
    ICustomerProfileService customerProfileService) : ControllerBase
{
    [HttpPost("me")]
    public async Task<ActionResult<CustomerProfileDto>> GetMe(
        CustomerProfileLookupRequest request,
        CancellationToken cancellationToken)
    {
        long? telegramUserId;
        try
        {
            telegramUserId = ResolveTelegramUserId(request);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { error = exception.Message });
        }

        if (!telegramUserId.HasValue)
        {
            return NotFound();
        }

        var profile = await customerProfileService.GetByTelegramUserIdAsync(
            telegramUserId.Value,
            cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    private long? ResolveTelegramUserId(CustomerProfileLookupRequest request)
    {
        var validation = telegramInitDataValidator.Validate(request.TelegramInitData);
        if (validation.IsValid)
        {
            return validation.UserId;
        }

        if (validation.CanUseDevelopmentFallback)
        {
            return request.TelegramUserId;
        }

        throw new UnauthorizedAccessException(validation.Error ?? "Telegram initData is invalid.");
    }
}
