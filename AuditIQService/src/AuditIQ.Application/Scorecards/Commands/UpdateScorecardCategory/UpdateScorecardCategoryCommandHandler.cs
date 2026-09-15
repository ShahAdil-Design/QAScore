using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.UpdateScorecardCategory;

public sealed class UpdateScorecardCategoryCommandHandler(IApplicationDbContext db) : ICommandHandler<UpdateScorecardCategoryCommand>
{
    public async Task<Result> Handle(UpdateScorecardCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await db.ScorecardCategories.FindAsync([command.Id], cancellationToken);
        if (category is null)
            return Result.Failure(ScorecardErrors.CategoryNotFound(command.Id));

        var nameTaken = await db.ScorecardCategories
            .AnyAsync(c => c.Id != command.Id && c.Name == command.Name, cancellationToken);
        if (nameTaken)
            return Result.Failure(ScorecardErrors.DuplicateCategoryName(command.Name));

        category.Name = command.Name;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
