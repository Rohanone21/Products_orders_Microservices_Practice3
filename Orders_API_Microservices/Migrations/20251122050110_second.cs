using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders_API_Microservices.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 1,
                column: "OrderDate",
                value: new DateTime(2025, 11, 22, 10, 31, 10, 239, DateTimeKind.Local).AddTicks(6599));

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 2,
                column: "OrderDate",
                value: new DateTime(2025, 11, 22, 10, 31, 10, 242, DateTimeKind.Local).AddTicks(1892));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 1,
                column: "OrderDate",
                value: new DateTime(2025, 11, 22, 10, 30, 2, 473, DateTimeKind.Local).AddTicks(8301));

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 2,
                column: "OrderDate",
                value: new DateTime(2025, 11, 22, 10, 30, 2, 537, DateTimeKind.Local).AddTicks(2731));
        }
    }
}
