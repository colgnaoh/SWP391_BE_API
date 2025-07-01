using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugPreventionSystemBE.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneAndAddressToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Appointments");
        }
    }
}
