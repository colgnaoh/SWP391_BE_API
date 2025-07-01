using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugPreventionSystemBE.Migrations
{
    /// <inheritdoc />
    public partial class AddPayOSCheckoutUrlToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayOSCheckoutUrl",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayOSCheckoutUrl",
                table: "Payments");
        }
    }
}
