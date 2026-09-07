using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeetingRoom.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetingRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBooked = table.Column<bool>(type: "bit", nullable: false),
                    MeetingRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlots_MeetingRooms_MeetingRoomId",
                        column: x => x.MeetingRoomId,
                        principalTable: "MeetingRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MeetingRooms",
                columns: new[] { "Id", "Capacity", "Name" },
                values: new object[,]
                {
                    { new Guid("b0000000-0000-0000-0000-000000000001"), 12, "Conference Room A" },
                    { new Guid("b0000000-0000-0000-0000-000000000002"), 4, "Focus Room B" },
                    { new Guid("b0000000-0000-0000-0000-000000000003"), 20, "Board Room C" }
                });

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "Id", "EndTime", "IsBooked", "MeetingRoomId", "StartTime" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0001-000000000009"), new DateTime(2024, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000010"), new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000011"), new DateTime(2024, 1, 2, 12, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000012"), new DateTime(2024, 1, 2, 13, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000013"), new DateTime(2024, 1, 2, 14, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 13, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000014"), new DateTime(2024, 1, 2, 15, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000015"), new DateTime(2024, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0001-000000000016"), new DateTime(2024, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000009"), new DateTime(2024, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000010"), new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000011"), new DateTime(2024, 1, 2, 12, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000012"), new DateTime(2024, 1, 2, 13, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000013"), new DateTime(2024, 1, 2, 14, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 13, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000014"), new DateTime(2024, 1, 2, 15, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000015"), new DateTime(2024, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0002-000000000016"), new DateTime(2024, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000009"), new DateTime(2024, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000010"), new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000011"), new DateTime(2024, 1, 2, 12, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 11, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000012"), new DateTime(2024, 1, 2, 13, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000013"), new DateTime(2024, 1, 2, 14, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 13, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000014"), new DateTime(2024, 1, 2, 15, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000015"), new DateTime(2024, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0003-000000000016"), new DateTime(2024, 1, 2, 17, 0, 0, 0, DateTimeKind.Utc), false, new Guid("b0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 2, 16, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRooms_Name",
                table: "MeetingRooms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_MeetingRoomId_StartTime",
                table: "TimeSlots",
                columns: new[] { "MeetingRoomId", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MeetingRooms");
        }
    }
}
