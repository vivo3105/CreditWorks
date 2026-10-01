using VehicleManagement.Domain.Entities;

namespace VehicleManagement.Application.Interfaces;

public interface IVehicleCategoryRepository
{
    Task<VehicleCategory?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleCategory>> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(VehicleCategory category, CancellationToken cancellationToken);

    void Remove(VehicleCategory category);
}
