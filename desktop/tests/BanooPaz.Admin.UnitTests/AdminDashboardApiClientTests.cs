using System.Net;
using System.Text;
using BanooPaz.WPF.Services.Api;

namespace BanooPaz.Admin.UnitTests;

public sealed class AdminDashboardApiClientTests
{
    [Fact]
    public async Task GetToday_uses_admin_dashboard_route()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"date\":\"2026-07-07\",\"totalOrders\":3,\"isTodayMenuOpen\":true}",
                Encoding.UTF8,
                "application/json")
        });
        var client = new AdminDashboardApiClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://localhost:5001/") });

        var summary = await client.GetTodayAsync();

        Assert.Equal("https://localhost:5001/api/admin/dashboard/today", handler.RequestUri?.ToString());
        Assert.Equal(3, summary.TotalOrders);
        Assert.True(summary.IsTodayMenuOpen);
    }

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(response);
        }
    }
}
