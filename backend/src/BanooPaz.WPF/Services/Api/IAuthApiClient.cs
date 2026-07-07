using BanooPaz.Contracts.Auth;

namespace BanooPaz.WPF.Services.Api;

public interface IAuthApiClient
{
    Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default);
}
