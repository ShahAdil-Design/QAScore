using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class EventSubTypeConfiguration : IEntityTypeConfiguration<EventSubType>
{
    public void Configure(EntityTypeBuilder<EventSubType> builder)
    {
        builder.ToTable("EventSubTypes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();

        builder.HasOne(e => e.EventType)
            .WithMany(t => t.SubTypes)
            .HasForeignKey(e => e.EventTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.EventTypeId, e.Name }).IsUnique();
    }
}
