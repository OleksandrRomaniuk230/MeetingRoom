using MeetingRoom.Api.Configuration;
using MeetingRoom.Api.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MeetingRoom.Api.Services;

/// <summary>
/// Issues HS256 tokens whose issuer, audience, lifetime and signing key line up with the
/// <c>TokenValidationParameters</c> configured in <c>Program.cs</c>.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenService(JwtOptions options, SigningCredentials signingCredentials)
    {
        _options = options;
        _signingCredentials = signingCredentials;
    }

    public AccessToken CreateAccessToken(User user)
    {
        // Second precision: the JWT "exp"/"iat" claims are NumericDate, so a sub-second
        // component here would not survive the round trip and the value we hand back
        // would disagree with the token.
        var now = DateTime.UtcNow;
        var issuedAt = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond));
        var expiresAt = issuedAt.AddMinutes(_options.ExpiryMinutes);

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = _signingCredentials,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Username,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                ["uid"] = user.Id.ToString(),

                // Always an array so consumers can parse one shape once multiple roles exist.
                [ClaimNames.Roles] = new[] { user.Role.ToClaimValue() },
            },
        });

        return new AccessToken(token, expiresAt);
    }
}
