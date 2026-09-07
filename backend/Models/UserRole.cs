namespace MeetingRoom.Api.Models;

/// <summary>
/// Roles a <see cref="User"/> can hold. Numeric values are persisted, so keep them stable.
/// </summary>
public enum UserRole
{
    /// <summary>Default role granted on self-registration.</summary>
    RegularUser = 0,

    /// <summary>Full administrative access. Never self-assignable.</summary>
    Admin = 1,
}

/// <summary>
/// The string values that appear in the <c>roles</c> claim of an issued token, and
/// which <c>[Authorize(Roles = ...)]</c> matches against.
/// </summary>
public static class RoleNames
{
    public const string RegularUser = "user";

    public const string Admin = "admin";

    public static string ToClaimValue(this UserRole role) => role switch
    {
        UserRole.RegularUser => RegularUser,
        UserRole.Admin => Admin,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unmapped role."),
    };
}
