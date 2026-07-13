using Kafgir.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kafgir.Infrastructure.Persistence.Configurations;

public sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.Property(profile => profile.PreferredName).IsRequired().HasMaxLength(150);
        builder.Property(profile => profile.DefaultPhoneNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(profile => profile.UserId).IsUnique();

        builder.HasMany(profile => profile.Addresses)
            .WithOne(address => address.CustomerProfile)
            .HasForeignKey(address => address.CustomerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(profile => profile.Orders)
            .WithOne(order => order.CustomerProfile)
            .HasForeignKey(order => order.CustomerProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
