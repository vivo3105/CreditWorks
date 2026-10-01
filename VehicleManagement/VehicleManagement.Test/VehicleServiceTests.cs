using Microsoft.VisualStudio.TestTools.UnitTesting;
using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Services;
using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.Services;
using VehicleManagement.Infrastructure.Repositories;


namespace VehicleManagement.Test
{
    [TestClass]
    public class VehicleServiceTests
    {
        private TestDatabase _database = null!;

        [TestInitialize]
        public void Setup() => _database = new TestDatabase();

        [TestCleanup]
        public void Cleanup() => _database.Dispose();

        private VehicleService CreateService()
        {
            var context = _database.CreateContext();

            return new VehicleService(
                new VehicleRepository(context),
                new ManufacturerRepository(context),
                new VehicleCategoryRepository(context),
                new VehicleCategoryResolver(),
                context);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsSeededVehicle()
        {
            var vehicle = await CreateService().GetByIdAsync(1, CancellationToken.None);

            Assert.IsNotNull(vehicle);
            Assert.AreEqual("Vi Vo", vehicle.OwnerName);
            Assert.AreEqual(1, vehicle.ManufacturerId);
            Assert.AreEqual("Mazda", vehicle.ManufacturerName);
            Assert.AreEqual(2025, vehicle.YearOfManufacture);
            Assert.AreEqual(1800m, vehicle.Weight);
            Assert.AreEqual("Medium", vehicle.CategoryName);
        }

        [TestMethod]
        public async Task Category_MaxWeightIsInclusive()
        {
            // Light [0, 499.99], Medium [500, 2499.99], Heavy [2500, ∞)
            var atLightMax = await CreateService().CreateAsync(new SaveVehicleRequest("A", 1, 2020, 499.99m), CancellationToken.None);
            var atMediumMin = await CreateService().CreateAsync(new SaveVehicleRequest("B", 1, 2020, 500m), CancellationToken.None);
            var atMediumMax = await CreateService().CreateAsync(new SaveVehicleRequest("C", 1, 2020, 2499.99m), CancellationToken.None);
            var atHeavyMin = await CreateService().CreateAsync(new SaveVehicleRequest("D", 1, 2020, 2500m), CancellationToken.None);

            Assert.AreEqual("Light", atLightMax.CategoryName);
            Assert.AreEqual("Medium", atMediumMin.CategoryName);
            Assert.AreEqual("Medium", atMediumMax.CategoryName);
            Assert.AreEqual("Heavy", atHeavyMin.CategoryName);
        }

        [TestMethod]
        public async Task Vehicle_InACategoryGap_IsUncategorised_NotAnError()
        {
            // Seeded vehicle 1 weighs 1800 kg, inside Medium [500, 2500).
            using (var context = _database.CreateContext())
            {
                context.VehicleCategories.Remove(context.VehicleCategories.Single(x => x.Name == "Medium"));
                await context.SaveChangesAsync();
            }

            var vehicle = await CreateService().GetByIdAsync(1, CancellationToken.None);
            var all = await CreateService().GetAllAsync(CancellationToken.None);

            Assert.IsNotNull(vehicle);
            Assert.IsNull(vehicle.CategoryName);
            Assert.IsNull(vehicle.CategoryIcon);
            Assert.AreEqual(1, all.Count);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            Assert.IsNull(await CreateService().GetByIdAsync(999, CancellationToken.None));
        }

        [TestMethod]
        public async Task CreateAsync_SavesVehicle_AndResolvesCategory()
        {
            var created = await CreateService().CreateAsync(
                new SaveVehicleRequest("Jane Doe", 2, 2020, 3000m),
                CancellationToken.None);

            // Read back through a fresh service/context to prove it was persisted.
            var saved = await CreateService().GetByIdAsync(created.Id, CancellationToken.None);

            Assert.IsNotNull(saved);
            Assert.AreEqual("Jane Doe", saved.OwnerName);
            Assert.AreEqual("Mercedes", saved.ManufacturerName);
            Assert.AreEqual("Heavy", saved.CategoryName);
        }

        [TestMethod]
        public async Task CreateAsync_Throws_WhenManufacturerDoesNotExist()
        {
            await Assert.ThrowsExceptionAsync<InvalidVehicleException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleRequest("Jane Doe", 999, 2020, 1000m),
                    CancellationToken.None));
        }

        [TestMethod]
        public async Task CreateAsync_Throws_WhenWeightIsInvalid()
        {
            await Assert.ThrowsExceptionAsync<InvalidVehicleException>(() =>
                CreateService().CreateAsync(
                    new SaveVehicleRequest("Jane Doe", 1, 2020, 0m),
                    CancellationToken.None));
        }

        [TestMethod]
        public async Task UpdateAsync_ChangesVehicle()
        {
            var updated = await CreateService().UpdateAsync(
                1,
                new SaveVehicleRequest("Vi Vo Updated", 5, 2024, 400m),
                CancellationToken.None);

            var saved = await CreateService().GetByIdAsync(1, CancellationToken.None);

            Assert.IsNotNull(updated);
            Assert.IsNotNull(saved);
            Assert.AreEqual("Vi Vo Updated", saved.OwnerName);
            Assert.AreEqual("Toyota", saved.ManufacturerName);
            Assert.AreEqual(2024, saved.YearOfManufacture);
            Assert.AreEqual("Light", saved.CategoryName);
        }

        [TestMethod]
        public async Task UpdateAsync_ReturnsNull_WhenNotFound()
        {
            Assert.IsNull(await CreateService().UpdateAsync(
                999,
                new SaveVehicleRequest("Nobody", 1, 2020, 1000m),
                CancellationToken.None));
        }

        [TestMethod]
        public async Task DeleteAsync_RemovesVehicle()
        {
            Assert.IsTrue(await CreateService().DeleteAsync(1, CancellationToken.None));
            Assert.IsNull(await CreateService().GetByIdAsync(1, CancellationToken.None));
        }

        [TestMethod]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            Assert.IsFalse(await CreateService().DeleteAsync(999, CancellationToken.None));
        }
    }
}
