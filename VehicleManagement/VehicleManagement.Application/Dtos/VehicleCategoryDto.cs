namespace VehicleManagement.Application.Dtos;

// MaxWeight is null for the open-ended top category.
public sealed record VehicleCategoryDto(
    int Id,
    string Name,
    decimal MinWeight,
    decimal? MaxWeight,
    string Icon);
