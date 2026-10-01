using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Interfaces;

namespace VehicleManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturersController : ControllerBase
{
    private readonly IManufacturerService _manufacturerService;

    public ManufacturersController(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ManufacturerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ManufacturerDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var manufacturers = await _manufacturerService.GetAllAsync(cancellationToken);

        return Ok(manufacturers);
    }
}
