using BanooPaz.Contracts.Auth;

namespace BanooPaz.Admin.Wpf.Services.Api;

public interface IAuthApiClient
{
    Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default);
}
