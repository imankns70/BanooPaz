using System.Net;
using System.Text;
using BanooPaz.Admin.Wpf.Services.Api;

namespace BanooPaz.Admin.UnitTests;

public sealed class AdminApiClientTests
{
    [Fact]
    public async Task GetFood_uses_admin_food_route()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new FoodsApiClient(CreateHttpClient(handler));

        var food = await client.GetFoodAsync(42);

        Assert.Null(food);
        Assert.Equal("https://localhost:5001/api/admin/foods/42", handler.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetDailyMenu_formats_date_in_route_and_returns_null_for_not_found()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new DailyMenusApiClient(CreateHttpClient(handler));

        var menu = await client.GetMenuByDateAsync(new DateOnly(2026, 7, 4));

        Assert.Null(menu);
        Assert.Equal(
            "https://localhost:5001/api/admin/daily-menus/by-date/2026-07-04",
            handler.RequestUri?.ToString());
    }

    [Fact]
    public async Task CreateFood_exposes_readable_api_error()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                "{\"error\":\"Food name is required.\"}",
                Encoding.UTF8,
                "application/json")
        });
        var client = new FoodsApiClient(CreateHttpClient(handler));

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.CreateFoodAsync(new() { Name = string.Empty }));

        Assert.Equal("Food name is required.", exception.Message);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://localhost:5001/") };

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
