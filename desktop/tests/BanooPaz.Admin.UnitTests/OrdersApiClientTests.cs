using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using BanooPaz.WPF.Services.Api;
using BanooPaz.Contracts.Orders;

namespace BanooPaz.Admin.UnitTests;

public sealed class OrdersApiClientTests
{
    [Fact]
    public async Task GetOrders_uses_date_and_status_query_parameters()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });
        var client = CreateClient(handler);

        await client.GetOrdersAsync(new DateOnly(2026, 7, 3), OrderStatus.Confirmed);

        Assert.Equal(
            "https://localhost:5001/api/admin/orders?date=2026-07-03&status=2",
            handler.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetOrders_uses_gregorian_date_when_current_culture_is_persian()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            var persianCulture = (CultureInfo)CultureInfo.GetCultureInfo("fa-IR").Clone();
            persianCulture.DateTimeFormat.Calendar = new PersianCalendar();
            CultureInfo.CurrentCulture = persianCulture;
            CultureInfo.CurrentUICulture = persianCulture;

            var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });
            var client = CreateClient(handler);

            await client.GetOrdersAsync(new DateOnly(2026, 7, 3), OrderStatus.Confirmed);

            Assert.Equal(
                "https://localhost:5001/api/admin/orders?date=2026-07-03&status=2",
                handler.RequestUri?.ToString());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public async Task GetOrder_returns_null_for_not_found()
    {
        var client = CreateClient(new StubHandler(new HttpResponseMessage(HttpStatusCode.NotFound)));

        var order = await client.GetOrderAsync(42);

        Assert.Null(order);
    }

    [Fact]
    public async Task UpdateStatus_exposes_readable_api_error()
    {
        var client = CreateClient(new StubHandler(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                "{\"error\":\"Not enough remaining portions.\"}",
                Encoding.UTF8,
                "application/json")
        }));

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.UpdateStatusAsync(7, new() { NewStatus = OrderStatus.Confirmed }));

        Assert.Equal("Not enough remaining portions.", exception.Message);
    }

    private static OrdersApiClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://localhost:5001/") });

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
