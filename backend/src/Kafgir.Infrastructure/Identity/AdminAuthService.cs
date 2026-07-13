using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kafgir.Application.Common;
using Kafgir.Application.Interfaces;
using Kafgir.Contracts.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kafgir.Infrastructure.Identity;

public sealed class AdminAuthService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> options) : IAdminAuthService
{
    public async Task<AdminLoginResult> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return new AdminLoginResult(AdminLoginStatus.InvalidCredentials);
        }

        var user = await userManager.FindByNameAsync(request.Username.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return new AdminLoginResult(AdminLoginStatus.InvalidCredentials);
        }

        var roles = await userManager.GetRolesAsync(user);
        if (!user.IsActive || !roles.Any(AppRoles.AdminRoles.Contains))
        {
            return new AdminLoginResult(AdminLoginStatus.Forbidden);
        }

        var jwtOptions = options.Value;
        if (jwtOptions.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must contain at least 32 characters.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiresMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AdminLoginResult(
            AdminLoginStatus.Success,
            new AdminLoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expiresAt,
                Username = user.UserName ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Roles = [.. roles]
            });
    }
}
