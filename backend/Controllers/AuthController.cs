using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoom.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
// Secure the whole controller, then open up only the two endpoints that must work
// without a token. This survives someone adding a new action and forgetting to guard it.
[Authorize]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// Verified against when the username is unknown, so a bad-username attempt costs the
    /// same work as a bad-password one and the response time does not reveal which failed.
    /// Built with default <see cref="PasswordHasherOptions"/> to match the registered hasher.
    /// </summary>
    private static readonly User DecoyUser = new()
    {
        Username = "-",
        Email = "-",
        PasswordHash = "-",
    };

    private static readonly string DecoyHash =
        new PasswordHasher<User>().HashPassword(DecoyUser, "not-a-real-password");

    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokens;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository users,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokens,
        ILogger<AuthController> logger)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
        _logger = logger;
    }

    /// <summary>Creates a regular-user account and returns a token for it.</summary>
    /// <remarks>
    /// Anonymous by necessity — a caller has no token yet. The created account is always
    /// <see cref="UserRole.RegularUser"/>; the role is not read from the request body, so
    /// this endpoint cannot be used to mint an administrator.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            // Self-registration never grants Admin; see AdminSeedOptions for how admins appear.
            Role = UserRole.RegularUser,
            PasswordHash = string.Empty,
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var result = await _users.TryAddAsync(user, cancellationToken);
        if (result != UserCreationResult.Created)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration failed",
                Detail = result == UserCreationResult.UsernameTaken
                    ? "That username is already taken."
                    : "That email address is already registered.",
            });
        }

        _logger.LogInformation("Registered user {Username} with role {Role}.", user.Username, user.Role);

        var token = _tokens.CreateAccessToken(user);
        var response = new AuthResponse(token.Value, token.ExpiresAtUtc, UserResponse.From(user));

        return CreatedAtAction(nameof(Me), null, response);
    }

    /// <summary>Exchanges credentials for a signed JWT carrying the user's role claim.</summary>
    /// <remarks>Anonymous by necessity — this is where tokens come from.</remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _users.FindByUsernameAsync(request.Username, cancellationToken);

        var verification = _passwordHasher.VerifyHashedPassword(
            user ?? DecoyUser,
            user?.PasswordHash ?? DecoyHash,
            request.Password);

        if (user is null || verification == PasswordVerificationResult.Failed)
        {
            _logger.LogInformation("Failed login attempt for {Username}.", request.Username);

            // One message for both causes, so the endpoint is not a username oracle.
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials",
                Detail = "The username or password is incorrect.",
            });
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _users.UpdateAsync(user, cancellationToken);
        }

        var token = _tokens.CreateAccessToken(user);

        return Ok(new AuthResponse(token.Value, token.ExpiresAtUtc, UserResponse.From(user)));
    }

    /// <summary>Echoes the identity carried by the bearer token.</summary>
    /// <remarks>Requires authentication via the controller-level <c>[Authorize]</c>.</remarks>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me() => Ok(new
    {
        Username = User.Identity?.Name,
        Roles = User.FindAll(ClaimNames.Roles).Select(claim => claim.Value),
        IsAdmin = User.IsInRole(RoleNames.Admin),
    });
}
