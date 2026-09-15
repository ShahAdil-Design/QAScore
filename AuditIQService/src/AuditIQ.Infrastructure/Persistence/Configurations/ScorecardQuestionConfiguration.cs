using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class ScorecardQuestionConfiguration : IEntityTypeConfiguration<ScorecardQuestion>
{
    public void Configure(EntityTypeBuilder<ScorecardQuestion> builder)
    {
        builder.ToTable("ScorecardQuestions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.SectionName).HasMaxLength(256).IsRequired();
        builder.Property(q => q.Text).HasMaxLength(1000).IsRequired();

        builder.HasOne(q => q.Scorecard)
            .WithMany(s => s.Questions)
            .HasForeignKey(q => q.ScorecardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.ScorecardId);
    }
}
