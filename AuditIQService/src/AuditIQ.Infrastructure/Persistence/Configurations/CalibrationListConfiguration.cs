using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class CalibrationListConfiguration : IEntityTypeConfiguration<CalibrationList>
{
    public void Configure(EntityTypeBuilder<CalibrationList> builder)
    {
        builder.ToTable("CalibrationLists");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).HasMaxLength(256);
        builder.Property(l => l.VisibilityScope).HasMaxLength(256);

        builder.HasOne(l => l.CreatedBy)
            .WithMany()
            .HasForeignKey(l => l.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
