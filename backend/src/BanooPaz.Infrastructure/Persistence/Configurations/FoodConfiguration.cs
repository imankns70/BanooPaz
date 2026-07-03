using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class FoodConfiguration : IEntityTypeConfiguration<Food>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ImageUrl).HasMaxLength(2000);
        builder.Property(x => x.DefaultPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasData(
            new Food { Id = 1, Name = "زرشک‌پلو با مرغ", Description = "زرشک‌پلو خانگی با مرغ مزه‌دار شده", DefaultPrice = 0, IsActive = true, CreatedAt = SeedCreatedAt },
            new Food { Id = 2, Name = "قورمه‌سبزی", Description = "قورمه‌سبزی خانگی با سبزی تازه، لوبیا، لیموعمانی و گوشت", DefaultPrice = 0, IsActive = true, CreatedAt = SeedCreatedAt },
            new Food { Id = 3, Name = "ماکارونی", Description = "ماکارونی خانگی با مایه گوشتی و ته‌دیگ", DefaultPrice = 0, IsActive = true, CreatedAt = SeedCreatedAt },
            new Food { Id = 4, Name = "قیمه", Description = "خورشت قیمه خانگی با لپه، لیموعمانی و سیب‌زمینی", DefaultPrice = 0, IsActive = true, CreatedAt = SeedCreatedAt });
    }
}
