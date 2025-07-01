using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugPreventionSystemBE.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Consultant_FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_ConsultantId",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_consultants_ConsultantId",
                table: "Appointments",
                column: "ConsultantId",
                principalTable: "consultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_consultants_ConsultantId",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_ConsultantId",
                table: "Appointments",
                column: "ConsultantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
