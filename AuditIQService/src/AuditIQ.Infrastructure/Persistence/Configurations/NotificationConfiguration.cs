using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Message).HasMaxLength(1024);

        // The feed query: this recipient's own notifications, newest first, optionally
        // unread-only — one index covers all three.
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead, n.CreatedAtUtc });
    }
}
