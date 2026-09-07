using System.ComponentModel.DataAnnotations;

namespace MeetingRoom.Api.Models;

public sealed record RoomRequest
{
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public required string Name { get; init; }

    [Range(1, 1000)]
    public required int Capacity { get; init; }
}

public sealed record RoomResponse(Guid Id, string Name, int Capacity)
{
    public static RoomResponse From(Room room) => new(room.Id, room.Name, room.Capacity);
}
