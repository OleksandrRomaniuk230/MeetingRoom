using MeetingRoom.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoom.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Fixed so the generated migration's seed data stays byte-for-byte stable across
    // `dotnet ef migrations add` runs; a wall-clock date would make the model look
    // "changed" on every regeneration.
    private static readonly DateOnly SeedBusinessDay = new(2024, 1, 2);

    private static readonly Guid ConferenceRoomId = Guid.Parse("b0000000-0000-0000-0000-000000000001");
    private static readonly Guid FocusRoomId = Guid.Parse("b0000000-0000-0000-0000-000000000002");
    private static readonly Guid BoardRoomId = Guid.Parse("b0000000-0000-0000-0000-000000000003");

    public DbSet<User> Users => Set<User>();

    public DbSet<Room> MeetingRooms => Set<Room>();

    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Email).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("MeetingRooms");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired();
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Capacity).IsRequired();

            entity.HasMany(r => r.TimeSlots)
                .WithOne(t => t.MeetingRoom)
                .HasForeignKey(t => t.MeetingRoomId)
                // Deleting a room deletes its slots with it - a slot cannot outlive its room.
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(
                new { Id = ConferenceRoomId, Name = "Conference Room A", Capacity = 12 },
                new { Id = FocusRoomId, Name = "Focus Room B", Capacity = 4 },
                new { Id = BoardRoomId, Name = "Board Room C", Capacity = 20 });
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.ToTable("TimeSlots");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.StartTime).IsRequired();
            entity.Property(t => t.EndTime).IsRequired();
            entity.Property(t => t.IsBooked).IsRequired();

            // A room can only have one slot starting at a given time.
            entity.HasIndex(t => new { t.MeetingRoomId, t.StartTime }).IsUnique();

            entity.HasData(SeedTimeSlots());
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BookedAtUtc).IsRequired();

            // A slot can have at most one booking - modeled as a true one-to-one, not just a
            // unique index, so EF enforces it in both directions of the navigation.
            entity.HasIndex(b => b.TimeSlotId).IsUnique();

            entity.HasOne(b => b.TimeSlot)
                .WithOne(t => t.Booking)
                .HasForeignKey<Booking>(b => b.TimeSlotId)
                // A booking is evidence of a reservation; deleting its slot must not silently
                // erase that history. This also means deleting a Room cascades to its
                // TimeSlots (see above) only for slots with no booking - a room with an
                // actively booked slot cannot be deleted until the booking is removed.
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                // Deleting a user takes their bookings with them.
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>Hourly 09:00-17:00 UTC slots for each seeded room on a fixed business day.</summary>
    private static IEnumerable<object> SeedTimeSlots()
    {
        const int businessDayStartHour = 9;
        const int businessDayEndHour = 17;

        (int RoomIndex, Guid RoomId)[] rooms =
        [
            (1, ConferenceRoomId),
            (2, FocusRoomId),
            (3, BoardRoomId),
        ];

        foreach (var (roomIndex, roomId) in rooms)
        {
            for (var hour = businessDayStartHour; hour < businessDayEndHour; hour++)
            {
                yield return new
                {
                    Id = SlotId(roomIndex, hour),
                    StartTime = SeedBusinessDay.ToDateTime(new TimeOnly(hour, 0), DateTimeKind.Utc),
                    EndTime = SeedBusinessDay.ToDateTime(new TimeOnly(hour + 1, 0), DateTimeKind.Utc),
                    IsBooked = false,
                    MeetingRoomId = roomId,
                };
            }
        }
    }

    private static Guid SlotId(int roomIndex, int hour) =>
        Guid.Parse($"a0000000-0000-0000-000{roomIndex}-{hour:D12}");
}
