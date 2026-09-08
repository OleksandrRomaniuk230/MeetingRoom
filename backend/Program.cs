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
builder.Services.AddSignalR();
builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------
// CORS Policy Configuration
// ---------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Allow frontend development port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Mandated to establish real-time SignalR WebSockets
    });
});

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
builder.Services.AddScoped<IBookingRepository, EfBookingRepository>();

// ---------------------------------------------------------------------------
// Identity services
// ---------------------------------------------------------------------------
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.AddSingleton(builder.Configuration
    .GetSection(AdminSeedOptions.SectionName)
    .Get<AdminSeedOptions>() ?? new AdminSeedOptions());

builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

var app = builder.Build();

await app.Services.SeedAdminAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

// ---------------------------------------------------------------------------
// HTTP Request Pipeline Configuration (ORDER MATTERS)
// ---------------------------------------------------------------------------

app.UseCors();

// 2. Safely capture redirects without clashing on local development environments
// app.UseHttpsRedirection(); // Commented out to eliminate the "Failed to determine the https port" local warning

// 3. Evaluate identity token claims [3.2]
app.UseAuthentication();
app.UseAuthorization();

// 4. Map active WebSocket hub pipelines and standard API controllers [7.1]
app.MapHub<MeetingRoom.Api.Hubs.BookingHub>("/api/hubs/bookings");
app.MapControllers();

app.Run();
