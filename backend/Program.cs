using System.Text;
using MeetingRoom.Api.Authorization;
using MeetingRoom.Api.Configuration;
using MeetingRoom.Api.Data;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------
// JWT bearer authentication
// ---------------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Missing '{JwtOptions.SectionName}' configuration section.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException(
        $"'{JwtOptions.SectionName}:Key' is not configured. Set it via user secrets or the " +
        $"{JwtOptions.SectionName}__Key environment variable.");
}

// HS256 requires a key of at least 256 bits.
var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Key);
if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        $"'{JwtOptions.SectionName}:Key' must be at least 32 bytes (256 bits) to sign HS256 tokens.");
}

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton(new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep the original JWT claim names ("sub", "roles") instead of remapping
        // them to the legacy WS-Federation URIs.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimNames.Roles,
        };
    });

// ---------------------------------------------------------------------------
// Authorization
// ---------------------------------------------------------------------------
// The fallback policy denies by default: any endpoint that does not carry its own
// [Authorize] or [AllowAnonymous] still requires a valid bearer token. New endpoints are
// therefore private until someone deliberately opens them up, rather than public until
// someone remembers to lock them down.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(AuthPolicies.AdminOnly, policy => policy
        .RequireAuthenticatedUser()
        .RequireRole(RoleNames.Admin));

// ---------------------------------------------------------------------------
// Database
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "'ConnectionStrings:DefaultConnection' is not configured. Set it via user secrets or the " +
        "ConnectionStrings__DefaultConnection environment variable.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IRoomRepository, EfRoomRepository>();
builder.Services.AddScoped<ITimeSlotRepository, EfTimeSlotRepository>();

// ---------------------------------------------------------------------------
// Identity services
// ---------------------------------------------------------------------------
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.AddSingleton(builder.Configuration
    .GetSection(AdminSeedOptions.SectionName)
    .Get<AdminSeedOptions>() ?? new AdminSeedOptions());

// InMemoryUserRepository keeps accounts in process memory only; replace with a
// database-backed IUserRepository before deploying more than a single instance.
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

var app = builder.Build();

await app.Services.SeedAdminAsync();

if (app.Environment.IsDevelopment())
{
    // The fallback policy would otherwise demand a token for the document itself, which
    // you cannot get without first reading the document.
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();

// Order matters and is load-bearing:
//   UseAuthentication  - reads/validates the bearer token, builds HttpContext.User
//   UseAuthorization   - evaluates [Authorize]/policies against that User
//   MapControllers     - runs the endpoint only once authorization has passed
// Swapping the first two leaves User unauthenticated at authorization time, which turns
// every protected endpoint into a blanket 401.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
