using VehicleManagement.Domain.Exceptions;

namespace VehicleManagement.Domain.Entities;

public sealed class Vehicle
{
    private Vehicle()
    {
        // Required by EF Core.
    }

    public Vehicle(
        string ownerName,
        int manufacturerId,
        int yearOfManufacture,
        decimal weight)
    {
        SetOwnerName(ownerName);
        SetManufacturer(manufacturerId);
        SetYearOfManufacture(yearOfManufacture);
        SetWeight(weight);
    }

    public int Id { get; private set; }

    public string OwnerName { get; private set; } = null!;

    public int ManufacturerId { get; private set; }

    public int YearOfManufacture { get; private set; }

    public decimal Weight { get; private set; }

    public void Update(
        string ownerName,
        int manufacturerId,
        int yearOfManufacture,
        decimal weight)
    {
        SetOwnerName(ownerName);
        SetManufacturer(manufacturerId);
        SetYearOfManufacture(yearOfManufacture);
        SetWeight(weight);
    }

    private void SetOwnerName(string ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
        {
            throw new InvalidVehicleException(
                "Owner name is required.");
        }

        OwnerName = ownerName.Trim();
    }

    private void SetManufacturer(int manufacturerId)
    {
        if (manufacturerId <= 0)
        {
            throw new InvalidVehicleException(
                "A valid manufacturer is required.");
        }

        ManufacturerId = manufacturerId;
    }

    private void SetYearOfManufacture(int year)
    {
        if (year <= 0)
        {
            throw new InvalidVehicleException(
                "Year of manufacture is required.");
        }

        YearOfManufacture = year;
    }

    private void SetWeight(decimal weight)
    {
        if (weight <= 0)
        {
            throw new InvalidVehicleException(
                "Vehicle weight must be greater than zero.");
        }

        if (decimal.Round(weight, 2) != weight)
        {
            throw new InvalidVehicleException(
                "Vehicle weight must have no more than two decimal places.");
        }

        Weight = weight;
    }
}