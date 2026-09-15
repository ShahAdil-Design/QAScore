using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class ScorecardCategoryConfiguration : IEntityTypeConfiguration<ScorecardCategory>
{
    public void Configure(EntityTypeBuilder<ScorecardCategory> builder)
    {
        builder.ToTable("ScorecardCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
