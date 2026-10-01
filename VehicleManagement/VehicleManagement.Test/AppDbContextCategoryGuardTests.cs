using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.ValueObjects;

namespace VehicleManagement.Test
{
    // AppDbContext rejects any save that would leave two categories clashing,
    // even when code bypasses VehicleCategoryService.
    // Seeded (max inclusive): Light [0, 499.99], Medium [500, 2499.99], Heavy [2500, ∞).
    [TestClass]
    public class AppDbContextCategoryGuardTests
    {
        private TestDatabase _database = null!;

        [TestInitialize]
        public void Setup() => _database = new TestDatabase();

        [TestCleanup]
        public void Cleanup() => _database.Dispose();

        [TestMethod]
        public async Task AddingAClashingCategoryDirectly_IsRejected_AndNothingIsSaved()
        {
            using (var context = _database.CreateContext())
            {
                context.VehicleCategories.Add(
                    new VehicleCategory("Very Heavy", new WeightRange(5000m, null), "very_heavy_icon"));

                var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(
                    () => context.SaveChangesAsync());
                StringAssert.Contains(ex.Message, "clash");
            }

            using var check = _database.CreateContext();
            Assert.AreEqual(3, await check.VehicleCategories.CountAsync());
        }

        [TestMethod]
        public async Task EditingOneCategoryIntoAnother_IsRejected_EvenWithOthersUntracked()
        {
            using var context = _database.CreateContext();
            var medium = await context.VehicleCategories.SingleAsync(x => x.Name == "Medium");
            medium.Update("Medium", new WeightRange(499.99m, 2499.99m), "medium_icon"); // shares 499.99 with Light

            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(
                () => context.SaveChangesAsync());
            StringAssert.Contains(ex.Message, "'Light'");
        }

        [TestMethod]
        public void SyncSaveChanges_IsGuardedToo()
        {
            using var context = _database.CreateContext();
            var heavy = context.VehicleCategories.Single(x => x.Name == "Heavy");
            heavy.Update("Heavy", new WeightRange(2000m, null), "heavy_icon");

            Assert.ThrowsException<InvalidCategoryConfigurationException>(() => context.SaveChanges());
        }

        [TestMethod]
        public async Task LeavingAGap_IsAllowed()
        {
            using (var context = _database.CreateContext())
            {
                var medium = await context.VehicleCategories.SingleAsync(x => x.Name == "Medium");
                context.VehicleCategories.Remove(medium);
                await context.SaveChangesAsync();
            }

            using var check = _database.CreateContext();
            Assert.AreEqual(2, await check.VehicleCategories.CountAsync());
        }

        [TestMethod]
        public async Task SavesThatDoNotTouchCategories_AreNotAffected()
        {
            using var context = _database.CreateContext();
            context.Vehicles.Add(new Vehicle("Jane Doe", 1, 2020, 1000m));

            Assert.AreEqual(1, await context.SaveChangesAsync());
        }
    }
}
