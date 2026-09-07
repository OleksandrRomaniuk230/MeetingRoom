using MeetingRoom.Api.Authorization;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoom.Api.Controllers;

/// <summary>
/// Room management. Reads are open to any authenticated user via the fallback policy;
/// mutations require <see cref="AuthorizeAdminAttribute"/>.
/// </summary>
[ApiController]
[Route("api/rooms")]
[Produces("application/json")]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomRepository _rooms;

    public RoomsController(IRoomRepository rooms)
    {
        _rooms = rooms;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoomResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var rooms = await _rooms.GetAllAsync(cancellationToken);
        return Ok(rooms.Select(RoomResponse.From));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var room = await _rooms.GetByIdAsync(id, cancellationToken);
        return room is null ? NotFound() : Ok(RoomResponse.From(room));
    }

    [AuthorizeAdmin]
    [HttpPost]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(RoomRequest request, CancellationToken cancellationToken)
    {
        var room = new Room { Name = request.Name, Capacity = request.Capacity };
        var created = await _rooms.AddAsync(room, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, RoomResponse.From(created));
    }

    [AuthorizeAdmin]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, RoomRequest request, CancellationToken cancellationToken)
    {
        var room = new Room { Id = id, Name = request.Name, Capacity = request.Capacity };
        var updated = await _rooms.UpdateAsync(room, cancellationToken);
        return updated ? Ok(RoomResponse.From(room)) : NotFound();
    }

    [AuthorizeAdmin]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _rooms.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
