using MeetingRoom.Api.Data;
using MeetingRoom.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoom.Api.Services;

public sealed class EfTimeSlotRepository(ApplicationDbContext dbContext) : ITimeSlotRepository
{
    public async Task<IReadOnlyList<TimeSlot>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default) =>
        await dbContext.TimeSlots
            .AsNoTracking()
            .Where(t => t.MeetingRoomId == roomId)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<TimeSlot?> ToggleBookedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var slot = await dbContext.TimeSlots.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (slot is null)
        {
            return null;
        }

        slot.IsBooked = !slot.IsBooked;
        await dbContext.SaveChangesAsync(cancellationToken);
        return slot;
    }
}
