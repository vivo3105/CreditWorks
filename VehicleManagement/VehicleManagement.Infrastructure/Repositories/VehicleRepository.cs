using Microsoft.EntityFrameworkCore;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Infrastructure.Persistence;

namespace VehicleManagement.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _db;

    public VehicleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        await _db.Vehicles.AddAsync(vehicle, cancellationToken);
    }

    public async Task<Vehicle?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.Vehicles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    } 

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Vehicles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Vehicle vehicle)
    {
        _db.Vehicles.Remove(vehicle);
    }
}
