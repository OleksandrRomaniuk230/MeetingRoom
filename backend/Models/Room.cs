namespace MeetingRoom.Api.Models;

/// <summary>A bookable physical room.</summary>
public sealed class Room
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Name { get; set; }

    public required int Capacity { get; set; }

    public ICollection<TimeSlot> TimeSlots { get; init; } = [];
}
