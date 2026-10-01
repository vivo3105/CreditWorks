using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VehicleManagement.Api;

namespace VehicleManagement.Test
{
    [TestClass]
    public class ConfigurationCategoryIconProviderTests
    {
        private static ConfigurationCategoryIconProvider Create(params (string? Name, string? ImageUrl)[] icons)
        {
            var values = new Dictionary<string, string?>();
            for (var i = 0; i < icons.Length; i++)
            {
                values[$"CategoryIcons:{i}:Name"] = icons[i].Name;
                values[$"CategoryIcons:{i}:ImageUrl"] = icons[i].ImageUrl;
            }

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            return new ConfigurationCategoryIconProvider(configuration);
        }

        [TestMethod]
        public void ReadsIconsInConfiguredOrder_TrimmingValues()
        {
            var provider = Create(("light_icon", "icons/light.svg"), (" heavy_icon ", " https://cdn.example/heavy.png "));

            var icons = provider.GetAll();
            Assert.AreEqual(2, icons.Count);
            Assert.AreEqual("light_icon", icons[0].Name);
            Assert.AreEqual("icons/light.svg", icons[0].ImageUrl);
            Assert.AreEqual("heavy_icon", icons[1].Name);
            Assert.AreEqual("https://cdn.example/heavy.png", icons[1].ImageUrl);
        }

        [TestMethod]
        public void Exists_MatchesConfiguredNamesExactly()
        {
            var provider = Create(("light_icon", "icons/light.svg"));

            Assert.IsTrue(provider.Exists("light_icon"));
            Assert.IsTrue(provider.Exists(" light_icon "));
            Assert.IsFalse(provider.Exists("LIGHT_ICON"));
            Assert.IsFalse(provider.Exists("aaaa"));
        }

        [TestMethod]
        public void RejectsMissingSection()
        {
            var ex = Assert.ThrowsException<InvalidOperationException>(() => Create());
            StringAssert.Contains(ex.Message, "at least one icon");
        }

        [TestMethod]
        public void RejectsMissingNameOrImageUrl()
        {
            StringAssert.Contains(
                Assert.ThrowsException<InvalidOperationException>(() => Create(("", "icons/x.svg"))).Message,
                "Name is required");
            StringAssert.Contains(
                Assert.ThrowsException<InvalidOperationException>(() => Create(("x_icon", null))).Message,
                "ImageUrl is required");
        }

        [TestMethod]
        public void RejectsDuplicateNames()
        {
            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => Create(("light_icon", "a.svg"), ("light_icon", "b.svg")));
            StringAssert.Contains(ex.Message, "more than once");
        }

        [TestMethod]
        public void RejectsNameLongerThanTheDatabaseColumn()
        {
            Assert.ThrowsException<InvalidOperationException>(() => Create((new string('x', 101), "a.svg")));
        }
    }
}
