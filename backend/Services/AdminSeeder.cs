using MeetingRoom.Api.Configuration;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Data; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; 

namespace MeetingRoom.Api.Services;

public static class AdminSeeder
{
    /// <summary>
    /// Creates the configured admin account if it does not exist yet. No-op when
    /// <see cref="AdminSeedOptions"/> is not fully configured.
    /// </summary>
    public static async Task SeedAdminAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var options = provider.GetRequiredService<AdminSeedOptions>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(AdminSeeder));

        if (!options.IsConfigured)
        {
            logger.LogInformation(
                "No '{Section}' configuration found; skipping admin seed.", AdminSeedOptions.SectionName);
            return;
        }

        var dbContext = provider.GetRequiredService<ApplicationDbContext>();
        
        var targetEmail = string.IsNullOrWhiteSpace(options.Email) ? $"{options.Username}@local" : options.Email;

        var adminExists = await dbContext.Users.AnyAsync(
            u => u.Username == options.Username || u.Email == targetEmail, 
            cancellationToken);

        if (adminExists)
        {
            logger.LogInformation("Admin account {Username} or email {Email} already exists; seed safely skipped.", options.Username, targetEmail);
            return; 
        }

        var users = provider.GetRequiredService<IUserRepository>();
        var hasher = provider.GetRequiredService<IPasswordHasher<User>>();

        var admin = new User
        {
            Username = options.Username!,
            Email = targetEmail,
            Role = UserRole.Admin,
            PasswordHash = string.Empty,
        };

        admin.PasswordHash = hasher.HashPassword(admin, options.Password!);

        var result = await users.TryAddAsync(admin, cancellationToken);
        if (result == UserCreationResult.Created)
        {
            logger.LogWarning(
                "Seeded admin account {Username} from configuration. Rotate its password before production.",
                admin.Username);
        }
        else
        {
            logger.LogInformation("Admin account {Username} already exists; seed skipped.", admin.Username);
        }
    }
}
