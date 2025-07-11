using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugPreventionSystemBE.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToUserAnswerLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "UserAnswerLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "SurveyId",
                table: "SurveyResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerLogs_UserId",
                table: "UserAnswerLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerLogs_Users_UserId",
                table: "UserAnswerLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerLogs_Users_UserId",
                table: "UserAnswerLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerLogs_UserId",
                table: "UserAnswerLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserAnswerLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "SurveyId",
                table: "SurveyResults",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
