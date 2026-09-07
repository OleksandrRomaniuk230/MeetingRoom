using MeetingRoom.Api.Models;

namespace MeetingRoom.Api.Services;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts <paramref name="user"/> only if the username and email are both free.
    /// Atomic, so two concurrent registrations cannot both win.
    /// </summary>
    /// <returns>
    /// <see cref="UserCreationResult.Created"/>, or the conflict that blocked the insert.
    /// </returns>
    Task<UserCreationResult> TryAddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}

public enum UserCreationResult
{
    Created = 0,
    UsernameTaken = 1,
    EmailTaken = 2,
}
