namespace MeetingRoom.Api.Models;

/// <summary>
/// A registered account. <see cref="PasswordHash"/> holds an ASP.NET Core Identity
/// PBKDF2 hash — the plaintext password is never stored or logged.
/// </summary>
public sealed class User
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>Login name. Compared case-insensitively; stored as the user typed it.</summary>
    public required string Username { get; init; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.RegularUser;

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
