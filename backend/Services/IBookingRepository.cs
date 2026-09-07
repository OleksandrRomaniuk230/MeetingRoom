using MeetingRoom.Api.Models;

namespace MeetingRoom.Api.Services;

public interface IBookingRepository
{
    /// <summary>
    /// Books <paramref name="timeSlotId"/> for <paramref name="userId"/>, atomically: the
    /// slot's <c>IsBooked</c> flag and the new <see cref="Booking"/> row are written in the
    /// same <c>SaveChangesAsync</c> call, guarded by the slot's RowVersion concurrency token.
    /// </summary>
    Task<BookingCreationResult> CreateAsync(Guid timeSlotId, Guid userId, CancellationToken cancellationToken = default);
}

public enum BookingCreationStatus
{
    Created = 0,
    SlotNotFound = 1,
    SlotAlreadyBooked = 2,
}

public sealed record BookingCreationResult(BookingCreationStatus Status, Booking? Booking = null);
