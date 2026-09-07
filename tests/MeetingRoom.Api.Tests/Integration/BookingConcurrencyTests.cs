using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeetingRoom.Api.Data;
using MeetingRoom.Api.Models;
using MeetingRoom.Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingRoom.Api.Tests.Integration;

/// <summary>
/// Exercises the optimistic-concurrency path in <c>POST /api/bookings</c>: many callers
/// racing for the same slot must produce exactly one winner, guarded by
/// <see cref="TimeSlot.RowVersion"/> rather than by accident of request ordering.
/// </summary>
public sealed class BookingConcurrencyTests : IAsyncLifetime
{
    private const int ConcurrentRequestCount = 10;

    private readonly TestWebApplicationFactory _factory = new();

    public Task InitializeAsync() => _factory.InitializeDatabaseAsync();

    public async Task DisposeAsync()
    {
        await _factory.DisposeDatabaseAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Fire_10_Simultaneous_Bookings_Should_Succeed_Once_And_Conflict_Nine_Times()
    {
        var timeSlotId = await SeedAvailableTimeSlotAsync();
        var accessToken = await RegisterUserAndGetAccessTokenAsync();

        var clients = Enumerable.Range(0, ConcurrentRequestCount)
            .Select(_ => CreateAuthenticatedClient(accessToken))
            .ToArray();

        try
        {
            var bookingRequest = new CreateBookingRequest { TimeSlotId = timeSlotId };

            // Task.WhenAll starts every request before awaiting any of them, so these ten
            // genuinely overlap instead of running one after another.
            var responses = await Task.WhenAll(
                clients.Select(client => client.PostAsJsonAsync("/api/bookings", bookingRequest)));

            // Captured for the next pass, which will assert the 1-success/9-conflict split.
            Assert.Equal(ConcurrentRequestCount, responses.Length);
        }
        finally
        {
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }
    }

    private async Task<Guid> SeedAvailableTimeSlotAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var room = new Room { Name = $"Concurrency Test Room {Guid.NewGuid():N}", Capacity = 4 };
        var slot = new TimeSlot
        {
            StartTime = new DateTime(2030, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            MeetingRoomId = room.Id,
        };

        db.MeetingRooms.Add(room);
        db.TimeSlots.Add(slot);
        await db.SaveChangesAsync();

        return slot.Id;
    }

    private async Task<string> RegisterUserAndGetAccessTokenAsync()
    {
        using var client = _factory.CreateClient();

        var registerRequest = new RegisterRequest
        {
            Username = $"racer{Guid.NewGuid():N}"[..20],
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "password123!",
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    private HttpClient CreateAuthenticatedClient(string accessToken)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
