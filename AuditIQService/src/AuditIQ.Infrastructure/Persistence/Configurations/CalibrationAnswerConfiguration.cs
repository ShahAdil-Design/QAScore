using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class CalibrationAnswerConfiguration : IEntityTypeConfiguration<CalibrationAnswer>
{
    public void Configure(EntityTypeBuilder<CalibrationAnswer> builder)
    {
        builder.ToTable("CalibrationAnswers");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AnswerValue).HasMaxLength(256);
        builder.Property(a => a.CauseCode).HasMaxLength(256);
        builder.Property(a => a.Comment).HasMaxLength(2000);
        builder.Property(a => a.Score).HasPrecision(5, 2);

        builder.HasOne(a => a.CalibrationRating)
            .WithMany(r => r.Answers)
            .HasForeignKey(a => a.CalibrationRatingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.ScorecardQuestion)
            .WithMany()
            .HasForeignKey(a => a.ScorecardQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.CalibrationRatingId, a.ScorecardQuestionId }).IsUnique();
    }
}
