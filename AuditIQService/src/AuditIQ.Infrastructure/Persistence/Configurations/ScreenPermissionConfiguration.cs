using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class ScreenPermissionConfiguration : IEntityTypeConfiguration<ScreenPermission>
{
    public void Configure(EntityTypeBuilder<ScreenPermission> builder)
    {
        builder.ToTable("ScreenPermissions");
        builder.HasKey(p => new { p.Role, p.ScreenKey });
        builder.Property(p => p.ScreenKey).HasMaxLength(64);
    }
}
