using BanooPaz.Application.Interfaces;
using BanooPaz.Infrastructure.Persistence;
using BanooPaz.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BanooPaz.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<BanooPazDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IDailyMenuRepository, DailyMenuRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
