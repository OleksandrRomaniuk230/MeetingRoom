namespace MeetingRoom.Api.Configuration;

/// <summary>
/// Optional bootstrap admin. Registration always creates a regular user, so without a
/// seeded admin no account could ever hold <see cref="Models.UserRole.Admin"/>.
/// Seeding is skipped entirely when username or password is missing.
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string? Username { get; init; }

    public string? Email { get; init; }

    public string? Password { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
