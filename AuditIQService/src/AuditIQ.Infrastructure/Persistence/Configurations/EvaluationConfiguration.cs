using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("Evaluations", b => b.IsTemporal());

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Reference).HasMaxLength(256);
        builder.Property(e => e.EvaluatorNotes).HasMaxLength(4000);
        builder.Property(e => e.DisputeReason).HasMaxLength(2000);
        builder.Property(e => e.ResolutionNotes).HasMaxLength(2000);
        builder.Property(e => e.TotalScore).HasPrecision(5, 2);
        builder.Property(e => e.LegacyTotalScore).HasPrecision(5, 2);
        builder.Ignore(e => e.DisplayScore);

        builder.HasOne(e => e.Scorecard)
            .WithMany()
            .HasForeignKey(e => e.ScorecardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Agent)
            .WithMany()
            .HasForeignKey(e => e.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Evaluator)
            .WithMany()
            .HasForeignKey(e => e.EvaluatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EventType)
            .WithMany()
            .HasForeignKey(e => e.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EventSubType)
            .WithMany()
            .HasForeignKey(e => e.EventSubTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AgentId);
        builder.HasIndex(e => e.EvaluatorId);
        builder.HasIndex(e => e.ScorecardId);
        builder.HasIndex(e => e.EventOccurredAt);
        builder.HasIndex(e => e.Status);
    }
}
