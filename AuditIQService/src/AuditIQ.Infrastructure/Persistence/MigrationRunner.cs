using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuditIQ.Infrastructure.Persistence;

/// <summary>
/// Applies pending EF Core migrations on startup, guarded by a SQL Server application lock so
/// multiple pod replicas starting simultaneously don't race to create the schema concurrently.
/// Only the pod that acquires the lock actually runs DDL; the others block until it releases,
/// then find migrations already applied and no-op. See the Hangfire schema-missing incident this
/// was built to prevent from recurring for any table, not just Hangfire's.
/// </summary>
public static class MigrationRunner
{
    private const string LockResource = "AuditIQ-Migration";

    public static async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditIQDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuditIQ.Migrations");

        // Opening via the DbContext (rather than a bare SqlConnection) means this still goes
        // through AzureSqlTokenInterceptor when workload identity is enabled — the lock/migrate
        // connection authenticates exactly like every other DbContext connection does.
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();

        try
        {
            await using (var lockCmd = connection.CreateCommand())
            {
                lockCmd.CommandText =
                    "EXEC sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 60000";
                lockCmd.CommandTimeout = 65;
                AddParameter(lockCmd, "@resource", LockResource);

                // Return codes: 0/1 = acquired; negative = timeout/deadlock/error (see sp_getapplock docs).
                var result = Convert.ToInt32(await lockCmd.ExecuteScalarAsync());
                if (result < 0)
                    throw new InvalidOperationException($"Could not acquire migration lock '{LockResource}' (sp_getapplock returned {result}).");
            }

            logger.LogInformation("Migration lock acquired — applying pending EF Core migrations...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations up to date.");
        }
        finally
        {
            // LockOwner=Session also releases automatically when the connection closes below,
            // but release explicitly first so the intent is clear from the log/trace, not just
            // an implicit side effect of disposal.
            await using (var unlockCmd = connection.CreateCommand())
            {
                unlockCmd.CommandText = "EXEC sp_releaseapplock @Resource = @resource, @LockOwner = 'Session'";
                AddParameter(unlockCmd, "@resource", LockResource);
                try { await unlockCmd.ExecuteNonQueryAsync(); }
                catch { /* connection may already be unusable if MigrateAsync itself failed — closing it below releases the lock regardless */ }
            }

            await db.Database.CloseConnectionAsync();
        }
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
