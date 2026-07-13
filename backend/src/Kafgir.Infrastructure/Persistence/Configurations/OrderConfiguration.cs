using Kafgir.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kafgir.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(order => order.OrderNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.Property(order => order.DeliveryFullName).IsRequired().HasMaxLength(150);
        builder.Property(order => order.DeliveryPhoneNumber).IsRequired().HasMaxLength(30);
        builder.Property(order => order.DeliveryCity).IsRequired().HasMaxLength(100);
        builder.Property(order => order.DeliveryAddressLine).IsRequired().HasMaxLength(1000);
        builder.Property(order => order.DeliveryAddressDescription).HasMaxLength(1000);
        builder.Property(order => order.SubtotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(order => order.DeliveryFee).HasColumnType("decimal(18,2)");
        builder.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(order => order.CustomerNote).HasMaxLength(1000);
        builder.Property(order => order.AdminNote).HasMaxLength(1000);
        builder.Property(order => order.Status).HasConversion<int>();
        builder.Property(order => order.PaymentMethod).HasConversion<int>();
        builder.Property(order => order.DeliveryMethod).HasConversion<int>();

        builder.HasOne(order => order.CustomerAddress)
            .WithMany()
            .HasForeignKey(order => order.CustomerAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(order => order.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(order => order.StatusHistories)
            .WithOne(history => history.Order)
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
