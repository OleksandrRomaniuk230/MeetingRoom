using MeetingRoom.Api.Models;

namespace MeetingRoom.Api.Services;

public interface ITimeSlotRepository
{
    Task<IReadOnlyList<TimeSlot>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);

    /// <returns>The slot with its booking state flipped, or <see langword="null"/> if it does not exist.</returns>
    Task<TimeSlot?> ToggleBookedAsync(Guid id, CancellationToken cancellationToken = default);
}
