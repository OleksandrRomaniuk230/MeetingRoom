using MeetingRoom.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MeetingRoom.Api.Tests.Infrastructure;

/// <summary>
/// Runs the API against its own throwaway SQL Server LocalDB database - a fresh, uniquely
/// named database per instance - instead of the developer's shared "MeetingRoomDb", and
/// instead of SQLite. <see cref="Models.TimeSlot.RowVersion"/> relies on SQL Server's native
/// <c>rowversion</c> type and store-generated concurrency tokens, which the SQLite provider
/// does not support; using the real provider is what makes a concurrency test actually
/// exercise that path instead of a different one.
/// </summary>
/// <remarks>
/// Program.cs reads ConnectionStrings:DefaultConnection and Jwt:Key - and calls
/// AddDbContext/AddSingleton with the values it read - before builder.Build() runs.
/// WebApplicationFactory's ConfigureWebHost/ConfigureAppConfiguration hooks only take effect
/// at Build()-interception time, which for a minimal-hosting Program.cs like this one is
/// *after* those eager reads already happened - so overriding config that way silently has
/// no effect, and the app quietly falls back to whatever appsettings.Development.json (or
/// appsettings.json) already had. Environment variables avoid this: they're one of
/// WebApplication.CreateBuilder's default configuration sources and are already present by
/// the time Program.cs runs, so they must be set before this factory's host is first built
/// (i.e. before the first CreateClient()/Services access).
/// </remarks>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string ConnectionStringVariable = "ConnectionStrings__DefaultConnection";
    private const string JwtKeyVariable = "Jwt__Key";

    private readonly string _connectionString =
        $"Server=(localdb)\\MSSQLLocalDB;Database=MeetingRoomTests_{Guid.NewGuid():N};" +
        "Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public TestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable(ConnectionStringVariable, _connectionString);
        Environment.SetEnvironmentVariable(JwtKeyVariable, "test-only-signing-key-at-least-32-bytes-long!!");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment(Environments.Development);

    /// <summary>
    /// Creates the schema (and its HasData seed rows) on this instance's private database by
    /// applying the real migrations - the same path production uses.
    /// </summary>
    /// <remarks>
    /// Deliberately uses a standalone <see cref="ApplicationDbContext"/> built directly from
    /// <see cref="_connectionString"/>, rather than one resolved from this factory's DI
    /// container via <c>Services.CreateScope()</c>: doing the latter reliably fails the very
    /// first connection to the just-auto-created database with SQL error 4060 ("Cannot open
    /// database ... login failed") when run under the hosted TestServer, for reasons that
    /// didn't reproduce with a plain, unhosted DbContext. Once the database exists, the
    /// hosted app's own DbContext (same connection string) connects to it with no issue.
    /// </remarks>
    public async Task InitializeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(_connectionString).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.MigrateAsync();
    }

    /// <summary>Drops this instance's private database so nothing outlives the test run.</summary>
    public async Task DisposeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(_connectionString).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureDeletedAsync();

        // These are process-wide, so clear them rather than leave a stale test connection
        // string/key around for whatever runs in this process next.
        Environment.SetEnvironmentVariable(ConnectionStringVariable, null);
        Environment.SetEnvironmentVariable(JwtKeyVariable, null);
    }
}
