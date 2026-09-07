namespace MeetingRoom.Api.Models;

public sealed record TimeSlotResponse(Guid Id, DateTime StartTime, DateTime EndTime, bool IsBooked, Guid MeetingRoomId)
{
    public static TimeSlotResponse From(TimeSlot slot) =>
        new(slot.Id, slot.StartTime, slot.EndTime, slot.IsBooked, slot.MeetingRoomId);
}
