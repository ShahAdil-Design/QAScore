using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", b => b.IsTemporal());

        builder.HasKey(u => u.Id);
        builder.Property(u => u.SsoSubjectId).HasMaxLength(256).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.ExternalId).HasMaxLength(128);
        builder.Property(u => u.EmploymentType).HasMaxLength(64);
        builder.Property(u => u.Notes).HasMaxLength(2000);

        builder.HasIndex(u => u.SsoSubjectId).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.SupervisorId);

        // Self-referencing — Restrict (not Cascade) since SQL Server rejects cascading paths
        // that could revisit the same table, which a self-reference always risks.
        builder.HasOne(u => u.Supervisor)
            .WithMany(u => u.DirectReports)
            .HasForeignKey(u => u.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deliberately no global query filter on IsDeleted: Evaluation/CalibrationParticipant
        // require a non-null Agent/Evaluator, and this platform's compliance-grade retention
        // (Section 9) means historical evaluations must stay visible even if the referenced
        // user is later soft-deleted. Filter explicitly (Where(!u.IsDeleted)) in queries that
        // list *active* users, e.g. a staff picker — never rely on an implicit global filter here.
    }
}
