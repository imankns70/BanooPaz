using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.AddressLine).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Description).HasMaxLength(1000);
    }
}
