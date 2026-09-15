using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.UpdateScorecard;

public sealed class UpdateScorecardCommandHandler(IApplicationDbContext db) : ICommandHandler<UpdateScorecardCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UpdateScorecardCommand command, CancellationToken cancellationToken)
    {
        var current = await db.Scorecards
            .FirstOrDefaultAsync(s => s.Id == command.ScorecardId && s.IsCurrentVersion, cancellationToken);

        if (current is null)
            return Result.Failure<Guid>(ScorecardErrors.NotFound(command.ScorecardId));

        if (current.IsArchived)
            return Result.Failure<Guid>(ScorecardErrors.Archived(current.Id));

        if (current.IsLocked)
            return Result.Failure<Guid>(ScorecardErrors.Locked(current.Id));

        var categoryExists = await db.ScorecardCategories.AnyAsync(c => c.Id == command.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result.Failure<Guid>(ScorecardErrors.CategoryNotFound(command.CategoryId));

        var groupIds = command.GroupIds.Distinct().ToList();
        if (groupIds.Count > 0)
        {
            var validGroupCount = await db.Groups.CountAsync(g => groupIds.Contains(g.Id), cancellationToken);
            if (validGroupCount != groupIds.Count)
                return Result.Failure<Guid>(ScorecardErrors.GroupNotFound(groupIds[0]));
        }

        // Two round trips, deliberately not one: IX_Scorecards_CurrentVersion is a unique
        // filtered index on (ScorecardGroupId) WHERE IsCurrentVersion = 1. SQL Server checks
        // a unique index per-statement, not at transaction commit, so flipping the old row off
        // and inserting the new one in the same SaveChanges call risks EF ordering the INSERT
        // before the UPDATE and hitting a transient duplicate-key violation.
        current.IsCurrentVersion = false;
        await db.SaveChangesAsync(cancellationToken);

        var newVersion = new Scorecard
        {
            ScorecardGroupId = current.ScorecardGroupId,
            Version = current.Version + 1,
            IsCurrentVersion = true,
            Name = command.Name,
            Description = command.Description,
            ScorecardType = command.ScorecardType,
            CategoryId = command.CategoryId,
            Location = command.Location,
            TargetPercentage = command.TargetPercentage,
            MaxScore = command.MaxScore,
            IsLocked = false,
            IsArchived = false,
        };

        foreach (var groupId in groupIds)
            newVersion.OrgGroups.Add(new ScorecardOrgGroup { ScorecardId = newVersion.Id, GroupId = groupId });

        foreach (var questionInput in command.Questions)
        {
            var question = new ScorecardQuestion
            {
                ScorecardId = newVersion.Id,
                SectionName = questionInput.SectionName,
                Text = questionInput.Text,
                Weight = questionInput.Weight,
                IsFailLogic = questionInput.IsFailLogic,
                SortOrder = questionInput.SortOrder,
            };

            for (var i = 0; i < questionInput.AnswerOptions.Count; i++)
            {
                var option = questionInput.AnswerOptions[i];
                question.AnswerOptions.Add(new QuestionAnswerOption
                {
                    ScorecardQuestionId = question.Id,
                    Label = option.Label,
                    SortOrder = i,
                    Value = option.Value,
                    IsFailSection = option.IsFailSection,
                    IsFailAll = option.IsFailAll,
                    IsNotApplicable = option.IsNotApplicable,
                });
            }

            newVersion.Questions.Add(question);
        }

        db.Scorecards.Add(newVersion);
        await db.SaveChangesAsync(cancellationToken);

        return newVersion.Id;
    }
}
