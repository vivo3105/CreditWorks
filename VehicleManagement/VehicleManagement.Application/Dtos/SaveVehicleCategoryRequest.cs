namespace VehicleManagement.Application.Dtos;

// Body for both create and update. MaxWeight null = no upper limit.
public sealed record SaveVehicleCategoryRequest(
    string Name,
    decimal MinWeight,
    decimal? MaxWeight,
    string Icon);
