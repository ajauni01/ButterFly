using System.Text.Json.Serialization;
using Butterfly.Api.Auth;
using Butterfly.Api.Infrastructure;
using Butterfly.Api.Services;
using Butterfly.Data;
using Butterfly.Shared.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---- Authentication: Microsoft Entra External ID (CIAM) ----
// Validates Entra-issued JWTs. App Roles arrive in the "roles" claim; Microsoft.Identity.Web
// wires that up so [Authorize(Roles = "...")] works directly. No token issuance here.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration, configSectionName: "EntraExternalId");

// Emit our consistent ErrorDto on auth challenge/forbid instead of an empty 401/403 body.
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events ??= new JwtBearerEvents();
    options.Events.OnChallenge = async ctx =>
    {
        ctx.HandleResponse();
        await ErrorResponse.WriteAsync(ctx.Response, StatusCodes.Status401Unauthorized,
            "unauthenticated", "A valid Entra access token is required.");
    };
    options.Events.OnForbidden = async ctx =>
        await ErrorResponse.WriteAsync(ctx.Response, StatusCodes.Status403Forbidden,
            "forbidden", "Your role does not permit this action.");
});

builder.Services.AddAuthorization();

// ---- Persistence ----
builder.Services.AddDbContext<ButterflyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ButterflyDb")));

// ---- App services ----
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IUserProvisioningService, UserProvisioningService>();

// ---- MVC + JSON ----
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Return validation failures as ErrorDto (code "validation_failed" + per-field errors).
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var dto = new ErrorDto
        {
            Code = "validation_failed",
            Message = "One or more fields are invalid.",
            Errors = errors
        };
        return new BadRequestObjectResult(dto);
    };
});

// ---- Swagger (dev), configured for Entra External ID OAuth2 auth-code + PKCE ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Butterfly API", Version = "v1" });

    var authority = builder.Configuration["EntraExternalId:Authority"];
    var apiScopeUri = builder.Configuration["EntraExternalId:ApiScopeUri"];
    var scopeName = builder.Configuration["Swagger:ApiScope"] ?? "access_as_user";

    // Only wire the OAuth2 flow when real (non-placeholder) config is present. The default
    // appsettings ships "<your-tenant>" markers that are not valid URIs — skip them so Swagger
    // still renders before the Entra tenant exists.
    var hasRealAuthority = !string.IsNullOrWhiteSpace(authority)
        && !authority.Contains('<')
        && Uri.TryCreate(authority, UriKind.Absolute, out _);

    if (hasRealAuthority && !string.IsNullOrWhiteSpace(apiScopeUri) && !apiScopeUri.Contains('<'))
    {
        var fullScope = $"{apiScopeUri.TrimEnd('/')}/{scopeName}";
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority.TrimEnd('/')}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"{authority.TrimEnd('/')}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        [fullScope] = "Access the Butterfly API as the signed-in user"
                    }
                }
            }
        };
        options.AddSecurityDefinition("EntraExternalId", scheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "EntraExternalId" }
            }] = new List<string>()
        });
    }

    // Surface XML docs from the API and Shared contracts.
    foreach (var xml in new[] { "Butterfly.Api.xml", "Butterfly.Shared.xml" })
    {
        var path = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(path))
            options.IncludeXmlComments(path, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.OAuthClientId(builder.Configuration["Swagger:ClientId"]);
        ui.OAuthUsePkce();
        ui.OAuthScopeSeparator(" ");
    });

    // Dev-only: ensure schema + demo data exist without a manual migration step.
    // Resilient: before Azure SQL is provisioned there may be no reachable database — a failure
    // here must not stop the API from starting (so Swagger and token validation still work).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ButterflyDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        if (db.Database.IsRelational() && db.Database.GetPendingMigrations().Any())
            await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Dev database init skipped — no reachable database yet. " +
            "Set ConnectionStrings:ButterflyDb (user-secrets) once Azure SQL / local SQL is available.");
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Exposed so integration tests can host the API via WebApplicationFactory.</summary>
public partial class Program { }
