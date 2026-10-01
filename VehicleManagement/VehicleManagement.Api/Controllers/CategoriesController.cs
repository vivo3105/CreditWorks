using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Application.Dtos;
using VehicleManagement.Application.Interfaces;

namespace VehicleManagement.Api.Controllers;

// Weight ranges may not clash: create and update return 400 (and save nothing)
// if the range overlaps another category. Gaps are allowed; vehicles whose
// weight falls in a gap are uncategorised.
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IVehicleCategoryService _categoryService;
    private readonly ICategoryIconProvider _icons;

    public CategoriesController(
        IVehicleCategoryService categoryService,
        ICategoryIconProvider icons)
    {
        _categoryService = categoryService;
        _icons = icons;
    }

    // The icons a category can use (configured under "CategoryIcons").
    [HttpGet("icons")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryIconDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<CategoryIconDto>> GetIcons()
    {
        return Ok(_icons.GetAll());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VehicleCategoryDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VehicleCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleCategoryDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehicleCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VehicleCategoryDto>> Create(
        SaveVehicleCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VehicleCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleCategoryDto>> Update(
        int id,
        SaveVehicleCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.UpdateAsync(id, request, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    // Leaves a gap; vehicles in the deleted range become uncategorised.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _categoryService.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
