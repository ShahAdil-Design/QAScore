using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
{
    public void Configure(EntityTypeBuilder<SurveyResponse> builder)
    {
        builder.ToTable("SurveyResponses");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Comment).HasMaxLength(2000);

        builder.HasOne(r => r.Survey)
            .WithMany(s => s.Responses)
            .HasForeignKey(r => r.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Evaluation)
            .WithMany()
            .HasForeignKey(r => r.EvaluationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
