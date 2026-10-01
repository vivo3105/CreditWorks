using VehicleManagement.Application.Dtos;

namespace VehicleManagement.Application.Interfaces;

public interface IManufacturerService
{
    Task<IReadOnlyList<ManufacturerDto>> GetAllAsync(CancellationToken cancellationToken);
}
