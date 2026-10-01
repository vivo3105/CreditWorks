using Microsoft.VisualStudio.TestTools.UnitTesting;
using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Application.Services;
using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.Services;
using VehicleManagement.Infrastructure.Repositories;

namespace VehicleManagement.Test
{
    // Seeded categories (max inclusive): 1 Light [0, 499.99], 2 Medium [500, 2499.99], 3 Heavy [2500, ∞).
    [TestClass]
    public class VehicleCategoryServiceTests
    {
        private TestDatabase _database = null!;

        [TestInitialize]
        public void Setup() => _database = new TestDatabase();

        [TestCleanup]
        public void Cleanup() => _database.Dispose();

        private VehicleCategoryService CreateService()
        {
            var context = _database.CreateContext();

            return new VehicleCategoryService(
                new VehicleCategoryRepository(context),
                new CategoryConfigurationValidator(),
                new FakeIconProvider(),
                context);
        }

        // Every icon name used by these tests counts as configured, except "unknown_icon".
        private sealed class FakeIconProvider : ICategoryIconProvider
        {
            public IReadOnlyList<CategoryIconDto> GetAll() => Array.Empty<CategoryIconDto>();

            public bool Exists(string name) => name != "unknown_icon";
        }

        [TestMethod]
        public async Task CreateAsync_RejectsIconNotInTheConfiguredList()
        {
            await CreateService().DeleteAsync(3, CancellationToken.None); // free 2500+

            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleCategoryRequest("Heavy", 2500m, null, "unknown_icon"),
                    CancellationToken.None));

