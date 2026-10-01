using Microsoft.EntityFrameworkCore;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Infrastructure.Persistence;

namespace VehicleManagement.Infrastructure.Repositories;

public class VehicleCategoryRepository : IVehicleCategoryRepository
{
    private readonly AppDbContext _db;

    public VehicleCategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(VehicleCategory category, CancellationToken cancellationToken)
    {
        await _db.VehicleCategories.AddAsync(category, cancellationToken);
    }

    public async Task<VehicleCategory?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.VehicleCategories
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    // Tracked so callers can edit categories and re-validate the whole set
    // before saving.
    public async Task<IReadOnlyList<VehicleCategory>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.VehicleCategories
            .OrderBy(x => x.MinWeight)
            .ToListAsync(cancellationToken);
    }

    public void Remove(VehicleCategory category)
    {
        _db.VehicleCategories.Remove(category);
    }
}
