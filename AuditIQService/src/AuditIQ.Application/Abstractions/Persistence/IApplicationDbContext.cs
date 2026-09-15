using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Abstractions.Persistence;

/// <summary>
/// The slice of AuditIQDbContext handlers are allowed to see. Handlers depend on
/// this interface (implemented by AuditIQDbContext in Infrastructure), not the
/// concrete DbContext, keeping Application free of an Infrastructure reference.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Group> Groups { get; }
    DbSet<ScreenPermission> ScreenPermissions { get; }
    DbSet<Team> Teams { get; }

    DbSet<ScorecardCategory> ScorecardCategories { get; }
    DbSet<Scorecard> Scorecards { get; }
    DbSet<ScorecardQuestion> ScorecardQuestions { get; }
    DbSet<QuestionAnswerOption> QuestionAnswerOptions { get; }
    DbSet<ScorecardOrgGroup> ScorecardOrgGroups { get; }
    DbSet<ScorecardTipSheet> ScorecardTipSheets { get; }

    DbSet<Evaluation> Evaluations { get; }
    DbSet<EvaluationAnswer> EvaluationAnswers { get; }
    DbSet<EvaluationAttachment> EvaluationAttachments { get; }

    DbSet<CommentLibraryItem> CommentLibraryItems { get; }
    DbSet<CauseCode> CauseCodes { get; }
    DbSet<EventType> EventTypes { get; }
    DbSet<EventSubType> EventSubTypes { get; }

    DbSet<CalibrationList> CalibrationLists { get; }
    DbSet<CalibrationListItem> CalibrationListItems { get; }
    DbSet<CalibrationRating> CalibrationRatings { get; }
    DbSet<CalibrationAnswer> CalibrationAnswers { get; }
    DbSet<Survey> Surveys { get; }
    DbSet<SurveyResponse> SurveyResponses { get; }
    DbSet<Kudos> Kudos { get; }

    DbSet<AuditLogEntry> AuditLogEntries { get; }

    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
