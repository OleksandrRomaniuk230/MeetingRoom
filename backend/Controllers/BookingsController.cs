using MeetingRoom.Api.Authorization;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using MeetingRoom.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingRoom.Api.Controllers;

/// <summary>Booking creation. Any authenticated user may book a slot for themselves.</summary>
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingRepository _bookings;
    private readonly IHubContext<BookingHub> _hubContext;

    public BookingsController(IBookingRepository bookings, IHubContext<BookingHub> hubContext)
    {
        _bookings = bookings;
        _hubContext = hubContext;
    }

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

    private async Task<IActionResult> HandleSuccessfulBooking(Booking booking, string timeSlotId)
    {
        await _hubContext.Clients.All.SendAsync("SlotStatusChanged", timeSlotId, true);

        return Ok(BookingResponse.From(booking));
    }
}
