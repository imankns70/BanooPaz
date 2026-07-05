using BanooPaz.Application.Common;
using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BanooPaz.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAdminAuthService authService) : ControllerBase
{
    [HttpPost("admin/login")]
    public async Task<ActionResult<AdminLoginResponse>> AdminLogin(
        AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.Status switch
        {
            AdminLoginStatus.Success => Ok(result.Response),
            AdminLoginStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden),
            _ => Unauthorized()
        };
    }
}
