using BanooPaz.Application.Common;
using BanooPaz.Contracts.Auth;

namespace BanooPaz.Application.Interfaces;

public interface IAdminAuthService
{
    Task<AdminLoginResult> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default);
}
