using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Interfaces;
using VehicleManagement.Domain.Entities;
using VehicleManagement.Domain.Exceptions;
using VehicleManagement.Domain.Services;

namespace VehicleManagement.Application.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicles;
    private readonly IManufacturerRepository _manufacturers;
    private readonly IVehicleCategoryRepository _categories;
    private readonly VehicleCategoryResolver _categoryResolver;
    private readonly IUnitOfWork _unitOfWork;

    public VehicleService(
        IVehicleRepository vehicles,
        IManufacturerRepository manufacturers,
        IVehicleCategoryRepository categories,
        VehicleCategoryResolver categoryResolver,
        IUnitOfWork unitOfWork)
    {
        _vehicles = vehicles;
        _manufacturers = manufacturers;
        _categories = categories;
        _categoryResolver = categoryResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var vehicles = await _vehicles.GetAllAsync(cancellationToken);

        if (vehicles.Count == 0)
        {
            return Array.Empty<VehicleDto>();
        }

        var manufacturers = await GetManufacturerNamesAsync(cancellationToken);
        var categories = await _categories.GetAllAsync(cancellationToken);

        return vehicles
            .Select(vehicle => ToDto(vehicle, manufacturers, categories))
            .ToList();
    }

    public async Task<VehicleDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.GetByIdAsync(id, cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        var manufacturers = await GetManufacturerNamesAsync(cancellationToken);
        var categories = await _categories.GetAllAsync(cancellationToken);

        return ToDto(vehicle, manufacturers, categories);
    }

    public async Task<VehicleDto> CreateAsync(
        SaveVehicleRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureManufacturerExistsAsync(request.ManufacturerId, cancellationToken);

        var vehicle = new Vehicle(
            request.OwnerName,
            request.ManufacturerId,
            request.YearOfManufacture,
            request.Weight);

        await _vehicles.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(vehicle, cancellationToken);
    }

    public async Task<VehicleDto?> UpdateAsync(
        int id,
        SaveVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.GetByIdAsync(id, cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        await EnsureManufacturerExistsAsync(request.ManufacturerId, cancellationToken);

        vehicle.Update(
            request.OwnerName,
            request.ManufacturerId,
            request.YearOfManufacture,
            request.Weight);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(vehicle, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.GetByIdAsync(id, cancellationToken);

        if (vehicle is null)
        {
            return false;
        }

        _vehicles.Remove(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureManufacturerExistsAsync(
        int manufacturerId,
        CancellationToken cancellationToken)
    {
        if (!await _manufacturers.ExistsAsync(manufacturerId, cancellationToken))
        {
            throw new InvalidVehicleException(
                $"Manufacturer {manufacturerId} does not exist.");
        }
    }

    private async Task<VehicleDto> ToDtoAsync(
        Vehicle vehicle,
        CancellationToken cancellationToken)
    {
        var manufacturers = await GetManufacturerNamesAsync(cancellationToken);
        var categories = await _categories.GetAllAsync(cancellationToken);

        return ToDto(vehicle, manufacturers, categories);
    }

    private async Task<IReadOnlyDictionary<int, string>> GetManufacturerNamesAsync(
        CancellationToken cancellationToken)
    {
        var manufacturers = await _manufacturers.GetAllAsync(cancellationToken);

        return manufacturers.ToDictionary(x => x.Id, x => x.Name);
    }

    private VehicleDto ToDto(
        Vehicle vehicle,
        IReadOnlyDictionary<int, string> manufacturers,
        IReadOnlyList<VehicleCategory> categories)
    {
        var category = _categoryResolver.Resolve(vehicle.Weight, categories);

        return new VehicleDto(
            vehicle.Id,
            vehicle.OwnerName,
            vehicle.ManufacturerId,
            manufacturers.GetValueOrDefault(vehicle.ManufacturerId, string.Empty),
            vehicle.YearOfManufacture,
            vehicle.Weight,
            category?.Name,
            category?.Icon);
    }
}
