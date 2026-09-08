using System;

namespace MeetingRoom.Api.Models;

public sealed record TimeSlotResponse(
    Guid Id, 
    DateTime StartTime, 
    DateTime EndTime, 
    bool IsBooked, 
    Guid MeetingRoomId,
    string? BookedByUserId = null,   
    string? BookedByUsername = null  
)
{
    public static TimeSlotResponse From(TimeSlot slot) =>
        new(
            slot.Id, 
            slot.StartTime, 
            slot.EndTime, 
            slot.IsBooked, 
            slot.MeetingRoomId,
            slot.Booking != null ? slot.Booking.UserId.ToString() : null,
            slot.Booking != null && slot.Booking.User != null ? slot.Booking.User.Username : null
        );
}
