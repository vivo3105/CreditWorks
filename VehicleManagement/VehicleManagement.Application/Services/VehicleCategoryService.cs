using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.Services;
using VehicleManagement.Domain.ValueObjects;

namespace VehicleManagement.Application.Services;

public sealed class VehicleCategoryService : IVehicleCategoryService
{
    // Matches the column sizes in VehicleCategoryConfiguration.
    private const int MaxTextLength = 100;

    private readonly IVehicleCategoryRepository _categories;
    private readonly CategoryConfigurationValidator _validator;
    private readonly ICategoryIconProvider _icons;
    private readonly IUnitOfWork _unitOfWork;

    public VehicleCategoryService(
        IVehicleCategoryRepository categories,
        CategoryConfigurationValidator validator,
        ICategoryIconProvider icons,
        IUnitOfWork unitOfWork)
    {
        _categories = categories;
        _validator = validator;
        _icons = icons;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<VehicleCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var categories = await _categories.GetAllAsync(cancellationToken);

        return categories.Select(ToDto).ToList();
    }

    public async Task<VehicleCategoryDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken);

        return category is null ? null : ToDto(category);
    }

    public async Task<VehicleCategoryDto> CreateAsync(
        SaveVehicleCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var range = ToWeightRange(request);
        var all = await _categories.GetAllAsync(cancellationToken);

        EnsureValidNameAndIcon(request, all, except: null);

        // Fails before anything is added if the range clashes with any category.
        _validator.EnsureNoClash(all, range);

        var created = new VehicleCategory(request.Name, range, request.Icon);

        await _categories.AddAsync(created, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(created);
    }

    public async Task<VehicleCategoryDto?> UpdateAsync(
        int id,
        SaveVehicleCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var all = await _categories.GetAllAsync(cancellationToken);
        var target = all.FirstOrDefault(x => x.Id == id);

        if (target is null)
        {
            return null;
        }

        var range = ToWeightRange(request);
        EnsureValidNameAndIcon(request, all, except: target);

        // Compare against every other category; fails before the target is
        // changed, so nothing is updated on a clash.
        _validator.EnsureNoClash(all.Where(x => x != target), range);

        target.Update(request.Name, range, request.Icon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(target);
    }

    // Deleting can only leave a gap, never a clash, so nothing else changes.
    // Vehicles whose weight was in this range become uncategorised.
    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken);

        if (category is null)
        {
            return false;
        }

        _categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private void EnsureValidNameAndIcon(
        SaveVehicleCategoryRequest request,
        IEnumerable<VehicleCategory> all,
        VehicleCategory? except)
    {
        // Every save must use a configured icon, including edits of a category
        // whose icon has since been removed from the list.
        if (!string.IsNullOrWhiteSpace(request.Icon) && !_icons.Exists(request.Icon))
        {
            throw new InvalidCategoryConfigurationException(
                $"Icon '{request.Icon.Trim()}' is not one of the configured category icons.");
        }

        if (request.Name?.Trim().Length > MaxTextLength || request.Icon?.Trim().Length > MaxTextLength)
        {
            throw new InvalidCategoryConfigurationException(
                $"Category name and icon must be {MaxTextLength} characters or fewer.");
        }

        var name = request.Name?.Trim();

        if (!string.IsNullOrEmpty(name) &&
            all.Any(x => x != except && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidCategoryConfigurationException(
                $"Category name '{name}' is already used.");
        }
    }

    // WeightRange throws ArgumentExceptions (a 500); report them as a
    // category configuration problem (a 400) instead.
    private static WeightRange ToWeightRange(SaveVehicleCategoryRequest request)
    {
        if (request.MinWeight < 0)
        {
            throw new InvalidCategoryConfigurationException(
                "Minimum weight cannot be negative.");
        }

        if (request.MaxWeight <= request.MinWeight)
        {
            throw new InvalidCategoryConfigurationException(
                "Maximum weight must be greater than the minimum weight.");
        }

        return new WeightRange(request.MinWeight, request.MaxWeight);
    }

    private static VehicleCategoryDto ToDto(VehicleCategory x) =>
        new(x.Id, x.Name, x.MinWeight, x.MaxWeight, x.Icon);
}
