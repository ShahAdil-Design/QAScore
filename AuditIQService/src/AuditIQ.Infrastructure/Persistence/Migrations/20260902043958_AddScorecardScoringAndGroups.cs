using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScorecardScoringAndGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFailAll",
                table: "QuestionAnswerOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFailSection",
                table: "QuestionAnswerOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotApplicable",
                table: "QuestionAnswerOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "QuestionAnswerOptions",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ScorecardOrgGroups",
                columns: table => new
                {
                    ScorecardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScorecardOrgGroups", x => new { x.ScorecardId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_ScorecardOrgGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScorecardOrgGroups_Scorecards_ScorecardId",
                        column: x => x.ScorecardId,
                        principalTable: "Scorecards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardOrgGroups_GroupId",
                table: "ScorecardOrgGroups",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScorecardOrgGroups");

            migrationBuilder.DropColumn(
                name: "IsFailAll",
                table: "QuestionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "IsFailSection",
                table: "QuestionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "IsNotApplicable",
                table: "QuestionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "QuestionAnswerOptions");
        }
    }
}
