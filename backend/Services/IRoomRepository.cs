using MeetingRoom.Api.Models;

namespace MeetingRoom.Api.Services;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default);

    /// <returns><see langword="false"/> if no room with <paramref name="room"/>'s id exists.</returns>
    Task<bool> UpdateAsync(Room room, CancellationToken cancellationToken = default);

    /// <returns><see langword="false"/> if no room with <paramref name="id"/> exists.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
