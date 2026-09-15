using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IApplicationDbContext db) : IQueryHandler<GetUserByIdQuery, UserDetailDto>
{
    public async Task<Result<UserDetailDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserTeams).ThenInclude(ut => ut.Team!).ThenInclude(t => t.Group)
            .Include(u => u.UserGroups).ThenInclude(ug => ug.Group!)
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UserDetailDto>(UserErrors.NotFound(query.UserId));

        var teams = user.UserTeams
            .Select(ut => new TeamMembershipDto(ut.Team!.Id, ut.Team.Name, ut.Team.GroupId, ut.Team.Group!.Name))
            .ToList();

        var groups = user.UserGroups
            .Select(ug => new GroupMembershipDto(ug.Group!.Id, ug.Group.Name))
            .ToList();

        return new UserDetailDto(
            user.Id, user.DisplayName, user.Email, user.Role, user.IsActive,
            user.EmploymentType, user.Notes, teams, groups);
    }
}
