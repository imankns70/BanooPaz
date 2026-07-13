using Kafgir.Contracts.Auth;

namespace Kafgir.WPF.Services.Api;

public sealed class AdminSession : IAdminSession
{
    public string? AccessToken { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }

    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(AccessToken)
        && ExpiresAtUtc.HasValue
        && ExpiresAtUtc.Value > DateTime.UtcNow;

    public void Start(AdminLoginResponse response)
    {
        AccessToken = response.AccessToken;
        ExpiresAtUtc = response.ExpiresAtUtc;
    }

    public void Clear()
    {
        AccessToken = null;
        ExpiresAtUtc = null;
    }
}
