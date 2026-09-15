using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class EvaluationAnswerConfiguration : IEntityTypeConfiguration<EvaluationAnswer>
{
    public void Configure(EntityTypeBuilder<EvaluationAnswer> builder)
    {
        builder.ToTable("EvaluationAnswers");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AnswerValue).HasMaxLength(256);
        builder.Property(a => a.CauseCode).HasMaxLength(256);
        builder.Property(a => a.Comment).HasMaxLength(2000);
        builder.Property(a => a.Score).HasPrecision(5, 2);

        builder.HasOne(a => a.Evaluation)
            .WithMany(e => e.Answers)
            .HasForeignKey(a => a.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.ScorecardQuestion)
            .WithMany()
            .HasForeignKey(a => a.ScorecardQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EvaluationId, a.ScorecardQuestionId }).IsUnique();
    }
}
