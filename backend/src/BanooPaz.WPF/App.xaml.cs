using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using BanooPaz.WPF.Services.Api;
using BanooPaz.WPF.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BanooPaz.WPF;

public partial class App : Application
{
    private IHost? _host;

    public App()
    {
        ConfigurePersianCulture();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configuration =>
            {
                configuration.SetBasePath(AppContext.BaseDirectory);
                configuration.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureServices((context, services) =>
            {
                var baseUrl = context.Configuration["Api:BaseUrl"]
                    ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

                services.AddSingleton<IAdminSession, AdminSession>();
                services.AddTransient<BearerTokenHandler>();
                services.AddHttpClient<IOrdersApiClient, OrdersApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl))
                    .AddHttpMessageHandler<BearerTokenHandler>();
                services.AddHttpClient<IAdminDashboardApiClient, AdminDashboardApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl))
                    .AddHttpMessageHandler<BearerTokenHandler>();
                services.AddHttpClient<IFoodsApiClient, FoodsApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl))
                    .AddHttpMessageHandler<BearerTokenHandler>();
                services.AddHttpClient<IDailyMenusApiClient, DailyMenusApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl))
                    .AddHttpMessageHandler<BearerTokenHandler>();
                services.AddHttpClient<IApiHealthClient, ApiHealthClient>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(3);
                });
                services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl));
                services.AddSingleton<LoginViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<OrdersViewModel>();
                services.AddSingleton<ManualOrderViewModel>();
                services.AddSingleton<FoodsViewModel>();
                services.AddSingleton<DailyMenuViewModel>();
                services.AddSingleton(provider =>
                {
                    var mainViewModel = new MainViewModel(
                        provider.GetRequiredService<LoginViewModel>(),
                        provider.GetRequiredService<DashboardViewModel>(),
                        provider.GetRequiredService<OrdersViewModel>(),
                        provider.GetRequiredService<ManualOrderViewModel>(),
                        provider.GetRequiredService<FoodsViewModel>(),
                        provider.GetRequiredService<DailyMenuViewModel>());
                    mainViewModel.Login.LoginSucceeded += (_, _) => mainViewModel.MarkAuthenticated();
                    return mainViewModel;
                });
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();
        _host.Services.GetRequiredService<MainWindow>().Show();
        _host.Services.GetRequiredService<LoginViewModel>().StartConnectionCheck();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            _host.Services.GetService<OrdersViewModel>()?.Dispose();
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static void ConfigurePersianCulture()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("fa-IR").Clone();
        culture.DateTimeFormat.Calendar = new PersianCalendar();
        culture.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
        culture.DateTimeFormat.LongDatePattern = "dddd d MMMM yyyy";
        culture.DateTimeFormat.ShortTimePattern = "HH:mm";
        culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
    }
}
