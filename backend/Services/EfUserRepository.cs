using MeetingRoom.Api.Data;
using MeetingRoom.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoom.Api.Services;

/// <summary>
/// SQL Server-backed <see cref="IUserRepository"/>. Relies on the database's default
/// case-insensitive collation for username lookups, matching <see cref="User.Username"/>'s
/// documented comparison semantics.
/// </summary>
public sealed class EfUserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<UserCreationResult> TryAddAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return UserCreationResult.Created;
        }
        catch (DbUpdateException)
        {
            // The unique indexes on Username/Email are what make this atomic: of two
            // concurrent registrations, only one insert can win. The loser lands here and
            // re-queries to find out which of its two values was already taken.
            dbContext.Entry(user).State = EntityState.Detached;

            var usernameTaken = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Username == user.Username, cancellationToken);

            return usernameTaken ? UserCreationResult.UsernameTaken : UserCreationResult.EmailTaken;
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
