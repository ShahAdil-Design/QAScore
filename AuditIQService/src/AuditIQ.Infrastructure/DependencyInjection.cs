using System.Data.Common;
using AuditIQ.Application.Abstractions.Directory;
using AuditIQ.Application.Abstractions.Notifications;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Time;
using AuditIQ.Application.Abstractions.Auth;
using AuditIQ.Infrastructure.Directory;
using AuditIQ.Infrastructure.Notifications;
using AuditIQ.Infrastructure.Persistence;
using AuditIQ.Infrastructure.Time;
using Azure.Core;
using Azure.Identity;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuditIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // When true, the DB connection is authenticated via Azure AD workload identity
        // federation (GKE-issued OIDC token exchanged for an Azure AD token) instead of
        // SQL auth in the connection string. Off by default so local dev keeps using
        // user-secrets-supplied SQL auth without any extra configuration.
        var useWorkloadIdentity = configuration.GetValue<bool>("Azure:UseWorkloadIdentity");
        if (useWorkloadIdentity)
        {
            services.AddSingleton<TokenCredential, WorkloadIdentityCredential>();
            services.AddSingleton<AzureSqlTokenInterceptor>();
        }

        // Connection string is resolved lazily (per DbContext instance / at Hangfire startup),
        // not captured eagerly here — WebApplicationFactory-based tests override configuration
        // after this method runs, and an eagerly-captured value would silently ignore that
        // override, pointing tests at the real dev database instead of a throwaway test one.
        services.AddScoped<AuditLoggingInterceptor>();
        services.AddDbContext<AuditIQDbContext>((sp, options) =>
        {
            options.UseSqlServer(GetConnectionString(sp));
            options.AddInterceptors(sp.GetRequiredService<AuditLoggingInterceptor>());
            if (useWorkloadIdentity)
                options.AddInterceptors(sp.GetRequiredService<AzureSqlTokenInterceptor>());
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AuditIQDbContext>());

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationJobs, HangfireNotificationJobs>();

        services.Configure<EntraGraphOptions>(configuration.GetSection(EntraGraphOptions.SectionName));
        services.AddHttpClient("Graph", client => client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/"));
        services.AddSingleton<IEntraDirectoryService, EntraDirectoryService>();

        services.AddHangfire((sp, config) =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            var storageOptions = new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            };

            // Hangfire manages its own connections and doesn't go through the EF Core
            // interceptor pipeline, so it needs its own token-bearing connection factory
            // rather than a plain connection string when workload identity is in use.
            if (useWorkloadIdentity)
            {
                var credential = sp.GetRequiredService<TokenCredential>();
                var connectionString = GetConnectionString(sp);
                config.UseSqlServerStorage(() => CreateSqlConnection(connectionString, credential), storageOptions);
            }
            else
            {
                config.UseSqlServerStorage(GetConnectionString(sp), storageOptions);
            }
        });

        // In-process worker for Phase 1 (Section 6/12.A #9 — low job volume; revisit if a
        // separate GKE worker deployment is needed).
        services.AddHangfireServer();

        return services;
    }

    private static string GetConnectionString(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("AuditIQDb")
            ?? throw new InvalidOperationException("Connection string 'AuditIQDb' is not configured.");

    private static DbConnection CreateSqlConnection(string connectionString, TokenCredential credential)
    {
        var connection = new SqlConnection(connectionString);
        var token = credential.GetToken(new TokenRequestContext(AzureSqlTokenInterceptor.Scopes), default);
        connection.AccessToken = token.Token;
        return connection;
    }
}
