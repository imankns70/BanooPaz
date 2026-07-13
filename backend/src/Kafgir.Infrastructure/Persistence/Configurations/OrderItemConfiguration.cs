using Kafgir.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kafgir.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(x => x.FoodName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Quantity).IsRequired();

        builder.HasOne(x => x.DailyMenuItem)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.DailyMenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
