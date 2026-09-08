using MeetingRoom.Api.Authorization;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using MeetingRoom.Api.Hubs;
using MeetingRoom.Api.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingRoom.Api.Controllers;

/// <summary>
/// Handles concurrent meeting room booking processing and state mutations.
/// Enforces business validation rules and coordinates live SignalR workspace broadcast updates.
/// </summary>
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingRepository _bookings;
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly ApplicationDbContext _context;

    public BookingsController(IBookingRepository bookings, IHubContext<BookingHub> hubContext, ApplicationDbContext context)
    {
        _bookings = bookings;
        _hubContext = hubContext;
        _context = context;
    }

    /// <summary>
    /// Establishes an atomic transaction scope to capture a specific time slot allocation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        Guid userId = User.GetUserId();

        var result = await _bookings.CreateAsync(request.TimeSlotId, userId, cancellationToken);

        return result.Status switch
        {
            BookingCreationStatus.Created => await HandleSuccessfulBooking(result.Booking!, request.TimeSlotId.ToString()),

            BookingCreationStatus.SlotNotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Time slot not found",
                Detail = $"No time slot with id '{request.TimeSlotId}' exists.",
            }),

            BookingCreationStatus.SlotAlreadyBooked => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Slot already booked",
                Detail = "This time slot has already been booked. Choose a different slot.",
            }),

            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Unmapped booking creation status."),
        };
    }

    /// <summary>
    /// Terminates a reservation record context. 
    /// Enforces rigorous access rules: Admins can drop any payload; Regular users can solely cancel their own.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        Guid currentUserId = User.GetUserId();
        
        // Flexible non-case-sensitive role identification mapping across authorization topologies [3.2]
        bool isAdmin = User.IsInRole("Admin") || 
                       User.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "admin") || 
                       User.HasClaim(c => c.Type == "roles" && c.Value == "admin");

        // INTELLIGENT ROUTING FALLBACK: First look up via explicit BookingId, fallback to TimeSlotId if client provides slot identifier
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (booking == null)
        {
            booking = await _context.Bookings.FirstOrDefaultAsync(b => b.TimeSlotId == id, cancellationToken);
        }
        
        if (booking == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Booking record missing",
                Detail = $"No allocation matching tracking reference '{id}' was located in the central ledger."
            });
        }

        // Enforce explicit security isolation policy rules
        if (!isAdmin && booking.UserId != currentUserId)
        {
            return Forbid();
        }

        // Release the bound slot allocation back to the available pool
        var timeSlot = await _context.TimeSlots.FindAsync(new object[] { booking.TimeSlotId }, cancellationToken);
        if (timeSlot != null)
        {
            timeSlot.IsBooked = false;
        }

        // Purge the structural entity relation context tracking item
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify client clusters over active SignalR engine streams that the target block is now free (false) [7.1, 7.3]
        await _hubContext.Clients.All.SendAsync("SlotStatusChanged", booking.TimeSlotId.ToString(), false);

        return NoContent();
    }

    private async Task<IActionResult> HandleSuccessfulBooking(Booking booking, string timeSlotId)
    {
        // Broadcast state transitions over SignalR conduits to update matrices dynamically (true — isBooked) [7.1]
        await _hubContext.Clients.All.SendAsync("SlotStatusChanged", timeSlotId, true);

        return Ok(BookingResponse.From(booking));
    }
}
