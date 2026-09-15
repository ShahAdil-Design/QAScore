using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class QuestionAnswerOptionConfiguration : IEntityTypeConfiguration<QuestionAnswerOption>
{
    public void Configure(EntityTypeBuilder<QuestionAnswerOption> builder)
    {
        builder.ToTable("QuestionAnswerOptions");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Label).HasMaxLength(256).IsRequired();
        builder.Property(o => o.Value).HasPrecision(5, 2);

        builder.HasOne(o => o.ScorecardQuestion)
            .WithMany(q => q.AnswerOptions)
            .HasForeignKey(o => o.ScorecardQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.ScorecardQuestionId);
    }
}
