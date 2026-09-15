using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.CreateScorecard;

public sealed class CreateScorecardCommandHandler(IApplicationDbContext db) : ICommandHandler<CreateScorecardCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateScorecardCommand command, CancellationToken cancellationToken)
    {
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

        var scorecard = new Scorecard
        {
            ScorecardGroupId = Guid.NewGuid(),
            Version = 1,
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
            scorecard.OrgGroups.Add(new ScorecardOrgGroup { ScorecardId = scorecard.Id, GroupId = groupId });

        foreach (var questionInput in command.Questions)
        {
            var question = new ScorecardQuestion
            {
                ScorecardId = scorecard.Id,
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

            scorecard.Questions.Add(question);
        }

        db.Scorecards.Add(scorecard);
        await db.SaveChangesAsync(cancellationToken);

        return scorecard.Id;
    }
}
