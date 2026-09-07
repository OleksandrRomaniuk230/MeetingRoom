using MeetingRoom.Api.Authorization;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoom.Api.Controllers;

/// <summary>Booking creation. Any authenticated user may book a slot for themselves.</summary>
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingRepository _bookings;

    public BookingsController(IBookingRepository bookings)
    {
        _bookings = bookings;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _bookings.CreateAsync(request.TimeSlotId, userId, cancellationToken);

        return result.Status switch
        {
            BookingCreationStatus.Created => Created(
                $"/api/bookings/{result.Booking!.Id}",
                BookingResponse.From(result.Booking)),

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
}
