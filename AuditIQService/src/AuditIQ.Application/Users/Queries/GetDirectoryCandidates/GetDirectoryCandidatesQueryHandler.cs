using AuditIQ.Application.Abstractions.Directory;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.Extensions.Logging;

namespace AuditIQ.Application.Users.Queries.GetDirectoryCandidates;

public sealed class GetDirectoryCandidatesQueryHandler(IEntraDirectoryService directoryService, ILogger<GetDirectoryCandidatesQueryHandler> logger)
    : IQueryHandler<GetDirectoryCandidatesQuery, IReadOnlyList<EntraDirectoryMember>>
{
    public async Task<Result<IReadOnlyList<EntraDirectoryMember>>> Handle(GetDirectoryCandidatesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var members = await directoryService.GetConfiguredGroupMembersAsync(cancellationToken);
            return Result.Success(members);
        }
        catch (Exception ex)
        {
            // Graph outages/misconfiguration shouldn't 500 the whole request — the Create User
            // form falls back to free-typing an email when this fails (see frontend).
            logger.LogError(ex, "Failed to fetch Entra directory group members.");
            return Result.Failure<IReadOnlyList<EntraDirectoryMember>>(
                UserErrors.DirectoryUnavailable);
        }
    }
}
