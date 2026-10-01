namespace VehicleManagement.Application.Dtos;

// Body for both create and update; field rules are enforced by the Vehicle entity.
public sealed record SaveVehicleRequest(
    string OwnerName,
    int ManufacturerId,
    int YearOfManufacture,
    decimal Weight);
