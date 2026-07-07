using BanooPaz.Contracts.Auth;

namespace BanooPaz.WPF.Services.Api;

public interface IAdminSession
{
    string? AccessToken { get; }
    DateTime? ExpiresAtUtc { get; }
    bool IsAuthenticated { get; }

    void Start(AdminLoginResponse response);
    void Clear();
}
