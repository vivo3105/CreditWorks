using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Interfaces;

namespace VehicleManagement.Api;

// Reads the selectable category icons from configuration:
//
//   "CategoryIcons": [
//     { "Name": "light_icon", "ImageUrl": "icons/light.svg" }
//   ]
//
// The list is checked once at startup so a bad config fails fast rather than
// on the first save. Changing it needs an API restart.
public sealed class ConfigurationCategoryIconProvider : ICategoryIconProvider
{
    public const string SectionName = "CategoryIcons";

    // Matches the Icon column size in VehicleCategoryConfiguration.
    private const int MaxNameLength = 100;

    private readonly IReadOnlyList<CategoryIconDto> _icons;

    public ConfigurationCategoryIconProvider(IConfiguration configuration)
    {
        var entries = configuration.GetSection(SectionName).Get<List<IconEntry>>() ?? new List<IconEntry>();

        _icons = Validate(entries);
    }

    public IReadOnlyList<CategoryIconDto> GetAll() => _icons;

    public bool Exists(string name) =>
        _icons.Any(x => string.Equals(x.Name, name?.Trim(), StringComparison.Ordinal));

    private static IReadOnlyList<CategoryIconDto> Validate(IReadOnlyList<IconEntry> entries)
    {
        if (entries.Count == 0)
        {
            throw new InvalidOperationException(
                $"Configuration section '{SectionName}' must list at least one icon.");
        }

        var icons = new List<CategoryIconDto>();

        foreach (var (entry, index) in entries.Select((e, i) => (e, i)))
        {
            var name = entry.Name?.Trim();
            var imageUrl = entry.ImageUrl?.Trim();

            if (string.IsNullOrEmpty(name) || name.Length > MaxNameLength)
            {
                throw new InvalidOperationException(
                    $"{SectionName}[{index}]: Name is required and must be {MaxNameLength} characters or fewer.");
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new InvalidOperationException($"{SectionName}[{index}] ('{name}'): ImageUrl is required.");
            }

            if (icons.Any(x => x.Name == name))
            {
                throw new InvalidOperationException($"{SectionName}: icon name '{name}' is listed more than once.");
            }

            icons.Add(new CategoryIconDto(name, imageUrl));
        }

        return icons;
    }

    private sealed class IconEntry
    {
        public string? Name { get; set; }

        public string? ImageUrl { get; set; }
    }
}
