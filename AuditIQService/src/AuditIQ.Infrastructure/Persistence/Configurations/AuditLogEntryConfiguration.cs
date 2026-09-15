using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("AuditLogEntries");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).HasMaxLength(128);
        builder.Property(a => a.EntityId).HasMaxLength(128);
        builder.Property(a => a.Changes).HasColumnType("nvarchar(max)");

        // The two ways the audit page filters/sorts — by target entity, and by time.
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.TimestampUtc);
    }
}
