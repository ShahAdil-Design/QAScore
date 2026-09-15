namespace AuditIQ.Infrastructure.Directory;

public sealed class EntraGraphOptions
{
    public const string SectionName = "EntraGraph";

    public required string TenantId { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }

    /// <summary>The Entra group whose members populate the Create User email picker.</summary>
    public required string GroupId { get; set; }
}
