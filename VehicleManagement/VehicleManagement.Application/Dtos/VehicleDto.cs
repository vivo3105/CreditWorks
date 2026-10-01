namespace VehicleManagement.Application.Dtos;

// CategoryName and CategoryIcon are null when the weight falls in a gap
// between categories (the vehicle is uncategorised).
public sealed record VehicleDto(
    int Id,
    string OwnerName,
    int ManufacturerId,
    string ManufacturerName,
    int YearOfManufacture,
    decimal Weight,
    string? CategoryName,
    string? CategoryIcon);
