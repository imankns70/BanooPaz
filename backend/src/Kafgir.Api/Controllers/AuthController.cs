using Kafgir.Application.Common;
using Kafgir.Application.Interfaces;
using Kafgir.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Kafgir.Api.Controllers;

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
