using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehicleManagement.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// REVIEW BEFORE APPLYING TO A DATABASE WITH REAL DATA. Brings the database
    /// in line with the 5-manufacturer seed list in ManufacturerConfiguration
    /// (changed earlier without a migration). Harmless on a brand-new database.
    ///
    /// - Deleting manufacturers 6–17 fails if any vehicle uses one (FK is Restrict).
    /// - Renaming 1–5 relabels every vehicle using them (e.g. Audi becomes Mazda,
    ///   Toyota's old id 15 is deleted while id 5 becomes "Toyota").
    ///
    /// Written by hand (no .Designer.cs model snapshot), so every data operation
    /// states its column types explicitly; without them EF can't resolve the
    /// table and fails with "There is no entity type mapped to the table".
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261001023900_ApplyManufacturerSeedChange")]
    public partial class ApplyManufacturerSeedChange : Migration
    {
        private const string Table = "Manufacturers";
        private const string IdType = "int";
        private const string NameType = "nvarchar(100)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            for (var id = 6; id <= 17; id++)
            {
                migrationBuilder.DeleteData(
                    table: Table, keyColumn: "Id", keyColumnType: IdType, keyValue: id);
            }

            // Order matters for the unique Name index: id 5 is still "Honda", so
            // it becomes "Toyota" (old id 15, deleted above) before id 3 takes "Honda".
            Rename(migrationBuilder, 5, "Toyota");
            Rename(migrationBuilder, 1, "Mazda");
            Rename(migrationBuilder, 2, "Mercedes");
            Rename(migrationBuilder, 3, "Honda");
            Rename(migrationBuilder, 4, "Ferrari");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the original 17 names. Rename 1–5 first: "Honda" (now id 3)
            // must move back to id 5 without breaking the unique Name index.
            Rename(migrationBuilder, 3, "Ford");
            Rename(migrationBuilder, 5, "Honda");
            Rename(migrationBuilder, 1, "Audi");
            Rename(migrationBuilder, 2, "BMW");
            Rename(migrationBuilder, 4, "Holden");

            migrationBuilder.InsertData(
                table: Table,
                columns: new[] { "Id", "Name" },
                columnTypes: new[] { IdType, NameType },
                values: new object[,]
                {
                    { 6, "Hyundai" },
                    { 7, "Kia" },
                    { 8, "Mazda" },
                    { 9, "Mercedes-Benz" },
                    { 10, "Mitsubishi" },
                    { 11, "Nissan" },
                    { 12, "Subaru" },
                    { 13, "Suzuki" },
                    { 14, "Tesla" },
                    { 15, "Toyota" },
                    { 16, "Volkswagen" },
                    { 17, "Volvo" }
                });
        }

        private static void Rename(MigrationBuilder migrationBuilder, int id, string name) =>
            migrationBuilder.UpdateData(
                table: Table,
                keyColumns: new[] { "Id" },
                keyColumnTypes: new[] { IdType },
                keyValues: new object[] { id },
                columns: new[] { "Name" },
                columnTypes: new[] { NameType },
                values: new object[] { name });
    }
}
