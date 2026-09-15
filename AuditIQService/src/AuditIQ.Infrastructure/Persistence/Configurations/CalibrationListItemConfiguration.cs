using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class CalibrationListItemConfiguration : IEntityTypeConfiguration<CalibrationListItem>
{
    public void Configure(EntityTypeBuilder<CalibrationListItem> builder)
    {
        builder.ToTable("CalibrationListItems");
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.CalibrationList)
            .WithMany(l => l.Items)
            .HasForeignKey(i => i.CalibrationListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Evaluation)
            .WithMany()
            .HasForeignKey(i => i.EvaluationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Same evaluation can't be added to the same list twice.
        builder.HasIndex(i => new { i.CalibrationListId, i.EvaluationId }).IsUnique();
    }
}
