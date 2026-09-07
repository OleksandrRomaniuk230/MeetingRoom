using MeetingRoom.Api.Data;
using MeetingRoom.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoom.Api.Services;

public sealed class EfBookingRepository(ApplicationDbContext dbContext) : IBookingRepository
{
    public async Task<BookingCreationResult> CreateAsync(Guid timeSlotId, Guid userId, CancellationToken cancellationToken = default)
    {
        var slot = await dbContext.TimeSlots.FirstOrDefaultAsync(t => t.Id == timeSlotId, cancellationToken);
        if (slot is null)
        {
            return new BookingCreationResult(BookingCreationStatus.SlotNotFound);
        }

        if (slot.IsBooked)
        {
            return new BookingCreationResult(BookingCreationStatus.SlotAlreadyBooked);
        }

        slot.IsBooked = true;

        var booking = new Booking { UserId = userId, TimeSlotId = timeSlotId };
        dbContext.Bookings.Add(booking);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new BookingCreationResult(BookingCreationStatus.Created, booking);
        }
        catch (DbUpdateConcurrencyException)
        {
            // TimeSlot.RowVersion caught a race: another request committed a change to this
            // same slot between our read and our write. This tracker is now in an
            // indeterminate state for both entries, so clear it before reporting.
            dbContext.ChangeTracker.Clear();

            // Confirm the slot really was claimed rather than assuming - it could also have
            // been deleted (via cascade from its room) in the same window.
            var stillExists = await dbContext.TimeSlots
                .AsNoTracking()
                .AnyAsync(t => t.Id == timeSlotId, cancellationToken);

            return new BookingCreationResult(stillExists
                ? BookingCreationStatus.SlotAlreadyBooked
                : BookingCreationStatus.SlotNotFound);
        }
    }
}
