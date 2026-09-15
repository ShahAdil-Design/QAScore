using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class CalibrationRatingConfiguration : IEntityTypeConfiguration<CalibrationRating>
{
    public void Configure(EntityTypeBuilder<CalibrationRating> builder)
    {
        builder.ToTable("CalibrationRatings");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Score).HasPrecision(5, 2);

        builder.HasOne(r => r.CalibrationListItem)
            .WithMany(i => i.Ratings)
            .HasForeignKey(r => r.CalibrationListItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Evaluator)
            .WithMany()
            .HasForeignKey(r => r.EvaluatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.CalibrationListItemId, r.EvaluatorId }).IsUnique();
    }
}
