using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugPreventionSystemBE.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseIdToOrderLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "OrderLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_CourseId",
                table: "OrderLogs",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLogs_Courses_CourseId",
                table: "OrderLogs",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLogs_Courses_CourseId",
                table: "OrderLogs");

            migrationBuilder.DropIndex(
                name: "IX_OrderLogs_CourseId",
                table: "OrderLogs");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "OrderLogs");
        }
    }
}
