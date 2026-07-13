using System.Globalization;
using System.Net;
using System.Text;
using BanooPaz.WPF.Services.Api;
using BanooPaz.Contracts.Menus;

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
    public async Task GetDailyMenu_uses_gregorian_route_when_current_culture_is_persian()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            var persianCulture = (CultureInfo)CultureInfo.GetCultureInfo("fa-IR").Clone();
            persianCulture.DateTimeFormat.Calendar = new PersianCalendar();
            CultureInfo.CurrentCulture = persianCulture;
            CultureInfo.CurrentUICulture = persianCulture;

            var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
            var client = new DailyMenusApiClient(CreateHttpClient(handler));

            var menu = await client.GetMenuByDateAsync(new DateOnly(2026, 7, 4));

            Assert.Null(menu);
            Assert.Equal(
                "https://localhost:5001/api/admin/daily-menus/by-date/2026-07-04",
                handler.RequestUri?.ToString());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public async Task AddDailyMenuItem_uses_date_item_route()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"menuDate\":\"2026-07-04\",\"items\":[]}",
                Encoding.UTF8,
                "application/json")
        });
        var client = new DailyMenusApiClient(CreateHttpClient(handler));

        await client.AddMenuItemAsync(new DateOnly(2026, 7, 4), new UpsertDailyMenuItemRequest
        {
            FoodId = 1,
            Price = 150_000,
            CapacityPortions = 12
        });

        Assert.Equal(
            "https://localhost:5001/api/admin/daily-menus/by-date/2026-07-04/items",
            handler.RequestUri?.ToString());
    }

    [Fact]
    public async Task UpdateDailyMenuSettings_uses_date_settings_route()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"menuDate\":\"2026-07-04\",\"items\":[]}",
                Encoding.UTF8,
                "application/json")
        });
        var client = new DailyMenusApiClient(CreateHttpClient(handler));

        await client.UpdateMenuSettingsAsync(
            new DateOnly(2026, 7, 4),
            new UpdateDailyMenuSettingsRequest { IsOpen = false, Note = "closed" });

        Assert.Equal(HttpMethod.Patch, handler.Method);
        Assert.Equal(
            "https://localhost:5001/api/admin/daily-menus/by-date/2026-07-04",
            handler.RequestUri?.ToString());
    }

    [Fact]
    public async Task DeleteDailyMenuItem_uses_item_delete_route()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"menuDate\":\"2026-07-04\",\"items\":[]}",
                Encoding.UTF8,
                "application/json")
        });
        var client = new DailyMenusApiClient(CreateHttpClient(handler));

        await client.DeleteMenuItemAsync(42);

        Assert.Equal(HttpMethod.Delete, handler.Method);
        Assert.Equal(
            "https://localhost:5001/api/admin/daily-menus/items/42",
            handler.RequestUri?.ToString());
    }

    [Fact]
    public async Task UpdateDailyMenuItem_uses_item_patch_route()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"menuDate\":\"2026-07-04\",\"items\":[]}",
                Encoding.UTF8,
                "application/json")
        });
        var client = new DailyMenusApiClient(CreateHttpClient(handler));

        await client.UpdateMenuItemAsync(
            42,
            new UpdateDailyMenuItemRequest
            {
                Price = 150_000,
                CapacityPortions = 12,
                IsAvailable = true
            });

        Assert.Equal(HttpMethod.Patch, handler.Method);
        Assert.Equal(
            "https://localhost:5001/api/admin/daily-menus/items/42",
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

    [Fact]
    public async Task ApiHealth_uses_health_route()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new ApiHealthClient(CreateHttpClient(handler));

        var isAvailable = await client.IsApiAvailableAsync();

        Assert.True(isAvailable);
        Assert.Equal("https://localhost:5001/api/health", handler.RequestUri?.ToString());
    }

    [Fact]
    public async Task ApiHealth_returns_false_when_server_is_unreachable()
    {
        var client = new ApiHealthClient(CreateHttpClient(new ThrowingHandler()));

        var isAvailable = await client.IsApiAvailableAsync();

        Assert.False(isAvailable);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://localhost:5001/") };

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Method = request.Method;
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("Server is unreachable.");
    }
}
