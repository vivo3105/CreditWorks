using Microsoft.EntityFrameworkCore;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Infrastructure.Persistence;

namespace VehicleManagement.Infrastructure.Repositories;

public class ManufacturerRepository : IManufacturerRepository
{
    private readonly AppDbContext _db;

    public ManufacturerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Manufacturer?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.Manufacturers
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Manufacturer>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Manufacturers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return _db.Manufacturers.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
