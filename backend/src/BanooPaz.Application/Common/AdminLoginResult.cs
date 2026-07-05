using BanooPaz.Contracts.Auth;

namespace BanooPaz.Application.Common;

public enum AdminLoginStatus
{
    Success,
    InvalidCredentials,
    Forbidden
}

public sealed record AdminLoginResult(AdminLoginStatus Status, AdminLoginResponse? Response = null);
