using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RebuildCalibrationAsLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationAnswers_CalibrationParticipants_CalibrationParticipantId",
                table: "CalibrationAnswers");

            migrationBuilder.DropTable(
                name: "CalibrationParticipants");

            migrationBuilder.DropTable(
                name: "CalibrationSessions");

            migrationBuilder.RenameColumn(
                name: "CalibrationParticipantId",
                table: "CalibrationAnswers",
                newName: "CalibrationRatingId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationAnswers_CalibrationParticipantId_ScorecardQuestionId",
                table: "CalibrationAnswers",
                newName: "IX_CalibrationAnswers_CalibrationRatingId_ScorecardQuestionId");

            migrationBuilder.CreateTable(
                name: "CalibrationLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    VisibilityScope = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LegacySourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationLists_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalibrationListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalibrationListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegacySourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationListItems_CalibrationLists_CalibrationListId",
                        column: x => x.CalibrationListId,
                        principalTable: "CalibrationLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalibrationListItems_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalibrationRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalibrationListItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LegacySourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationRatings_CalibrationListItems_CalibrationListItemId",
                        column: x => x.CalibrationListItemId,
                        principalTable: "CalibrationListItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalibrationRatings_Users_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationListItems_CalibrationListId_EvaluationId",
                table: "CalibrationListItems",
                columns: new[] { "CalibrationListId", "EvaluationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationListItems_EvaluationId",
                table: "CalibrationListItems",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationListItems_LegacySourceId",
                table: "CalibrationListItems",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationLists_CreatedByUserId",
                table: "CalibrationLists",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationLists_LegacySourceId",
                table: "CalibrationLists",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationRatings_CalibrationListItemId_EvaluatorId",
                table: "CalibrationRatings",
                columns: new[] { "CalibrationListItemId", "EvaluatorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationRatings_EvaluatorId",
                table: "CalibrationRatings",
                column: "EvaluatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationRatings_LegacySourceId",
                table: "CalibrationRatings",
                column: "LegacySourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationAnswers_CalibrationRatings_CalibrationRatingId",
                table: "CalibrationAnswers",
                column: "CalibrationRatingId",
                principalTable: "CalibrationRatings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalibrationAnswers_CalibrationRatings_CalibrationRatingId",
                table: "CalibrationAnswers");

            migrationBuilder.DropTable(
                name: "CalibrationRatings");

            migrationBuilder.DropTable(
                name: "CalibrationListItems");

            migrationBuilder.DropTable(
                name: "CalibrationLists");

            migrationBuilder.RenameColumn(
                name: "CalibrationRatingId",
                table: "CalibrationAnswers",
                newName: "CalibrationParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_CalibrationAnswers_CalibrationRatingId_ScorecardQuestionId",
                table: "CalibrationAnswers",
                newName: "IX_CalibrationAnswers_CalibrationParticipantId_ScorecardQuestionId");

            migrationBuilder.CreateTable(
                name: "CalibrationSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegacySourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VisibilityScope = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationSessions_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalibrationParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalibrationSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegacySourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationParticipants_CalibrationSessions_CalibrationSessionId",
                        column: x => x.CalibrationSessionId,
                        principalTable: "CalibrationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalibrationParticipants_Users_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationParticipants_CalibrationSessionId_EvaluatorId",
                table: "CalibrationParticipants",
                columns: new[] { "CalibrationSessionId", "EvaluatorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationParticipants_EvaluatorId",
                table: "CalibrationParticipants",
                column: "EvaluatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationParticipants_LegacySourceId",
                table: "CalibrationParticipants",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationSessions_EvaluationId",
                table: "CalibrationSessions",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationSessions_LegacySourceId",
                table: "CalibrationSessions",
                column: "LegacySourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalibrationAnswers_CalibrationParticipants_CalibrationParticipantId",
                table: "CalibrationAnswers",
                column: "CalibrationParticipantId",
                principalTable: "CalibrationParticipants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
