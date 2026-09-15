using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.CreateScorecardCategory;

public sealed class CreateScorecardCategoryCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateScorecardCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateScorecardCategoryCommand command, CancellationToken cancellationToken)
    {
        var nameExists = await db.ScorecardCategories
            .AnyAsync(c => c.Name == command.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<Guid>(ScorecardErrors.DuplicateCategoryName(command.Name));

        var category = new ScorecardCategory { Name = command.Name };

        db.ScorecardCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
