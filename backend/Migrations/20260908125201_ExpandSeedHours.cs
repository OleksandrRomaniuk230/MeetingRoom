using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetingRoom.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExpandSeedHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MeetingRooms",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000001"),
                column: "Name",
                value: "Room A");

            migrationBuilder.UpdateData(
                table: "MeetingRooms",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000002"),
                column: "Name",
                value: "Room B");

            migrationBuilder.UpdateData(
                table: "MeetingRooms",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000003"),
                column: "Name",
                value: "Room C");

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "Id", "EndTime", "IsBooked", "MeetingRoomId", "StartTime" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0001-000000000007"), new DateTime(2024, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000008"), new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000017"), new DateTime(2024, 1, 2, 18, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000007"), new DateTime(2024, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000008"), new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000017"), new DateTime(2024, 1, 2, 18, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000007"), new DateTime(2024, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 7, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000008"), new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000017"), new DateTime(2024, 1, 2, 18, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0001-000000000007"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0001-000000000008"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0001-000000000017"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0002-000000000007"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0002-000000000008"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0002-000000000017"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0003-000000000007"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0003-000000000008"));

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0003-000000000017"));

            migrationBuilder.UpdateData(
                table: "MeetingRooms",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000001"),
                column: "Name",
                value: "Conference Room A");

            migrationBuilder.UpdateData(
                table: "MeetingRooms",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000002"),
                column: "Name",
                value: "Focus Room B");

            migrationBuilder.UpdateData(
                table: "MeetingRooms",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000003"),
                column: "Name",
                value: "Board Room C");
        }
    }
}
