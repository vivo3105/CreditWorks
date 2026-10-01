namespace VehicleManagement.Application.Dtos;

// One selectable category icon. Name is what a category stores in its Icon
// field; ImageUrl is where the image is loaded from (a path relative to the web
// app, e.g. "icons/light.svg", or an absolute URL).
public sealed record CategoryIconDto(string Name, string ImageUrl);
