using MeetingRoom.Api.Data;
using MeetingRoom.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoom.Api.Services;

public sealed class EfRoomRepository(ApplicationDbContext dbContext) : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.MeetingRooms
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.MeetingRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        dbContext.MeetingRooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<bool> UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.MeetingRooms.FirstOrDefaultAsync(r => r.Id == room.Id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Name = room.Name;
        existing.Capacity = room.Capacity;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.MeetingRooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        // Cascade delete (configured in ApplicationDbContext) removes the room's time slots.
        dbContext.MeetingRooms.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
