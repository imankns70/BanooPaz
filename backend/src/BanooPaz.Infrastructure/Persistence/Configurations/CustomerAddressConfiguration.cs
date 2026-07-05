using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.Property(address => address.Title).IsRequired().HasMaxLength(100);
        builder.Property(address => address.City).IsRequired().HasMaxLength(100);
        builder.Property(address => address.AddressLine).IsRequired().HasMaxLength(1000);
        builder.Property(address => address.Description).HasMaxLength(1000);
        builder.Property(address => address.IsActive).HasDefaultValue(true);
        builder.HasIndex(address => address.CustomerProfileId);
    }
}
