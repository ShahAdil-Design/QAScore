using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacySourceIdTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "Teams",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "Surveys",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "SurveyResponses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "ScorecardTipSheets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "Scorecards",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "ScorecardQuestions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "ScorecardCategories",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "QuestionAnswerOptions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "Kudos",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "Groups",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "EventTypes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "EventSubTypes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "Evaluations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "EvaluationAttachments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "EvaluationAnswers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "CommentLibraryItems",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "CauseCodes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "CalibrationSessions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySourceId",
                table: "CalibrationParticipants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_LegacySourceId",
                table: "Users",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LegacySourceId",
                table: "Teams",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_LegacySourceId",
                table: "Surveys",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_LegacySourceId",
                table: "SurveyResponses",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardTipSheets_LegacySourceId",
                table: "ScorecardTipSheets",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Scorecards_LegacySourceId",
                table: "Scorecards",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardQuestions_LegacySourceId",
                table: "ScorecardQuestions",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScorecardCategories_LegacySourceId",
                table: "ScorecardCategories",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswerOptions_LegacySourceId",
                table: "QuestionAnswerOptions",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Kudos_LegacySourceId",
                table: "Kudos",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LegacySourceId",
                table: "Groups",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_LegacySourceId",
                table: "EventTypes",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSubTypes_LegacySourceId",
                table: "EventSubTypes",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_LegacySourceId",
                table: "Evaluations",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationAttachments_LegacySourceId",
                table: "EvaluationAttachments",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationAnswers_LegacySourceId",
                table: "EvaluationAnswers",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLibraryItems_LegacySourceId",
                table: "CommentLibraryItems",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CauseCodes_LegacySourceId",
                table: "CauseCodes",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationSessions_LegacySourceId",
                table: "CalibrationSessions",
                column: "LegacySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationParticipants_LegacySourceId",
                table: "CalibrationParticipants",
                column: "LegacySourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_LegacySourceId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Teams_LegacySourceId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_LegacySourceId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_LegacySourceId",
                table: "SurveyResponses");

            migrationBuilder.DropIndex(
                name: "IX_ScorecardTipSheets_LegacySourceId",
                table: "ScorecardTipSheets");

            migrationBuilder.DropIndex(
                name: "IX_Scorecards_LegacySourceId",
                table: "Scorecards");

            migrationBuilder.DropIndex(
                name: "IX_ScorecardQuestions_LegacySourceId",
                table: "ScorecardQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ScorecardCategories_LegacySourceId",
                table: "ScorecardCategories");

            migrationBuilder.DropIndex(
                name: "IX_QuestionAnswerOptions_LegacySourceId",
                table: "QuestionAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_Kudos_LegacySourceId",
                table: "Kudos");

            migrationBuilder.DropIndex(
                name: "IX_Groups_LegacySourceId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_EventTypes_LegacySourceId",
                table: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_EventSubTypes_LegacySourceId",
                table: "EventSubTypes");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_LegacySourceId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationAttachments_LegacySourceId",
                table: "EvaluationAttachments");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationAnswers_LegacySourceId",
                table: "EvaluationAnswers");

            migrationBuilder.DropIndex(
                name: "IX_CommentLibraryItems_LegacySourceId",
                table: "CommentLibraryItems");

            migrationBuilder.DropIndex(
                name: "IX_CauseCodes_LegacySourceId",
                table: "CauseCodes");

            migrationBuilder.DropIndex(
                name: "IX_CalibrationSessions_LegacySourceId",
                table: "CalibrationSessions");

            migrationBuilder.DropIndex(
                name: "IX_CalibrationParticipants_LegacySourceId",
                table: "CalibrationParticipants");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "SurveyResponses");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "ScorecardTipSheets");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "Scorecards");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "ScorecardQuestions");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "ScorecardCategories");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "QuestionAnswerOptions");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "Kudos");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "EventTypes");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "EventSubTypes");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "EvaluationAttachments");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "EvaluationAnswers");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "CommentLibraryItems");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "CauseCodes");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "CalibrationSessions");

            migrationBuilder.DropColumn(
                name: "LegacySourceId",
                table: "CalibrationParticipants");
        }
    }
}
