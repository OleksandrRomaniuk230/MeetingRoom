using System.ComponentModel.DataAnnotations;

namespace MeetingRoom.Api.Models;

public sealed record RegisterRequest
{
    [Required]
    [StringLength(32, MinimumLength = 3)]
    [RegularExpression(
        "^[A-Za-z0-9._-]+$",
        ErrorMessage = "Username may only contain letters, digits, dots, underscores and hyphens.")]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; init; }

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public required string Password { get; init; }
}

public sealed record LoginRequest
{
    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }
}

/// <summary>Returned by both register and login.</summary>
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserResponse User);

public sealed record UserResponse(Guid Id, string Username, string Email, string Role)
{
    public static UserResponse From(User user) =>
        new(user.Id, user.Username, user.Email, user.Role.ToClaimValue());
}
