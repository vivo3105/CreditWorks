using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleManagement.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// VehicleCategory.MaxWeight changes from exclusive ("below") to inclusive
    /// ("up to and including"), and a category's minimum may no longer equal
    /// another's maximum.
    ///
    /// Only maximums that equal another category's minimum (a shared limit, now
    /// a clash) are moved down by 0.01. Weights have 2 decimal places, so an
    /// exclusive max of 500 next to a category starting at 500 becomes an
    /// inclusive 499.99: every vehicle keeps its category. Rows already in the
    /// new form (e.g. 499.99 next to 500) are left alone, so this is safe to run
    /// on data that was adjusted by hand.
    /// </summary>
    public partial class MakeCategoryMaxWeightInclusive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE c
                SET c.MaxWeight = c.MaxWeight - 0.01
                FROM VehicleCategories c
                WHERE c.MaxWeight IS NOT NULL
                  AND EXISTS (SELECT 1 FROM VehicleCategories o
                              WHERE o.Id <> c.Id AND o.MinWeight = c.MaxWeight);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: neighbours one cent apart go back to sharing the limit.
            migrationBuilder.Sql(
                """
                UPDATE c
                SET c.MaxWeight = c.MaxWeight + 0.01
                FROM VehicleCategories c
                WHERE c.MaxWeight IS NOT NULL
                  AND EXISTS (SELECT 1 FROM VehicleCategories o
                              WHERE o.Id <> c.Id AND o.MinWeight = c.MaxWeight + 0.01);
                """);
        }
    }
}
