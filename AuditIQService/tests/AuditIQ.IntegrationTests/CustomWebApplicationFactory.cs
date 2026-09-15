using AuditIQ.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuditIQ.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // CI sets this to its own sa password; falls back to Password12! (CI's actual password) so
    // CI needs no env var at all. Local runs against the DevReset2026!-reset sql1 container
    // export SQL_SA_PASSWORD=DevReset2026! before `dotnet test` instead of editing this literal.
    private static readonly string SaPassword =
        Environment.GetEnvironmentVariable("SQL_SA_PASSWORD") ?? "Password12!";

    private readonly string _connectionString =
        $"Server=localhost,1433;Database=AuditIQ.IntegrationTest.{Guid.NewGuid():N};User Id=sa;Password={SaPassword};TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AuditIQDb"] = _connectionString,
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    /// <summary>
    /// Creates and migrates the test database using a standalone DbContext — deliberately
    /// NOT resolved via this factory's Services, because first access to Services/CreateClient
    /// starts the whole host, including Hangfire's hosted server, which tries to connect
    /// immediately. If the database doesn't exist yet at that point, Hangfire's connection
    /// fails with a login error (SQL Server's ambiguous error for "no such database"), and
    /// nothing later fixes it for that storage instance. Migrating before Services is ever
    /// touched guarantees the database exists before Hangfire's hosted service starts.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<AuditIQDbContext>().UseSqlServer(_connectionString).Options;
        await using var db = new AuditIQDbContext(options);
        await db.Database.MigrateAsync();
    }

    public async Task DropDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<AuditIQDbContext>().UseSqlServer(_connectionString).Options;
        await using var db = new AuditIQDbContext(options);
        await db.Database.EnsureDeletedAsync();
    }
}
