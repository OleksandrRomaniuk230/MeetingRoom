using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingRoom.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeSlotRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TimeSlots",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TimeSlots");
        }
    }
}
