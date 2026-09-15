using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class CauseCodeConfiguration : IEntityTypeConfiguration<CauseCode>
{
    public void Configure(EntityTypeBuilder<CauseCode> builder)
    {
        builder.ToTable("CauseCodes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Text).HasMaxLength(500).IsRequired();
    }
}
