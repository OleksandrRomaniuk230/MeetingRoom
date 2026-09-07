using System.ComponentModel.DataAnnotations;

namespace MeetingRoom.Api.Models;

public sealed record CreateBookingRequest
{
    [Required]
    public required Guid TimeSlotId { get; init; }
}

public sealed record BookingResponse(Guid Id, Guid UserId, Guid TimeSlotId, DateTime BookedAtUtc)
{
    public static BookingResponse From(Booking booking) =>
        new(booking.Id, booking.UserId, booking.TimeSlotId, booking.BookedAtUtc);
}
