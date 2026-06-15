using Microsoft.AspNetCore.Mvc;
using SPARTA_WAM.Data.Services;
using SPARTA_WAM.Services;

namespace SPARTA_WAM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LaoFreezeController : ControllerBase
{
    private readonly ILaoFreezeProcessingService _processingService;
    private readonly ILaoFreezeDatabaseService _databaseService;
    private readonly ILogger<LaoFreezeController> _logger;

    public LaoFreezeController(
        ILaoFreezeProcessingService processingService,
        ILaoFreezeDatabaseService databaseService,
        ILogger<LaoFreezeController> logger)
    {
        _processingService = processingService;
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Generate SQL scripts for LAO Freeze/Block PA operation.
    /// </summary>
    [HttpPost("freeze-pa/generate-script")]
    public async Task<IActionResult> GenerateLaoFreezeScript(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx files are supported");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var (scripts, errors) = await _processingService.ProcessLaoFreezeAsync(stream);

                if (errors.Count > 0)
                    return BadRequest(new { errors });

                var formattedScript = scripts.Any()
                    ? string.Join("\n\nGO\n\n", scripts.Select(s => s.SqlStatement))
                    : string.Empty;

                return Ok(new
                {
                    success = true,
                    freezeType = "PA",
                    scriptCount = scripts.Count,
                    sqlScript = formattedScript,
                    details = scripts.Select(s => new
                    {
                        s.ContractNumber,
                        s.BlockDate,
                        s.ReleaseDate
                    })
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LAO Freeze script");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate SQL scripts for LAO Product Freeze operation.
    /// </summary>
    [HttpPost("freeze-product/generate-script")]
    public async Task<IActionResult> GenerateLaoProductFreezeScript(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx files are supported");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var (scripts, errors) = await _processingService.ProcessLaoProductFreezeAsync(stream);

                if (errors.Count > 0)
                    return BadRequest(new { errors });

                var formattedScript = scripts.Any()
                    ? string.Join("\n\nGO\n\n", scripts.Select(s => s.SqlStatement))
                    : string.Empty;

                return Ok(new
                {
                    success = true,
                    freezeType = "Product",
                    scriptCount = scripts.Count,
                    sqlScript = formattedScript,
                    details = scripts.Select(s => new
                    {
                        s.SalesOrganization,
                        s.ShortCode,
                        s.BlockDate,
                        s.ReleaseDate
                    })
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LAO Product Freeze script");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute LAO Freeze scripts.
    /// </summary>
    [HttpPost("execute-scripts")]
    public async Task<IActionResult> ExecuteScripts([FromBody] List<string> sqlScripts)
    {
        if (sqlScripts == null || sqlScripts.Count == 0)
            return BadRequest("At least one SQL script is required");

        try
        {
            var scriptResults = sqlScripts.Select((script, index) => new SPARTA_WAM.Models.LaoFreezeScriptResult
            {
                SqlStatement = script,
                BlockDate = DateTime.Now,
                ReleaseDate = DateTime.Now,
                FreezeType = "Manual"
            }).ToList();

            var rowsAffected = await _databaseService.ExecuteLaoFreezeScriptsAsync(scriptResults);

            return Ok(new
            {
                success = true,
                message = "Scripts executed successfully",
                rowsAffected
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scripts");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get LAO Freeze records.
    /// </summary>
    [HttpGet("records")]
    public async Task<IActionResult> GetRecords(
        [FromQuery] string? contractNumber = null,
        [FromQuery] string? salesOrg = null)
    {
        try
        {
            var records = await _databaseService.GetLaoFreezeRecordsAsync(contractNumber, salesOrg);
            return Ok(new
            {
                success = true,
                recordCount = records.Count,
                records
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving records");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get active frozen items.
    /// </summary>
    [HttpGet("active-frozen-items")]
    public async Task<IActionResult> GetActiveFrozenItems()
    {
        try
        {
            var records = await _databaseService.GetActiveFrozenItemsAsync();
            return Ok(new
            {
                success = true,
                recordCount = records.Count,
                records
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active frozen items");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Upload and execute LAO Freeze/Block PA in one step.
    /// </summary>
    [HttpPost("freeze-pa/upload-and-execute")]
    public async Task<IActionResult> UploadAndExecuteLaoFreeze(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx files are supported");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var (scripts, errors) = await _processingService.ProcessLaoFreezeAsync(stream);

                if (errors.Count > 0)
                    return BadRequest(new { errors });

                if (!scripts.Any())
                    return BadRequest("No valid scripts to execute");

                var rowsAffected = await _databaseService.ExecuteLaoFreezeScriptsAsync(scripts);

                return Ok(new
                {
                    success = true,
                    freezeType = "PA",
                    message = "LAO Freeze/Block PA completed successfully",
                    scriptCount = scripts.Count,
                    rowsAffected,
                    details = scripts.Select(s => new
                    {
                        s.ContractNumber,
                        s.BlockDate,
                        s.ReleaseDate
                    })
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LAO Freeze/Block PA upload and execute");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Upload and execute LAO Product Freeze in one step.
    /// </summary>
    [HttpPost("freeze-product/upload-and-execute")]
    public async Task<IActionResult> UploadAndExecuteLaoProductFreeze(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx files are supported");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var (scripts, errors) = await _processingService.ProcessLaoProductFreezeAsync(stream);

                if (errors.Count > 0)
                    return BadRequest(new { errors });

                if (!scripts.Any())
                    return BadRequest("No valid scripts to execute");

                var rowsAffected = await _databaseService.ExecuteLaoFreezeScriptsAsync(scripts);

                return Ok(new
                {
                    success = true,
                    freezeType = "Product",
                    message = "LAO Product Freeze completed successfully",
                    scriptCount = scripts.Count,
                    rowsAffected,
                    details = scripts.Select(s => new
                    {
                        s.SalesOrganization,
                        s.ShortCode,
                        s.BlockDate,
                        s.ReleaseDate
                    })
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LAO Product Freeze upload and execute");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}