using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class UserTeamConfiguration : IEntityTypeConfiguration<UserTeam>
{
    public void Configure(EntityTypeBuilder<UserTeam> builder)
    {
        builder.ToTable("UserTeams");
        builder.HasKey(ut => new { ut.UserId, ut.TeamId });

        builder.HasOne(ut => ut.User)
            .WithMany(u => u.UserTeams)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ut => ut.Team)
            .WithMany(t => t.UserTeams)
            .HasForeignKey(ut => ut.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
