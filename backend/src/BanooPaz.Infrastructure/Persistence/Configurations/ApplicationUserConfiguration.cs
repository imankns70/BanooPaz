using BanooPaz.Domain.Entities;
using BanooPaz.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.TelegramFirstName).HasMaxLength(150);
        builder.Property(user => user.TelegramLastName).HasMaxLength(150);
        builder.Property(user => user.TelegramLanguageCode).HasMaxLength(20);
        builder.Property(user => user.FullName).HasMaxLength(150);
        builder.Property(user => user.PhoneNumber).HasMaxLength(30);
        builder.Property(user => user.IsActive).HasDefaultValue(true);
        builder.HasIndex(user => user.TelegramUserId)
            .IsUnique()
            .HasFilter("[TelegramUserId] IS NOT NULL");

        builder.HasOne(user => user.CustomerProfile)
            .WithOne()
            .HasForeignKey<CustomerProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
