using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
{
    public void Configure(EntityTypeBuilder<EventType> builder)
    {
        builder.ToTable("EventTypes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();

        // Unique per scorecard, not globally — different scorecards legitimately have their own
        // same-named event type (e.g. "Manage Collections"), each a distinct Scorebuddy record.
        builder.HasIndex(e => new { e.ScorecardId, e.Name }).IsUnique();

        builder.HasOne(e => e.Scorecard)
            .WithMany()
            .HasForeignKey(e => e.ScorecardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
