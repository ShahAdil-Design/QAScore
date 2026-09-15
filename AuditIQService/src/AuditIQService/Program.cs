using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Middleware;
using AuditIQ.Application;
using AuditIQ.Application.Abstractions.Auth;
using AuditIQ.Infrastructure;
using AuditIQ.Infrastructure.Persistence;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- K8s-mounted config/secrets (see charts/auditiqservice/templates/deployment.yaml) ---
// Both optional so local dev (where these paths don't exist) is unaffected; user-secrets and
// appsettings.*.json still work as normal in that case.
builder.Configuration
    .AddJsonFile("/app/config/appsettings.config.json", optional: true, reloadOnChange: true)
    .AddJsonFile("/app/secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);

// --- Infrastructure (DbContext, Hangfire) and Application (CQRS handlers) ---
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// --- CORS: frontend deploys separately from the API (Section 2/12.C) ---
const string FrontendCorsPolicy = "Frontend";
var frontendOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
{
    if (frontendOrigins.Length > 0)
        policy.WithOrigins(frontendOrigins).AllowAnyHeader().AllowAnyMethod();
}));

// --- API versioning (Section 12.A "Base API conventions") ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// --- Auth: Azure AD SSO proves identity only (Section 9, ADR-09) ---
// Values are read from config, never hardcoded, per Section 12.A #4. Role/responsibility is never
// taken from the Azure AD token itself — AuditIqRoleClaimsTransformation (registered below,
// applies to every scheme) resolves the caller's actual AuditIQ role by matching their token's
// email against our own Users table.
//
// The scheme choice is keyed on whether Sso:Authority is actually a usable https URL, NOT on
// IsDevelopment() — the Helm chart's ASPNETCORE_ENVIRONMENT only distinguishes "prod" from
// everything else, so every non-prod K8s environment (dev01 included) reports as "Development"
// even once real Entra values are injected via Octopus. Gating on IsDevelopment() alone silently
// routed dev01's real Azure AD JWTs through DevAuthHandler (which doesn't parse them at all, just
// grants Admin with no email/user-id claim), producing a permanent 403 NotProvisioned regardless
// of the Users table. DevAuthHandler now only stands in when no *usable* SSO config exists — a
// true local dev box, or an environment SSO hasn't been wired up for yet.
//
// Validating the URL shape (not just non-empty) matters because an unresolved Octopus
// substitution token (literal "#{Sso.Authority}") is non-empty but crashes JwtBearer's
// PostConfigure ("Authority must use HTTPS") on the very first request — that must fail safe to
// DevAuthHandler with a loud log, not crash-loop the pod.
var ssoAuthority = builder.Configuration["Sso:Authority"];
var ssoConfigured = Uri.TryCreate(ssoAuthority, UriKind.Absolute, out var authorityUri)
    && authorityUri.Scheme == Uri.UriSchemeHttps;

if (!string.IsNullOrWhiteSpace(ssoAuthority) && !ssoConfigured)
{
    Console.Error.WriteLine(
        $"WARNING: Sso:Authority is set but is not a valid https URL ('{ssoAuthority}') — " +
        "falling back to DevAuthHandler. Check Octopus variable substitution for Sso.Authority/Sso.Audience.");
}

if (!ssoConfigured)
{
    builder.Services
        .AddAuthentication(DevAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, _ => { });
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = ssoAuthority;
            options.Audience = builder.Configuration["Sso:Audience"];
            options.RequireHttpsMetadata = true;

            // The default WWW-Authenticate: error="invalid_token" header on a failed request
            // never surfaces *why* validation failed (bad signature, issuer, audience, lifetime,
            // metadata-fetch failure...) — dev01 hit exactly this blind spot investigating a
            // rejected-but-apparently-correct token. Log the real exception so it's visible.
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("AuditIQ.Auth.JwtBearer");
                    logger.LogError(context.Exception, "JWT validation failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                },
            };
        });
}

builder.Services.AddScoped<IClaimsTransformation, AuditIqRoleClaimsTransformation>();

builder.Services.AddAuthorization(options => options.AddAuditIQPolicies());

// --- API conventions ---
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AuditIQ API", Version = "v1" });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearerScheme, Array.Empty<string>() } });
});

// --- Global exception handling (Section 12.A #6) ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- Health checks (Section 12.A #8) — DB connectivity, required for GKE probes ---
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuditIQDbContext>();

var app = builder.Build();

// Applies pending EF Core migrations before this pod starts accepting traffic — guarded by a
// SQL Server application lock (MigrationRunner) so concurrent replicas don't race each other's
// DDL on startup. Runs in every environment, including local dev, so `dotnet ef database
// update` is no longer a required manual step.
await MigrationRunner.ApplyMigrationsAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Hangfire dashboard — auth-gated per Section 12.A #9. Locked to Admins; loosen only for local dev.
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAdminAuthorizationFilter()],
    IgnoreAntiforgeryToken = true
});

app.Run();

public partial class Program;

file sealed class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(Roles.Admin);
    }
}
