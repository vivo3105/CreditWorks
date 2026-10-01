using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_sample_vehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "ManufacturerId", "OwnerName", "Weight", "YearOfManufacture" },
                values: new object[] { 1, 1, "Vi Vo", 1800m, 2025 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
