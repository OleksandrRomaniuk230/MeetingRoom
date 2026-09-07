using System.ComponentModel.DataAnnotations;

namespace MeetingRoom.Api.Models;

/// <summary>A fixed, hourly bookable window within a single <see cref="Room"/>.</summary>
public sealed class TimeSlot
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public bool IsBooked { get; set; }

    public required Guid MeetingRoomId { get; init; }

    public Room? MeetingRoom { get; init; }

    public Booking? Booking { get; init; }

    /// <summary>
    /// EF Core concurrency token. Two concurrent booking attempts on the same slot cause
    /// the second <c>SaveChangesAsync</c> to throw <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>
    /// instead of silently overwriting each other.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; init; }
}
