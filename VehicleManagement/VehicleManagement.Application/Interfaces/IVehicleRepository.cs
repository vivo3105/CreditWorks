using VehicleManagement.Domain.Entities;

namespace VehicleManagement.Application.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);

    void Remove(Vehicle vehicle);
}
