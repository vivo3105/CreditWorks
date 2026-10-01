using VehicleManagement.Domain.Entities;

namespace VehicleManagement.Application.Interfaces;

public interface IManufacturerRepository
{
    Task<Manufacturer?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Manufacturer>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
}
