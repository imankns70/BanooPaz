using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.Property(message => message.Target).HasMaxLength(120);
        builder.Property(message => message.Text).HasMaxLength(2000);
        builder.Property(message => message.OrderNumber).HasMaxLength(32);
        builder.Property(message => message.LastError).HasMaxLength(1000);
        builder.HasIndex(message => new { message.Status, message.NextAttemptAt, message.CreatedAt });
        builder.HasIndex(message => message.OrderId);
        builder.HasOne(message => message.Order)
            .WithMany()
            .HasForeignKey(message => message.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
