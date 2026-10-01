using Microsoft.EntityFrameworkCore;
using VehicleManagement.Infrastructure.Persistence;

namespace VehicleManagement.Test
{
    // Isolated in-memory database per test, seeded from the model's HasData.
    public sealed class TestDatabase : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDatabase()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public AppDbContext CreateContext() => new(_options);

        public void Dispose()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
        }
    }
}
