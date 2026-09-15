using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Break-glass bootstrap for the very first AuditIQ Admin on a brand-new environment.
    /// AuditIqRoleClaimsTransformation resolves role/access purely by matching the caller's SSO
    /// email against the Users table — there is no auto-provisioning by design (Section 13), so a
    /// freshly-provisioned database has nobody who can open the Staff page to create anyone else.
    /// SsoSubjectId is never checked at login (only Email is), so any unique placeholder works —
    /// same convention CreateUserCommandHandler uses for admin-created users.
    ///
    /// Guarded by "only if Users is completely empty": safe to leave in the migration history
    /// permanently, since it becomes a permanent no-op the moment any environment has its first
    /// real user (including the very row it inserts). It is NOT environment-specific — it runs
    /// wherever MigrationRunner applies pending migrations against an empty Users table.
    ///
    /// IMPORTANT: edit AdminEmail/AdminDisplayName below to the real first Admin's SSO email
    /// BEFORE this migration is applied to dev01 (or any target environment). Left as the
    /// placeholder, it creates an inert row nobody can sign in as.
    /// </summary>
    public partial class BootstrapFirstAdmin : Migration
    {
        private const string AdminEmail = "adil.shah@oakbrookfinance.com";
        private const string AdminDisplayName = "Adil Shah";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM Users)
                BEGIN
                    INSERT INTO Users (Id, SsoSubjectId, DisplayName, Email, Role, IsActive, IsDeleted)
                    VALUES (
                        NEWID(),
                        CONCAT('bootstrap:', CONVERT(varchar(36), NEWID())),
                        N'{AdminDisplayName}',
                        N'{AdminEmail}',
                        0, -- UserRole.Admin
                        1, -- IsActive
                        0  -- IsDeleted
                    )
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM Users WHERE Email = N'{AdminEmail}'");
        }
    }
}
