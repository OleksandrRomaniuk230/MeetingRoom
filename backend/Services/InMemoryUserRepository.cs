using MeetingRoom.Api.Models;

namespace MeetingRoom.Api.Services;

/// <summary>
/// Process-local user store. Everything is lost on restart and nothing is shared across
/// instances — swap this for an EF Core / database implementation of
/// <see cref="IUserRepository"/> before running more than one node.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _byUsername = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _emails = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _byUsername.TryGetValue(username, out var user);
            return Task.FromResult(user);
        }
    }

    public Task<UserCreationResult> TryAddAsync(User user, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_byUsername.ContainsKey(user.Username))
            {
                return Task.FromResult(UserCreationResult.UsernameTaken);
            }

            if (_emails.Contains(user.Email))
            {
                return Task.FromResult(UserCreationResult.EmailTaken);
            }

            _byUsername.Add(user.Username, user);
            _emails.Add(user.Email);
            return Task.FromResult(UserCreationResult.Created);
        }
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // Entities are handed out by reference, so mutations are already visible. Assigning
        // keeps the contract honest for stores that need an explicit write.
        lock (_gate)
        {
            _byUsername[user.Username] = user;
        }

        return Task.CompletedTask;
    }
}
