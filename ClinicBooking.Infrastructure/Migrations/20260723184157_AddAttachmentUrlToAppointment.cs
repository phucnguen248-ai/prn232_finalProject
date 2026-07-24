using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentUrlToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Appointments");
        }
    }
}
