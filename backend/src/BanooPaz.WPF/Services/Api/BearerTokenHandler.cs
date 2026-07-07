using System.Net.Http;
using System.Net.Http.Headers;

namespace BanooPaz.WPF.Services.Api;

public sealed class BearerTokenHandler(IAdminSession adminSession) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (adminSession.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                adminSession.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
