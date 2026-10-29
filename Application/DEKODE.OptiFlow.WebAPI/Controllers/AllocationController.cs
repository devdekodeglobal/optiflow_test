using DEKODE.OptiFlow.Domain.Interfaces;
using DEKODE.OptiFlow.DTO.Allocation;
using DEKODE.OptiFlow.DTO.Common;
using DEKODE.OptiFlow.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DEKODE.OptiFlow.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AllocationController : ControllerBase
{
    private readonly IAllocationService _allocationService;

    public AllocationController(IAllocationService allocationService)
    {
        _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
    }

    /// <summary>
    /// Pure calculation endpoint. Runs the allocation waterfall in memory without fetching or persisting to database.
    /// Ideal for simulations, dry-runs, and scenario testing.
    /// Requires Admin role.
    /// </summary>
    [HttpPost("calculate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AllocationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public IActionResult Calculate([FromBody] AllocationExecutionRequestDto request)
    {
        if (request == null || request.PlanogramLines.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("Allocation execution request must contain at least one planogram line."));
        }

        var executionContext = request.ToExecutionContext();
        var calculationResult = _allocationService.CalculateAllocationWithResult(executionContext);

        if (!calculationResult.IsSuccess || calculationResult.Allocation == null)
        {
            return BadRequest(ApiResponse<object>.Fail("Allocation calculation failed.", calculationResult.Errors));
        }

        string message = calculationResult.Warnings.Count > 0
            ? $"Allocation calculation completed with warnings: {string.Join("; ", calculationResult.Warnings)}"
            : "Allocation calculation completed successfully.";

        return Ok(ApiResponse<AllocationResultDto>.Ok(calculationResult.Allocation.ToDto(isPersisted: false), message));
    }

    /// <summary>
    /// Runs allocation calculation using the supplied payload, and persists the generated lines
    /// and KPIs atomically to the database using Unit of Work transactions.
    /// Requires Admin role.
    /// </summary>
    [HttpPost("run")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AllocationResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunAndPersist([FromBody] AllocationExecutionRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null || request.PlanogramLines.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("Allocation execution request must contain at least one planogram line."));
        }

        var executionContext = request.ToExecutionContext();
        var persistedAllocation = await _allocationService.RunAndPersistAllocationAsync(executionContext, cancellationToken);

        var resultDto = persistedAllocation.ToDto(isPersisted: true);
        return CreatedAtAction(nameof(GetById), new { id = persistedAllocation.Id }, ApiResponse<AllocationResultDto>.Ok(resultDto, "Allocation executed and persisted successfully."));
    }

    /// <summary>
    /// Retrieves a historical allocation run and its dispatched lines by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AllocationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var allocation = await _allocationService.GetAllocationByIdAsync(id, cancellationToken);
        if (allocation == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Allocation with ID {id} was not found."));
        }

        return Ok(ApiResponse<AllocationResultDto>.Ok(allocation.ToDto(isPersisted: true)));
    }
}
