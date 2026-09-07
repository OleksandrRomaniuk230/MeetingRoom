using MeetingRoom.Api.Data;
using MeetingRoom.Api.Models;
using Microsoft.Data.SqlClient;
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
        catch (DbUpdateException ex) when (ex is DbUpdateConcurrencyException || IsUniqueBookingViolation(ex))
        {
            // Two distinct races land here:
            //  - DbUpdateConcurrencyException: TimeSlot.RowVersion caught a concurrent
            //    change to the slot between our read and our write.
            //  - A plain DbUpdateException wrapping a unique-key violation on
            //    IX_Bookings_TimeSlotId: with enough simultaneous callers, more than one
            //    can read the slot as unbooked before any of them commits, so the
            //    RowVersion check on the *slot* update doesn't fire for all of the losers -
            //    the Bookings table's own unique index is the backstop that always does.
            // Either way, this tracker is now in an indeterminate state, so clear it before
            // reporting.
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

    private static bool IsUniqueBookingViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 } sqlEx &&
        sqlEx.Message.Contains("IX_Bookings_TimeSlotId", StringComparison.Ordinal);
}
