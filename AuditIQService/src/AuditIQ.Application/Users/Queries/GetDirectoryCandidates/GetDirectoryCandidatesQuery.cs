using AuditIQ.Application.Abstractions.Directory;
using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Users.Queries.GetDirectoryCandidates;

/// <summary>Members of the configured Entra ID group — candidates for the Create User email picker.</summary>
public sealed record GetDirectoryCandidatesQuery : IQuery<IReadOnlyList<EntraDirectoryMember>>;
