using System.Windows;
using BanooPaz.Admin.Wpf.Services.Api;
using BanooPaz.Admin.Wpf.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BanooPaz.Admin.Wpf;

public partial class App : Application
{
    private IHost? _host;

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

                services.AddHttpClient<IOrdersApiClient, OrdersApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl));
                services.AddHttpClient<IFoodsApiClient, FoodsApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl));
                services.AddHttpClient<IDailyMenusApiClient, DailyMenusApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl));
                services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
                    client.BaseAddress = new Uri(baseUrl));
                services.AddSingleton<LoginViewModel>();
                services.AddSingleton<OrdersViewModel>();
                services.AddSingleton<FoodsViewModel>();
                services.AddSingleton<DailyMenuViewModel>();
                services.AddSingleton(provider =>
                {
                    var mainViewModel = new MainViewModel(
                        provider.GetRequiredService<LoginViewModel>(),
                        provider.GetRequiredService<OrdersViewModel>(),
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
}
