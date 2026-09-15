using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class EvaluationAttachmentConfiguration : IEntityTypeConfiguration<EvaluationAttachment>
{
    public void Configure(EntityTypeBuilder<EvaluationAttachment> builder)
    {
        builder.ToTable("EvaluationAttachments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).HasMaxLength(256).IsRequired();
        builder.Property(a => a.StoragePath).HasMaxLength(1024).IsRequired();

        builder.HasOne(a => a.Evaluation)
            .WithMany(e => e.Attachments)
            .HasForeignKey(a => a.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