            StringAssert.Contains(ex.Message, "not one of the configured category icons");
        }

        [TestMethod]
        public async Task UpdateAsync_RejectsIconNotInTheConfiguredList_AndSavesNothing()
        {
            await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().UpdateAsync(
                    1, new SaveVehicleCategoryRequest("Light", 0m, 499.99m, "unknown_icon"), CancellationToken.None));

            var light = await CreateService().GetByIdAsync(1, CancellationToken.None);
            Assert.AreEqual("light_icon", light!.Icon);
        }

        // Reads back through a fresh context to prove what was persisted.
        private async Task<string[]> SavedRanges()
        {
            var saved = await CreateService().GetAllAsync(CancellationToken.None);
            return saved
                .Select(x => x.MaxWeight is null ? $"{x.Name} [{x.MinWeight:0.##}, ∞)" : $"{x.Name} [{x.MinWeight:0.##}, {x.MaxWeight:0.##}]")
                .ToArray();
        }

        private static readonly string[] Seeded =
        {
            "Light [0, 499.99]", "Medium [500, 2499.99]", "Heavy [2500, ∞)",
        };

        // ---- Create -------------------------------------------------------

        [TestMethod]
        public async Task CreateAsync_FailsOnClash_AndSavesNothing()
        {
            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleCategoryRequest("Very Heavy", 5000m, null, "very_heavy_icon"),
                    CancellationToken.None));

            StringAssert.Contains(ex.Message, "clashes with category 'Heavy'");
            CollectionAssert.AreEqual(Seeded, await SavedRanges());
        }

        [TestMethod]
        public async Task CreateAsync_IntoAGap_Succeeds_WithoutChangingOthers()
        {
            // Make room first: Heavy now stops at 4999.99, leaving 5000+ uncovered.
            await CreateService().UpdateAsync(
                3, new SaveVehicleCategoryRequest("Heavy", 2500m, 4999.99m, "heavy_icon"), CancellationToken.None);

            var created = await CreateService().CreateAsync(
                new SaveVehicleCategoryRequest("Very Heavy", 5000m, null, "very_heavy_icon"),
                CancellationToken.None);

            Assert.IsTrue(created.Id > 0);
            CollectionAssert.AreEqual(
                new[] { "Light [0, 499.99]", "Medium [500, 2499.99]", "Heavy [2500, 4999.99]", "Very Heavy [5000, ∞)" },
                await SavedRanges());
        }

        [TestMethod]
        public async Task CreateAsync_RejectsMinEqualToAnotherCategorysMax()
        {
            await CreateService().DeleteAsync(2, CancellationToken.None); // gap: 500 – 2499.99

            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleCategoryRequest("Medium", 499.99m, 2499.99m, "medium_icon"),
                    CancellationToken.None));

            StringAssert.Contains(ex.Message, "clashes with category 'Light' (0 – 499.99 kg)");
        }

        [TestMethod]
        public async Task CreateAsync_RejectsMaxEqualToAnotherCategorysMin()
        {
            await CreateService().DeleteAsync(2, CancellationToken.None);

            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleCategoryRequest("Medium", 500m, 2500m, "medium_icon"),
                    CancellationToken.None));

            StringAssert.Contains(ex.Message, "clashes with category 'Heavy'");
        }

        [TestMethod]
        public async Task UpdateAsync_RejectsMinEqualToAnotherCategorysMax()
        {
            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().UpdateAsync(
                    2, new SaveVehicleCategoryRequest("Medium", 499.99m, 2499.99m, "medium_icon"), CancellationToken.None));

            StringAssert.Contains(ex.Message, "clashes with category 'Light'");
            CollectionAssert.AreEqual(Seeded, await SavedRanges());
        }

        [TestMethod]
        public async Task CreateAsync_RejectsDuplicateName_IgnoringCase()
        {
            await CreateService().DeleteAsync(3, CancellationToken.None); // free a range

            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleCategoryRequest(" light ", 2500m, null, "x"),
                    CancellationToken.None));

            StringAssert.Contains(ex.Message, "already used");
        }

        [TestMethod]
        public async Task CreateAsync_RejectsMaxNotAboveMin()
        {
            await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleCategoryRequest("X", 100m, 100m, "x"),
                    CancellationToken.None));
        }

        // ---- Update -------------------------------------------------------

        [TestMethod]
        public async Task UpdateAsync_FailsOnClash_AndUpdatesNothing()
        {
            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().UpdateAsync(
                    2, new SaveVehicleCategoryRequest("Medium Renamed", 400m, 2499.99m, "m"), CancellationToken.None));

            StringAssert.Contains(ex.Message, "clashes with category 'Light'");
            CollectionAssert.AreEqual(Seeded, await SavedRanges()); // name not changed either
        }

        [TestMethod]
        public async Task UpdateAsync_DoesNotClashWithItsOwnOldRange()
        {
            var updated = await CreateService().UpdateAsync(
                2, new SaveVehicleCategoryRequest("Medium", 600m, 2400m, "medium_icon"), CancellationToken.None);

            Assert.IsNotNull(updated);
            CollectionAssert.AreEqual(
                new[] { "Light [0, 499.99]", "Medium [600, 2400]", "Heavy [2500, ∞)" },
                await SavedRanges());
        }

        [TestMethod]
        public async Task UpdateAsync_NameAndIconOnly_Succeeds()
        {
            var updated = await CreateService().UpdateAsync(
                1, new SaveVehicleCategoryRequest("Featherweight", 0m, 499.99m, "feather_icon"), CancellationToken.None);

            Assert.AreEqual("Featherweight", updated!.Name);
            Assert.AreEqual("feather_icon", updated.Icon);
        }

        [TestMethod]
        public async Task UpdateAsync_RejectsTakingAnotherCategorysName()
        {
            var ex = await Assert.ThrowsExceptionAsync<InvalidCategoryConfigurationException>(() =>
                CreateService().UpdateAsync(2, new SaveVehicleCategoryRequest("LIGHT", 500m, 2499.99m, "m"), CancellationToken.None));

            StringAssert.Contains(ex.Message, "already used");
        }

        [TestMethod]
        public async Task UpdateAsync_ReturnsNull_WhenNotFound()
        {
            Assert.IsNull(await CreateService().UpdateAsync(
                99, new SaveVehicleCategoryRequest("X", 0m, null, "x"), CancellationToken.None));
        }

        // ---- Delete -------------------------------------------------------

        [TestMethod]
        public async Task DeleteAsync_RemovesOnlyThatCategory_LeavingAGap()
        {
            Assert.IsTrue(await CreateService().DeleteAsync(2, CancellationToken.None));

            CollectionAssert.AreEqual(new[] { "Light [0, 499.99]", "Heavy [2500, ∞)" }, await SavedRanges());
        }

        [TestMethod]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            Assert.IsFalse(await CreateService().DeleteAsync(99, CancellationToken.None));
        }
    }
}
