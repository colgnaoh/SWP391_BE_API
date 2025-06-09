using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugPreventionSystemBE.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoriesTableExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20250607071019_InitialCreate', '9.0.5')");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
