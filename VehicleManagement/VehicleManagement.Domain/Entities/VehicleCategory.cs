using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.ValueObjects;

namespace VehicleManagement.Domain.Entities;

public sealed class VehicleCategory
{
    private VehicleCategory()
    {
        // Required by EF Core.
    }

    public VehicleCategory(
        string name,
        WeightRange weightRange,
        string icon)
    {
        SetName(name);
        SetWeightRange(weightRange);
        SetIcon(icon);
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public decimal MinWeight { get; private set; }

    public decimal? MaxWeight { get; private set; }

    public string Icon { get; private set; } = null!;

    public WeightRange GetWeightRange()
    {
        return new WeightRange(
            MinWeight,
            MaxWeight);
    }

    public bool ContainsWeight(decimal weight)
    {
        return GetWeightRange().Contains(weight);
    }

    public void Update(
        string name,
        WeightRange weightRange,
        string icon)
    {
        SetName(name);
        SetWeightRange(weightRange);
        SetIcon(icon);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidCategoryConfigurationException(
                "Category name is required.");
        }

        Name = name.Trim();
    }

    private void SetWeightRange(WeightRange weightRange)
    {
        ArgumentNullException.ThrowIfNull(weightRange);

        MinWeight = weightRange.MinWeight;
        MaxWeight = weightRange.MaxWeight;
    }

    private void SetIcon(string icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
        {
            throw new InvalidCategoryConfigurationException(
                "Category icon is required.");
        }

        Icon = icon.Trim();
    }
}