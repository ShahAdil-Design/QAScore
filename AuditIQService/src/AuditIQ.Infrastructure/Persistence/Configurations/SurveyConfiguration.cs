using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.ToTable("Surveys");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Type).HasMaxLength(64).IsRequired();
    }
}
