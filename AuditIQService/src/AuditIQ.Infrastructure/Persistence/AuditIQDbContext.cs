    using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Infrastructure.Persistence;

public class AuditIQDbContext(DbContextOptions<AuditIQDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<ScreenPermission> ScreenPermissions => Set<ScreenPermission>();
    public DbSet<Team> Teams => Set<Team>();

    public DbSet<ScorecardCategory> ScorecardCategories => Set<ScorecardCategory>();
    public DbSet<Scorecard> Scorecards => Set<Scorecard>();
    public DbSet<ScorecardQuestion> ScorecardQuestions => Set<ScorecardQuestion>();
    public DbSet<QuestionAnswerOption> QuestionAnswerOptions => Set<QuestionAnswerOption>();
    public DbSet<ScorecardOrgGroup> ScorecardOrgGroups => Set<ScorecardOrgGroup>();
    public DbSet<ScorecardTipSheet> ScorecardTipSheets => Set<ScorecardTipSheet>();

    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    public DbSet<EvaluationAnswer> EvaluationAnswers => Set<EvaluationAnswer>();
    public DbSet<EvaluationAttachment> EvaluationAttachments => Set<EvaluationAttachment>();

    public DbSet<CommentLibraryItem> CommentLibraryItems => Set<CommentLibraryItem>();
    public DbSet<CauseCode> CauseCodes => Set<CauseCode>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<EventSubType> EventSubTypes => Set<EventSubType>();

    public DbSet<CalibrationList> CalibrationLists => Set<CalibrationList>();
    public DbSet<CalibrationListItem> CalibrationListItems => Set<CalibrationListItem>();
    public DbSet<CalibrationRating> CalibrationRatings => Set<CalibrationRating>();
    public DbSet<CalibrationAnswer> CalibrationAnswers => Set<CalibrationAnswer>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
    public DbSet<Kudos> Kudos => Set<Kudos>();

    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditIQDbContext).Assembly);

        // Applies to every entity via the shared Entity base class (Epic 2 migration traceability)
        // rather than repeating this in each IEntityTypeConfiguration.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var property = entityType.FindProperty(nameof(Domain.Common.Entity.LegacySourceId));
            if (property is null)
                continue;

            property.SetMaxLength(256);
            entityType.AddIndex(property);
        }
    }
}
