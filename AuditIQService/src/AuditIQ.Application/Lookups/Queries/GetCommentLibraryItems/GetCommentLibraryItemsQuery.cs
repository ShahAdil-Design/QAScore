using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Lookups.Dtos;

namespace AuditIQ.Application.Lookups.Queries.GetCommentLibraryItems;

public sealed record GetCommentLibraryItemsQuery : IQuery<IReadOnlyList<LookupItemDto>>;
