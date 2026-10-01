using Microsoft.VisualStudio.TestTools.UnitTesting;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.Services;
using VehicleManagement.Domain.ValueObjects;

namespace VehicleManagement.Test
{
    // MaxWeight is inclusive, so neighbours end 0.01 below the next minimum.
    [TestClass]
    public class CategoryConfigurationValidatorTests
    {
        private static readonly CategoryConfigurationValidator Validator = new();

        private static VehicleCategory Category(string name, decimal min, decimal? max) =>
            new(name, new WeightRange(min, max), $"{name.ToLowerInvariant()}_icon");

        private static readonly VehicleCategory[] Seeded =
        {
            Category("Light", 0m, 499.99m),
            Category("Medium", 500m, 2499.99m),
            Category("Heavy", 2500m, null),
        };

        // ---- EnsureNoClash (used by create/update) --------------------------

        [TestMethod]
        public void EnsureNoClash_AllowsRangeInAGap()
        {
            var withGap = new[] { Category("Light", 0m, 499.99m), Category("Heavy", 2500m, null) };

            Validator.EnsureNoClash(withGap, new WeightRange(500m, 2499.99m));
        }

        [TestMethod]
        public void EnsureNoClash_AllowsStartingOneCentAboveAnotherMax()
        {
            Validator.EnsureNoClash(new[] { Category("Light", 0m, 499.99m) }, new WeightRange(500m, null));
        }

        [TestMethod]
        public void EnsureNoClash_RejectsMinEqualToAnotherMax()
        {
            var ex = Assert.ThrowsException<InvalidCategoryConfigurationException>(
                () => Validator.EnsureNoClash(new[] { Category("Light", 0m, 500m) }, new WeightRange(500m, null)));

            Assert.AreEqual(
                "Weight range 500 kg and above clashes with category 'Light' (0 – 500 kg).",
                ex.Message);
        }

        [TestMethod]
        public void EnsureNoClash_RejectsMaxEqualToAnotherMin()
        {
            Assert.ThrowsException<InvalidCategoryConfigurationException>(
                () => Validator.EnsureNoClash(new[] { Category("Heavy", 2500m, null) }, new WeightRange(500m, 2500m)));
        }

        [TestMethod]
        public void EnsureNoClash_RejectsOverlap_NamingTheClashingCategory()
        {
            var ex = Assert.ThrowsException<InvalidCategoryConfigurationException>(
                () => Validator.EnsureNoClash(Seeded, new WeightRange(5000m, null)));

            Assert.AreEqual(
                "Weight range 5000 kg and above clashes with category 'Heavy' (2500 kg and above).",
                ex.Message);
        }

        [TestMethod]
        public void EnsureNoClash_RejectsRangeInsideAnother_AndRangeCoveringAnother()
        {
            Assert.ThrowsException<InvalidCategoryConfigurationException>(
                () => Validator.EnsureNoClash(Seeded, new WeightRange(1000m, 2000m)));
            Assert.ThrowsException<InvalidCategoryConfigurationException>(
                () => Validator.EnsureNoClash(new[] { Category("Medium", 500m, 2499.99m) }, new WeightRange(0m, null)));
        }

        // ---- Validate (whole set) ------------------------------------------

        [TestMethod]
        public void Validate_AllowsGaps_AndEmptySet()
        {
            Validator.Validate(Seeded);
            Validator.Validate(new[] { Category("Light", 0m, 499.99m), Category("Heavy", 2500m, null) });
            Validator.Validate(Array.Empty<VehicleCategory>());
        }

        [TestMethod]
        public void Validate_RejectsSharedLimit()
        {
            var ex = Assert.ThrowsException<InvalidCategoryConfigurationException>(
                () => Validator.Validate(new[] { Category("Light", 0m, 500m), Category("Medium", 500m, 2499.99m) }));

            Assert.AreEqual(
                "Categories 'Light' (0 – 500 kg) and 'Medium' (500 – 2499.99 kg) clash.",
                ex.Message);
        }
    }
}
