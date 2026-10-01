using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Interfaces;

namespace VehicleManagement.Application.Services;

public sealed class ManufacturerService : IManufacturerService
{
    private readonly IManufacturerRepository _manufacturers;

    public ManufacturerService(IManufacturerRepository manufacturers)
    {
        _manufacturers = manufacturers;
    }

    public async Task<IReadOnlyList<ManufacturerDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var manufacturers = await _manufacturers.GetAllAsync(cancellationToken);

        return manufacturers
            .Select(x => new ManufacturerDto(x.Id, x.Name))
            .ToList();
    }
}
