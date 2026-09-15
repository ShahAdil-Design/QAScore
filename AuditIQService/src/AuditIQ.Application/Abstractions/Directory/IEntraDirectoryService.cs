namespace AuditIQ.Application.Abstractions.Directory;

public sealed record EntraDirectoryMember(string ObjectId, string DisplayName, string? Email);

/// <summary>
/// Looks up members of a specific Entra ID (Azure AD) group via Microsoft Graph — backs the
/// Create User form's email picker, so an Admin selects a real directory account instead of
/// free-typing an email that may not match any real identity once SSO goes live (Section 13).
/// </summary>
public interface IEntraDirectoryService
{
    Task<IReadOnlyList<EntraDirectoryMember>> GetConfiguredGroupMembersAsync(CancellationToken cancellationToken);
}
