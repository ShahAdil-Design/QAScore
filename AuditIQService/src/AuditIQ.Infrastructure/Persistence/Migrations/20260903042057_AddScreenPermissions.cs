using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScreenPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScreenPermissions",
                columns: table => new
                {
                    Role = table.Column<int>(type: "int", nullable: false),
                    ScreenKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenPermissions", x => new { x.Role, x.ScreenKey });
                });

            // Seed reproduces today's hardcoded Sidebar.tsx behavior exactly (adminOnly screens
            // visible only to Admin=0; the rest visible to every role) — shipping this migration
            // changes nothing visible until someone edits the matrix in the new admin panel.
            // Roles: Admin=0, Supervisor=1, TeamLead=2, QaEvaluator=3, Agent=4.
            var roles = new[] { 0, 1, 2, 3, 4 };
            var adminOnlyScreens = new[] { "score", "calibration", "reports", "scorecards", "staff" };
            var openScreens = new[] { "dashboard", "review" };

            foreach (var screen in adminOnlyScreens)
                foreach (var role in roles)
                    migrationBuilder.InsertData("ScreenPermissions", ["Role", "ScreenKey", "IsVisible"], [role, screen, role == 0]);

            foreach (var screen in openScreens)
                foreach (var role in roles)
                    migrationBuilder.InsertData("ScreenPermissions", ["Role", "ScreenKey", "IsVisible"], [role, screen, true]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScreenPermissions");
        }
    }
}
