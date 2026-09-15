using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalibrationAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalibrationAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalibrationParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScorecardQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerValue = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CauseCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LegacySourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationAnswers_CalibrationParticipants_CalibrationParticipantId",
                        column: x => x.CalibrationParticipantId,
                        principalTable: "CalibrationParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalibrationAnswers_ScorecardQuestions_ScorecardQuestionId",
                        column: x => x.ScorecardQuestionId,
                        principalTable: "ScorecardQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationAnswers_CalibrationParticipantId_ScorecardQuestionId",
                table: "CalibrationAnswers",
                columns: new[] { "CalibrationParticipantId", "ScorecardQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationAnswers_LegacySourceId",
                table: "CalibrationAnswers",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationAnswers_ScorecardQuestionId",
                table: "CalibrationAnswers",
                column: "ScorecardQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalibrationAnswers");
        }
    }
}
