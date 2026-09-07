namespace MeetingRoom.Api.Models;

/// <summary>
/// Ties one <see cref="Models.TimeSlot"/> to the <see cref="User"/> who reserved it. A slot
/// can have at most one booking, enforced by a unique index on <see cref="TimeSlotId"/>.
/// </summary>
public sealed class Booking
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required Guid UserId { get; init; }

    public required Guid TimeSlotId { get; init; }

    public DateTime BookedAtUtc { get; init; } = DateTime.UtcNow;

    public User? User { get; init; }

    public TimeSlot? TimeSlot { get; init; }
}
