using VehicleManagement.Application.Dtos;

namespace VehicleManagement.Application.Interfaces;

// The icons a category may use. Supplied by the host (the API reads them from
// configuration, section "CategoryIcons").
public interface ICategoryIconProvider
{
    IReadOnlyList<CategoryIconDto> GetAll();

    bool Exists(string name);
}
