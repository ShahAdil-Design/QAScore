using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(256).IsRequired();

        builder.HasOne(t => t.Group)
            .WithMany(g => g.Teams)
            .HasForeignKey(t => t.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.GroupId, t.Name }).IsUnique();
    }
}
