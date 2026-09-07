using MeetingRoom.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace MeetingRoom.Api.Authorization;

/// <summary>
/// Named authorization policies. Prefer these over hand-written
/// <c>[Authorize(Roles = "admin")]</c> strings so the role name lives in exactly one place
/// (<see cref="RoleNames"/>) and a typo becomes a startup error instead of a silent
/// always-deny.
/// </summary>
public static class AuthPolicies
{
    /// <summary>
    /// Administrative surface: room creation/editing/deletion, user administration.
    /// Satisfied only by <see cref="UserRole.Admin"/>.
    /// </summary>
    public const string AdminOnly = "AdminOnly";
}

/// <summary>
/// Restricts an endpoint or controller to administrators.
/// Use on administrative features such as room management:
/// <code>
/// [ApiController]
/// [Route("api/rooms")]
/// public sealed class RoomsController : ControllerBase
/// {
///     [HttpGet]                    // any authenticated user (fallback policy)
///     public IActionResult List() => ...;
///
///     [HttpPost, AuthorizeAdmin]   // admins only -> 403 for regular users
///     public IActionResult Create(...) => ...;
/// }
/// </code>
/// </summary>
/// <remarks>
/// There is deliberately no matching "regular user only" attribute. A plain
/// <c>[Authorize]</c> — or no attribute at all, thanks to the fallback policy — already
/// admits every signed-in account, and administrators should not be shut out of ordinary
/// user-facing features.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class AuthorizeAdminAttribute : AuthorizeAttribute
{
    public AuthorizeAdminAttribute() : base(AuthPolicies.AdminOnly)
    {
    }
}
