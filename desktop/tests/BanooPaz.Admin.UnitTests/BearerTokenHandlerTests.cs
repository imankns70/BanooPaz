using System.Net;
using System.Net.Http;
using BanooPaz.Admin.Wpf.Services.Api;
using BanooPaz.Contracts.Auth;

namespace BanooPaz.Admin.UnitTests;

public sealed class BearerTokenHandlerTests
{
    [Fact]
    public async Task SendAsync_attaches_bearer_token_when_session_is_authenticated()
    {
        var session = new AdminSession();
        session.Start(new AdminLoginResponse
        {
            AccessToken = "test-token",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        });
        var innerHandler = new StubHandler();
        using var invoker = CreateInvoker(session, innerHandler);

        await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/api/admin/orders"),
            CancellationToken.None);

        Assert.Equal("Bearer", innerHandler.AuthorizationScheme);
        Assert.Equal("test-token", innerHandler.AuthorizationParameter);
    }

    [Fact]
    public async Task SendAsync_does_not_attach_bearer_token_when_session_is_expired()
    {
        var session = new AdminSession();
        session.Start(new AdminLoginResponse
        {
            AccessToken = "expired-token",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1)
        });
        var innerHandler = new StubHandler();
        using var invoker = CreateInvoker(session, innerHandler);

        await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/api/admin/orders"),
            CancellationToken.None);

        Assert.Null(innerHandler.AuthorizationScheme);
        Assert.Null(innerHandler.AuthorizationParameter);
    }

    private static HttpMessageInvoker CreateInvoker(
        IAdminSession session,
        HttpMessageHandler innerHandler)
    {
        return new HttpMessageInvoker(new BearerTokenHandler(session)
        {
            InnerHandler = innerHandler
        });
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
