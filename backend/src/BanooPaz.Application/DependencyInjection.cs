using BanooPaz.Application.Interfaces;
using BanooPaz.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BanooPaz.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFoodService, FoodService>();
        services.AddScoped<IDailyMenuService, DailyMenuService>();
        return services;
    }
}
