using System.Globalization;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.ValueObjects;

namespace VehicleManagement.Domain.Services;

// The one category rule: no two weight ranges may clash (overlap), so a vehicle
// weight matches at most one category. Gaps are allowed; a vehicle whose
// weight falls in a gap is simply uncategorised.
public sealed class CategoryConfigurationValidator
{
    // Use before creating or updating a category: fails if `range` clashes with
    // any category in `others` (exclude the category being updated).
    public void EnsureNoClash(
        IEnumerable<VehicleCategory> others,
        WeightRange range)
    {
        ArgumentNullException.ThrowIfNull(others);
        ArgumentNullException.ThrowIfNull(range);

        var clash = others
            .OrderBy(x => x.MinWeight)
            .FirstOrDefault(x => x.GetWeightRange().Overlaps(range));

        if (clash is not null)
        {
            throw new InvalidCategoryConfigurationException(
                $"Weight range {Describe(range)} clashes with category " +
                $"'{clash.Name}' ({Describe(clash.GetWeightRange())}).");
        }
    }

    // Checks a whole set, e.g. everything about to be saved.
    public void Validate(
        IEnumerable<VehicleCategory> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        var list = categories.OrderBy(x => x.MinWeight).ToList();

        for (var i = 0; i < list.Count; i++)
        {
            for (var j = i + 1; j < list.Count; j++)
            {
                if (list[i].GetWeightRange().Overlaps(list[j].GetWeightRange()))
                {
                    throw new InvalidCategoryConfigurationException(
                        $"Categories '{list[i].Name}' ({Describe(list[i].GetWeightRange())}) and " +
                        $"'{list[j].Name}' ({Describe(list[j].GetWeightRange())}) clash.");
                }
            }
        }
    }

    private static string Describe(WeightRange range) =>
        range.MaxWeight is null
            ? string.Create(CultureInfo.InvariantCulture, $"{range.MinWeight:0.##} kg and above")
            : string.Create(CultureInfo.InvariantCulture, $"{range.MinWeight:0.##} – {range.MaxWeight:0.##} kg");
}
