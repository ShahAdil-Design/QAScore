using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AuditIQ.Application.Abstractions.Directory;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace AuditIQ.Infrastructure.Directory;

/// <summary>
/// Client-credentials (application-permission) Graph access — no signed-in user involved, since
/// this backs an Admin's Create User form, not something done on a user's own behalf. Requires
/// GroupMember.Read.All granted (with admin consent) on the app registration in EntraGraphOptions.
/// </summary>
public sealed class EntraDirectoryService(IHttpClientFactory httpClientFactory, IOptions<EntraGraphOptions> options) : IEntraDirectoryService
{
    private static readonly string[] Scopes = ["https://graph.microsoft.com/.default"];

    private readonly SemaphoreSlim _lock = new(1, 1);
    private ClientSecretCredential? _credential;
    private AccessToken _cachedToken;

    public async Task<IReadOnlyList<EntraDirectoryMember>> GetConfiguredGroupMembersAsync(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Graph");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync(cancellationToken));

        var members = new List<EntraDirectoryMember>();
        // $select keeps the payload small; only direct members (not nested-group members) —
        // matches how the group is expected to be maintained (flat membership).
        var requestUri = $"groups/{options.Value.GroupId}/members?$select=id,displayName,mail,userPrincipalName&$top=999";

        while (requestUri is not null)
        {
            var response = await client.GetFromJsonAsync<GraphMembersResponse>(requestUri, cancellationToken)
                ?? throw new InvalidOperationException("Microsoft Graph returned an empty response listing group members.");

            foreach (var member in response.Value)
            {
                // Groups can contain other groups or service principals, not just users — only
                // "#microsoft.graph.user" objects are real people who can be provisioned as an
                // AuditIQ User.
                if (member.ODataType != "#microsoft.graph.user")
                    continue;

                members.Add(new EntraDirectoryMember(member.Id, member.DisplayName ?? member.UserPrincipalName ?? member.Id, member.Mail ?? member.UserPrincipalName));
            }

            requestUri = response.NextLink;
        }

        return members;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken.Token is not null && _cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(2))
            return _cachedToken.Token;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedToken.Token is not null && _cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(2))
                return _cachedToken.Token;

            _credential ??= new ClientSecretCredential(options.Value.TenantId, options.Value.ClientId, options.Value.ClientSecret);
            _cachedToken = await _credential.GetTokenAsync(new TokenRequestContext(Scopes), cancellationToken);
            return _cachedToken.Token;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed record GraphMembersResponse(
        [property: JsonPropertyName("value")] GraphDirectoryObject[] Value,
        [property: JsonPropertyName("@odata.nextLink")] string? NextLink);

    private sealed record GraphDirectoryObject(
        [property: JsonPropertyName("@odata.type")] string? ODataType,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("displayName")] string? DisplayName,
        [property: JsonPropertyName("mail")] string? Mail,
        [property: JsonPropertyName("userPrincipalName")] string? UserPrincipalName);
}
