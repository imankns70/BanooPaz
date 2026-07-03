using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.Role).HasConversion<int>();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.HasIndex(x => x.TelegramUserId);
    }
}
