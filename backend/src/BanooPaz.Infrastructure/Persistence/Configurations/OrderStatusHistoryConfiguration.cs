using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.Property(x => x.FromStatus).HasConversion<int>();
        builder.Property(x => x.ToStatus).HasConversion<int>();
        builder.Property(x => x.Note).HasMaxLength(1000);
    }
}
