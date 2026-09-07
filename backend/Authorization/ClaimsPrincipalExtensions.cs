using System.Security.Claims;
using MeetingRoom.Api.Models;

namespace MeetingRoom.Api.Authorization;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The authenticated user's id, from the <see cref="ClaimNames.UserId"/> claim embedded
    /// at token issuance. Every token this API issues carries it, so a missing or malformed
    /// value means a validated token somehow bypassed <see cref="Services.JwtTokenService"/>.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimNames.UserId), out var id)
            ? id
            : throw new InvalidOperationException(
                $"Authenticated principal is missing a valid '{ClaimNames.UserId}' claim.");
}
