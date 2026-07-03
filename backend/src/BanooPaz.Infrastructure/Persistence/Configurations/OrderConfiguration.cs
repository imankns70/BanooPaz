using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.Property(x => x.SubtotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DeliveryFee).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CustomerNote).HasMaxLength(1000);
        builder.Property(x => x.AdminNote).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.PaymentMethod).HasConversion<int>();
        builder.Property(x => x.DeliveryMethod).HasConversion<int>();

        builder.HasOne(x => x.CustomerAddress)
            .WithMany()
            .HasForeignKey(x => x.CustomerAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusHistories)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
