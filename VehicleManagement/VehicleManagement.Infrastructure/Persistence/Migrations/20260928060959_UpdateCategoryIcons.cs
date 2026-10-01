using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryIcons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "VehicleCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Icon",
                value: "light_icon");

            migrationBuilder.UpdateData(
                table: "VehicleCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Icon",
                value: "medium_icon");

            migrationBuilder.UpdateData(
                table: "VehicleCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Icon",
                value: "heavy_icon");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "VehicleCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Icon",
                value: "two_wheeler");

            migrationBuilder.UpdateData(
                table: "VehicleCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Icon",
                value: "directions_car");

            migrationBuilder.UpdateData(
                table: "VehicleCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Icon",
                value: "local_shipping");
        }
    }
}
