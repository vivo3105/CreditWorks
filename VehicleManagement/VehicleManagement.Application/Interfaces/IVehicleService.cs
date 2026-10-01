using VehicleManagement.Application.Dtos;

namespace VehicleManagement.Application.Interfaces;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<VehicleDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<VehicleDto> CreateAsync(SaveVehicleRequest request, CancellationToken cancellationToken);

    // Returns null when the vehicle does not exist.
    Task<VehicleDto?> UpdateAsync(int id, SaveVehicleRequest request, CancellationToken cancellationToken);

    // Returns false when the vehicle does not exist.
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
