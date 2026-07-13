using Kafgir.Application.Common;
using Kafgir.Contracts.Auth;

namespace Kafgir.Application.Interfaces;

public interface IAdminAuthService
{
    Task<AdminLoginResult> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default);
}
