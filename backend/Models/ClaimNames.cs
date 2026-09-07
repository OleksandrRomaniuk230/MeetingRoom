namespace MeetingRoom.Api.Models;

/// <summary>
/// Non-standard claim names used by this API. Registered claims ("sub", "jti", ...) come
/// from <see cref="Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames"/>.
/// </summary>
public static class ClaimNames
{
    /// <summary>Multi-valued claim carrying the user's <see cref="RoleNames"/> values.</summary>
    public const string Roles = "roles";

    /// <summary>Carries <see cref="User.Id"/> as a string.</summary>
    public const string UserId = "uid";
}
