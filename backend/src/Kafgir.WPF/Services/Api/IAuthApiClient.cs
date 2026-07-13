using Kafgir.Contracts.Auth;

namespace Kafgir.WPF.Services.Api;

public interface IAuthApiClient
{
    Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default);
}
