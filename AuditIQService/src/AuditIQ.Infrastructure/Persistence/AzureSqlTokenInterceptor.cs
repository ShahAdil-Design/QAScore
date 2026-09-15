using System.Data.Common;
using System.Threading;
using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuditIQ.Infrastructure.Persistence;

public sealed class AzureSqlTokenInterceptor(TokenCredential credential) : DbConnectionInterceptor
{
    public static readonly string[] Scopes = ["https://database.windows.net/.default"];

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (connection is SqlConnection sqlConnection)
        {
            var token = await credential.GetTokenAsync(new TokenRequestContext(Scopes), cancellationToken);
            sqlConnection.AccessToken = token.Token;
        }

        return result;
    }

    // EF Core treats ConnectionOpening/ConnectionOpeningAsync as independent overrides — anything
    // that opens the connection synchronously (e.g. `dotnet ef database update`/Database.Migrate,
    // or a sync-style health check) would skip token injection entirely without this one too.
    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        if (connection is SqlConnection sqlConnection)
        {
            var token = credential.GetToken(new TokenRequestContext(Scopes), default);
            sqlConnection.AccessToken = token.Token;
        }

        return result;
    }
}
