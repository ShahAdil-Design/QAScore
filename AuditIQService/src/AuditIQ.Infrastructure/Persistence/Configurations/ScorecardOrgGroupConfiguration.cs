using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class ScorecardOrgGroupConfiguration : IEntityTypeConfiguration<ScorecardOrgGroup>
{
    public void Configure(EntityTypeBuilder<ScorecardOrgGroup> builder)
    {
        builder.ToTable("ScorecardOrgGroups");
        builder.HasKey(sg => new { sg.ScorecardId, sg.GroupId });

        builder.HasOne(sg => sg.Scorecard)
            .WithMany(s => s.OrgGroups)
            .HasForeignKey(sg => sg.ScorecardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sg => sg.Group)
            .WithMany()
            .HasForeignKey(sg => sg.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
