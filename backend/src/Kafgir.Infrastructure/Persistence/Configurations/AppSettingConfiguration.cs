using Kafgir.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kafgir.Infrastructure.Persistence.Configurations;

public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.Property(x => x.Key).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
