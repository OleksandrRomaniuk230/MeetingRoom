using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoom.Api.Controllers;

/// <summary>
/// Time-slot listing and booking toggling. Booking a slot is an ordinary user action, not
/// room management, so both endpoints are open to any authenticated user via the fallback
/// policy - there is no <c>[AuthorizeAdmin]</c> here.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class SlotsController : ControllerBase
{
    private readonly IRoomRepository _rooms;
    private readonly ITimeSlotRepository _slots;

    public SlotsController(IRoomRepository rooms, ITimeSlotRepository slots)
    {
        _rooms = rooms;
        _slots = slots;
    }

    [HttpGet("rooms/{roomId:guid}/slots")]
    [ProducesResponseType(typeof(IEnumerable<TimeSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRoom(Guid roomId, CancellationToken cancellationToken)
    {
        var room = await _rooms.GetByIdAsync(roomId, cancellationToken);
        if (room is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Room not found",
                Detail = $"No room with id '{roomId}' exists.",
            });
        }

        var slots = await _slots.GetByRoomIdAsync(roomId, cancellationToken);
        return Ok(slots.Select(TimeSlotResponse.From));
    }

    [HttpPost("slots/{id:guid}/toggle")]
    [ProducesResponseType(typeof(TimeSlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleBooked(Guid id, CancellationToken cancellationToken)
    {
        var slot = await _slots.ToggleBookedAsync(id, cancellationToken);
        if (slot is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Time slot not found",
                Detail = $"No time slot with id '{id}' exists.",
            });
        }

        return Ok(TimeSlotResponse.From(slot));
    }
}
