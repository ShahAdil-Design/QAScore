using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScopeEventTypesToScorecard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Every existing EventType/EventSubType row predates scorecard-scoping and has no
            // real ScorecardId to backfill — they were a flat, ungrounded global list (see
            // EventType.cs). Clear them here rather than fabricate a ScorecardId: the real,
            // scorecard-scoped data gets populated separately from Scorebuddy's migrated staging
            // data (tools/BackfillEventTypes). Evaluations.EventTypeId/EventSubTypeId are
            // optional, so nulling the ones that pointed at this bogus data is safe.
            migrationBuilder.Sql(
                """
                UPDATE Evaluations SET EventTypeId = NULL, EventSubTypeId = NULL
                WHERE EventTypeId IN (SELECT Id FROM EventTypes);
                DELETE FROM EventSubTypes;
                DELETE FROM EventTypes;
                """);

            migrationBuilder.DropIndex(
                name: "IX_EventTypes_Name",
                table: "EventTypes");

            migrationBuilder.AddColumn<Guid>(
                name: "ScorecardId",
                table: "EventTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_ScorecardId_Name",
                table: "EventTypes",
                columns: new[] { "ScorecardId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTypes_Scorecards_ScorecardId",
                table: "EventTypes",
                column: "ScorecardId",
                principalTable: "Scorecards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTypes_Scorecards_ScorecardId",
                table: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_EventTypes_ScorecardId_Name",
                table: "EventTypes");

            migrationBuilder.DropColumn(
                name: "ScorecardId",
                table: "EventTypes");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_Name",
                table: "EventTypes",
                column: "Name",
                unique: true);
        }
    }
}
