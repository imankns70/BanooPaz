using Kafgir.Contracts.Auth;

namespace Kafgir.WPF.Services.Api;

public interface IAdminSession
{
    string? AccessToken { get; }
    DateTime? ExpiresAtUtc { get; }
    bool IsAuthenticated { get; }

    void Start(AdminLoginResponse response);
    void Clear();
}
