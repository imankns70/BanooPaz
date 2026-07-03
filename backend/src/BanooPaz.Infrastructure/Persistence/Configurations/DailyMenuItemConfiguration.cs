using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class DailyMenuItemConfiguration : IEntityTypeConfiguration<DailyMenuItem>
{
    public void Configure(EntityTypeBuilder<DailyMenuItem> builder)
    {
        builder.Ignore(x => x.RemainingPortions);
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CapacityPortions).IsRequired();
        builder.Property(x => x.SoldPortions).HasDefaultValue(0);
        builder.Property(x => x.IsAvailable).HasDefaultValue(true);
        builder.HasIndex(x => new { x.DailyMenuId, x.FoodId }).IsUnique();

        builder.HasOne(x => x.Food)
            .WithMany(x => x.DailyMenuItems)
            .HasForeignKey(x => x.FoodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
