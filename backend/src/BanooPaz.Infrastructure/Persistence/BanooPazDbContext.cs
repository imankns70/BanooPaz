using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Persistence;

public sealed class BanooPazDbContext(DbContextOptions<BanooPazDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<DailyMenu> DailyMenus => Set<DailyMenu>();
    public DbSet<DailyMenuItem> DailyMenuItems => Set<DailyMenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BanooPazDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
