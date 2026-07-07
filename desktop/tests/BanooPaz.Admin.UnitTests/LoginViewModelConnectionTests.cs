using BanooPaz.Contracts.Auth;
using BanooPaz.WPF.Services.Api;
using BanooPaz.WPF.ViewModels;

namespace BanooPaz.Admin.UnitTests;

public sealed class LoginViewModelConnectionTests
{
    [Fact]
    public async Task Login_still_calls_auth_when_health_check_is_unavailable()
    {
        var auth = new FakeAuthApiClient();
        var viewModel = new LoginViewModel(
            auth,
            new FakeApiHealthClient(false),
            new AdminSession())
        {
            Username = "admin",
            Password = "Admin@123456"
        };

        await viewModel.LoginCommand.ExecuteAsync(null);

        Assert.Equal(1, auth.LoginCalls);
        Assert.True(viewModel.IsApiAvailable);
        Assert.Null(viewModel.ConnectionMessage);
    }

    [Fact]
    public async Task Login_calls_auth_when_api_is_available()
    {
        var auth = new FakeAuthApiClient();
        var viewModel = new LoginViewModel(
            auth,
            new FakeApiHealthClient(true),
            new AdminSession())
        {
            Username = "admin",
            Password = "Admin@123456"
        };

        await viewModel.LoginCommand.ExecuteAsync(null);

        Assert.Equal(1, auth.LoginCalls);
        Assert.True(viewModel.IsApiAvailable);
        Assert.Null(viewModel.ConnectionMessage);
    }

    [Fact]
    public async Task Login_shows_connection_message_when_auth_request_cannot_reach_api()
    {
        var viewModel = new LoginViewModel(
            new FakeAuthApiClient(new HttpRequestException("API is down.")),
            new FakeApiHealthClient(true),
            new AdminSession())
        {
            Username = "admin",
            Password = "Admin@123456"
        };

        await viewModel.LoginCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsApiAvailable);
        Assert.Contains("ارتباط با سرور", viewModel.ConnectionMessage);
    }

    private sealed class FakeApiHealthClient(bool isAvailable) : IApiHealthClient
    {
        public Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(isAvailable);
    }

    private sealed class FakeAuthApiClient(Exception? exception = null) : IAuthApiClient
    {
        public int LoginCalls { get; private set; }

        public Task<AdminLoginResponse> LoginAsync(
            AdminLoginRequest request,
            CancellationToken cancellationToken = default)
        {
            LoginCalls++;
            if (exception is not null)
            {
                throw exception;
            }

            return Task.FromResult(new AdminLoginResponse
            {
                AccessToken = "test-token",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            });
        }
    }
}
