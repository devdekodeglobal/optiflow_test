using DEKODE.OptiFlow.Domain.Interfaces;
using DEKODE.OptiFlow.DTO.Common;
using DEKODE.OptiFlow.DTO.Planogram;
using DEKODE.OptiFlow.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DEKODE.OptiFlow.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PlanogramController : ControllerBase
{
    private readonly IPlanogramService _planogramService;

    public PlanogramController(IPlanogramService planogramService)
    {
        _planogramService = planogramService ?? throw new ArgumentNullException(nameof(planogramService));
    }

    /// <summary>
    /// Uploads and parses a raw planogram CSV file (e.g. Planogram.csv), validates merchandising constraints,
    /// and persists it atomically with its target lines via Business Services.
    /// </summary>
    [HttpPost("upload-csv")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<PlanogramImportResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCsv(
        IFormFile file,
        [FromForm] string? planogramName,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("A non-empty CSV file must be provided."));
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<object>.Fail("File must have a .csv extension."));
        }

        string name = !string.IsNullOrWhiteSpace(planogramName)
            ? planogramName
            : Path.GetFileNameWithoutExtension(file.FileName);

        await using var stream = file.OpenReadStream();
        var planogram = await _planogramService.ImportFromCsvStreamAsync(stream, name, cancellationToken);

        var responseDto = new PlanogramImportResponseDto
        {
            PlanogramId = planogram.Id,
            PlanogramName = planogram.Name,
            SeasonOrMonth = planogram.SeasonOrMonth,
            ImportedLinesCount = planogram.Lines.Count,
            DistinctStoresCount = planogram.Lines.Select(l => l.StoreName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            DistinctBrandsCount = planogram.Lines.Select(l => l.BrandName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            TotalTargetCapacity = planogram.Lines.Sum(l => l.TargetCapacity),
            ImportedAtUtc = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetById), new { id = planogram.Id }, ApiResponse<PlanogramImportResponseDto>.Ok(responseDto, "Planogram imported and persisted successfully."));
    }

    /// <summary>
    /// Retrieves a planogram and its lines by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PlanogramDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var planogram = await _planogramService.GetPlanogramByIdAsync(id, cancellationToken);
        if (planogram == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Planogram with ID {id} was not found."));
        }

        return Ok(ApiResponse<PlanogramDto>.Ok(planogram.ToDto()));
    }

    /// <summary>
    /// Retrieves the latest active planogram.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<PlanogramDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var planogram = await _planogramService.GetActivePlanogramAsync(cancellationToken);
        if (planogram == null)
        {
            return NotFound(ApiResponse<object>.Fail("No active planogram found."));
        }

        return Ok(ApiResponse<PlanogramDto>.Ok(planogram.ToDto()));
    }
}
