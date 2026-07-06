using BanooPaz.Application.Interfaces;
using BanooPaz.Infrastructure.Persistence;
using BanooPaz.Infrastructure.Persistence.Repositories;
using BanooPaz.Infrastructure.Identity;
using BanooPaz.Infrastructure.Notifications;
using Microsoft.AspNetCore.Identity;
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
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<BanooPazDbContext>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
        services.Configure<TelegramNotificationOptions>(
            configuration.GetSection(TelegramNotificationOptions.SectionName));
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IDailyMenuRepository, DailyMenuRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerIdentityService, CustomerIdentityService>();
        services.AddScoped<ICustomerProfileService, CustomerProfileService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<INotificationQueue, NotificationQueue>();
        services.AddScoped<INotificationProcessor, NotificationProcessor>();
        services.AddSingleton<ITelegramInitDataValidator, TelegramInitDataValidator>();
        services.AddHttpClient<ITelegramMessageSender, TelegramMessageSender>(client =>
        {
            client.BaseAddress = new Uri("https://api.telegram.org/");
        });

        return services;
    }
}
