using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugPreventionSystemBE.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsDeletedFromAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "ProgramVidUrl",
                table: "Programs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskLevel",
                table: "Programs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskLevel",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgramVidUrl",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Courses");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
