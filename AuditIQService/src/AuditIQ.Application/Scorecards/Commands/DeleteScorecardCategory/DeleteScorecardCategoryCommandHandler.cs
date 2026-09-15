using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.DeleteScorecardCategory;

public sealed class DeleteScorecardCategoryCommandHandler(IApplicationDbContext db) : ICommandHandler<DeleteScorecardCategoryCommand>
{
    public async Task<Result> Handle(DeleteScorecardCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await db.ScorecardCategories.FindAsync([command.Id], cancellationToken);
        if (category is null)
            return Result.Failure(ScorecardErrors.CategoryNotFound(command.Id));

        var inUse = await db.Scorecards.AnyAsync(s => s.CategoryId == command.Id, cancellationToken);
        if (inUse)
            return Result.Failure(ScorecardErrors.CategoryInUse(command.Id));

        db.ScorecardCategories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
