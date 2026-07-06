using BanooPaz.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BanooPaz.Infrastructure.Persistence.Configurations;

public sealed class TelegramAccountConfiguration : IEntityTypeConfiguration<TelegramAccount>
{
    public void Configure(EntityTypeBuilder<TelegramAccount> builder)
    {
        builder.Property(account => account.Username).HasMaxLength(100);
        builder.Property(account => account.FirstName).HasMaxLength(150);
        builder.Property(account => account.LastName).HasMaxLength(150);
        builder.Property(account => account.LanguageCode).HasMaxLength(20);
        builder.Property(account => account.ChatId).HasMaxLength(120);
        builder.HasIndex(account => account.UserId).IsUnique();
        builder.HasIndex(account => account.TelegramUserId).IsUnique();
        builder.HasIndex(account => account.ChatId);

        builder.HasOne(account => account.User)
            .WithOne(user => user.TelegramAccount)
            .HasForeignKey<TelegramAccount>(account => account.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
