using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class ScorecardConfiguration : IEntityTypeConfiguration<Scorecard>
{
    public void Configure(EntityTypeBuilder<Scorecard> builder)
    {
        builder.ToTable("Scorecards", b => b.IsTemporal());

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.Property(s => s.ScorecardType).HasMaxLength(64).IsRequired();
        builder.Property(s => s.Location).HasMaxLength(256);
        builder.Property(s => s.TargetPercentage).HasPrecision(5, 2);
        builder.Property(s => s.MaxScore).HasPrecision(10, 2);

        builder.HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.CategoryId);
        builder.HasIndex(s => new { s.ScorecardGroupId, s.Version }).IsUnique();

        // Only one current version per scorecard group.
        builder.HasIndex(s => s.ScorecardGroupId)
            .IsUnique()
            .HasFilter("[IsCurrentVersion] = 1")
            .HasDatabaseName("IX_Scorecards_CurrentVersion");
    }
}
