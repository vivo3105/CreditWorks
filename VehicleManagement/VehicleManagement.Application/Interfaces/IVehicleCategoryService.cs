using VehicleManagement.Application.Dtos;

namespace VehicleManagement.Application.Interfaces;

// Weight ranges may not clash (overlap); create and update fail without saving
// if they would. Gaps are allowed: vehicles in a gap are uncategorised.
public interface IVehicleCategoryService
{
    Task<IReadOnlyList<VehicleCategoryDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<VehicleCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<VehicleCategoryDto> CreateAsync(SaveVehicleCategoryRequest request, CancellationToken cancellationToken);

    // Returns null when the category does not exist.
    Task<VehicleCategoryDto?> UpdateAsync(int id, SaveVehicleCategoryRequest request, CancellationToken cancellationToken);

    // Returns false when the category does not exist.
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
