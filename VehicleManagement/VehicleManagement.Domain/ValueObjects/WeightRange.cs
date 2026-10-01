namespace VehicleManagement.Domain.ValueObjects;

public sealed class WeightRange : IEquatable<WeightRange>
{
    public WeightRange(decimal minWeight, decimal? maxWeight)
    {
        if (minWeight < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minWeight),
                "Minimum weight cannot be negative.");
        }

        if (maxWeight.HasValue && maxWeight.Value <= minWeight)
        {
            throw new ArgumentException(
                "Maximum weight must be greater than minimum weight.",
                nameof(maxWeight));
        }

        MinWeight = minWeight;
        MaxWeight = maxWeight;
    }

    public decimal MinWeight { get; }

    // Inclusive ("up to and including"); null = no upper limit.
    public decimal? MaxWeight { get; }

    public bool Contains(decimal weight)
    {
        return weight >= MinWeight &&
               (!MaxWeight.HasValue || weight <= MaxWeight.Value);
    }

    // Both limits are inclusive, so sharing a limit is an overlap: [0, 500] and
    // [500, 2500] both contain 500. Neighbours must leave no shared weight,
    // e.g. [0, 499.99] and [500, 2499.99].
    public bool Overlaps(WeightRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisEndsAtOrAfterOtherStarts =
            !MaxWeight.HasValue ||
            MaxWeight.Value >= other.MinWeight;

        var otherEndsAtOrAfterThisStarts =
            !other.MaxWeight.HasValue ||
            other.MaxWeight.Value >= MinWeight;

        return thisEndsAtOrAfterOtherStarts &&
               otherEndsAtOrAfterThisStarts;
    }

    public override string ToString()
    {
        return MaxWeight.HasValue
            ? $"[{MinWeight}, {MaxWeight}]"
            : $"[{MinWeight}, ∞)";
    }

    public bool Equals(WeightRange? other)
    {
        if (other is null)
        {
            return false;
        }

        return MinWeight == other.MinWeight &&
               MaxWeight == other.MaxWeight;
    }

    public override bool Equals(object? obj)
    {
        return obj is WeightRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MinWeight, MaxWeight);
    }
}