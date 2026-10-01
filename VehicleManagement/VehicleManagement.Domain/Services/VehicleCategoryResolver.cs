using VehicleManagement.Domain.Entities;
using VehicleManagement.Domain.Exceptions;

namespace VehicleManagement.Domain.Services;

public sealed class VehicleCategoryResolver
{
    // Returns null when the weight falls in a gap between categories (the
    // vehicle is uncategorised). More than one match would mean clashing
    // ranges, which the validator prevents, so that is treated as corrupt data.
    public VehicleCategory? Resolve(
        decimal vehicleWeight,
        IEnumerable<VehicleCategory> categories)
    {
        if (vehicleWeight <= 0)
        {
            throw new InvalidVehicleException(
                "Vehicle weight must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(categories);

        var matches = categories
            .Where(category => category.ContainsWeight(vehicleWeight))
            .ToList();

        if (matches.Count > 1)
        {
            throw new InvalidCategoryConfigurationException(
                $"Vehicle weight {vehicleWeight} kg matches more than one category: " +
                string.Join(", ", matches.Select(x => $"'{x.Name}'")) + ".");
        }

        return matches.Count == 1 ? matches[0] : null;
    }
}