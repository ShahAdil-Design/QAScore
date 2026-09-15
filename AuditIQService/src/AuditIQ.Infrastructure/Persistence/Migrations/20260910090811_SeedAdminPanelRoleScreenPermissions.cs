using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminPanelRoleScreenPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // GroupAdmin=5, TeamAdmin=6, ReportsAnalyst=7, CalibrationAnalyst=8 (UserRole.cs) —
            // Scorebuddy admin-panel roles landing in AuditIQ dormant: every screen defaults to
            // invisible, same as every other role would if newly added, so shipping this changes
            // nothing visible until someone deliberately edits the matrix in the Permissions page.
            var newRoles = new[] { 5, 6, 7, 8 };
            var allScreens = new[] { "dashboard", "score", "review", "calibration", "reports", "scorecards", "staff" };

            foreach (var role in newRoles)
                foreach (var screen in allScreens)
                    migrationBuilder.InsertData("ScreenPermissions", ["Role", "ScreenKey", "IsVisible"], [role, screen, false]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var newRoles = new[] { 5, 6, 7, 8 };
            var allScreens = new[] { "dashboard", "score", "review", "calibration", "reports", "scorecards", "staff" };

            foreach (var role in newRoles)
                foreach (var screen in allScreens)
                    migrationBuilder.DeleteData("ScreenPermissions", ["Role", "ScreenKey"], [role, screen]);
        }
    }
}
