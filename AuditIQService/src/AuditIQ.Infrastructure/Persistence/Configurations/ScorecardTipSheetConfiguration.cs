using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class ScorecardTipSheetConfiguration : IEntityTypeConfiguration<ScorecardTipSheet>
{
    public void Configure(EntityTypeBuilder<ScorecardTipSheet> builder)
    {
        builder.ToTable("ScorecardTipSheets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(256).IsRequired();
        builder.Property(t => t.Content).HasMaxLength(8000).IsRequired();

        builder.HasOne(t => t.Scorecard)
            .WithMany(s => s.TipSheets)
            .HasForeignKey(t => t.ScorecardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
