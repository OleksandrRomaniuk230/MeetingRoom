using MeetingRoom.Api.Authorization;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using MeetingRoom.Api.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    private readonly ApplicationDbContext _context; 

    public RoomsController(IRoomRepository rooms, ApplicationDbContext context)
    {
        _rooms = rooms;
        _context = context;
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

    [HttpGet("{id:guid}/slots")]
    [ProducesResponseType(typeof(IEnumerable<TimeSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlotsByRoomId(Guid id, CancellationToken cancellationToken)
    {
        var roomExists = await _context.MeetingRooms.AnyAsync(r => r.Id == id, cancellationToken);
        if (!roomExists) return NotFound();

        var slots = await _context.TimeSlots
            .Include(t => t.Booking)
                .ThenInclude(b => b.User)
            .Where(t => t.MeetingRoomId == id)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);

        var response = slots.Select(s => new TimeSlotResponse(
            s.Id,
            s.StartTime,
            s.EndTime,
            s.IsBooked,
            s.MeetingRoomId,
            s.Booking != null ? s.Booking.UserId.ToString() : null,
            s.Booking != null && s.Booking.User != null ? s.Booking.User.Username : null
        ));

        return Ok(response);
    }

    [AuthorizeAdmin]
    [HttpPost]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(RoomRequest request, CancellationToken cancellationToken)
    {
        var room = new Room { Name = request.Name, Capacity = request.Capacity };
        var created = await _rooms.AddAsync(room, cancellationToken);

        var baseDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var newSlots = new List<TimeSlot>();

        for (int hour = 7; hour <= 17; hour++)
        {
            newSlots.Add(new TimeSlot
            {
                Id = Guid.NewGuid(), 
                MeetingRoomId = created.Id, 
                StartTime = baseDate.AddHours(hour),
                EndTime = baseDate.AddHours(hour + 1),
                IsBooked = false
            });
        }

        await _context.TimeSlots.AddRangeAsync(newSlots, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

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
