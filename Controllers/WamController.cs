using Microsoft.AspNetCore.Mvc;
using SPARTA_WAM.Data.Services;
using SPARTA_WAM.Services;

namespace SPARTA_WAM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WamController : ControllerBase
{
    private readonly IWamProcessingService _processingService;
    private readonly IWamDatabaseService _databaseService;
    private readonly ILogger<WamController> _logger;

    public WamController(
        IWamProcessingService processingService,
        IWamDatabaseService databaseService,
        ILogger<WamController> logger)
    {
        _processingService = processingService;
        _databaseService = databaseService;
        _logger = logger;
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("generate-script")]
    public async Task<IActionResult> GenerateWamUpdateScript(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx files are supported");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var (scripts, errors) = await _processingService.ProcessWamUpdateAsync(stream);

                if (errors.Count > 0)
                    return BadRequest(new { errors });

                var formattedScript = scripts.Any()
                    ? string.Join("\n\nGO\n\n", scripts.Select(s => s.SqlStatement))
                    : string.Empty;

                return Ok(new
                {
                    success = true,
                    scriptCount = scripts.Count,
                    sqlScript = formattedScript,
                    details = scripts.Select(s => new
                    {
                        s.ShortCode,
                        s.MarginValue,
                        s.SalesOrganization,
                        s.PricingType
                    })
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating WAM script");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("execute-scripts")]
    public async Task<IActionResult> ExecuteWamScripts([FromBody] List<string> sqlScripts)
    {
        if (sqlScripts == null || sqlScripts.Count == 0)
            return BadRequest("At least one SQL script is required");

        try
        {
            var combinedScript = string.Join("\n\nGO\n\n", sqlScripts);
            var rowsAffected = await _databaseService.ExecuteWamScriptsAsync(
                sqlScripts.Select((script, index) => new SPARTA_WAM.Models.SqlScriptResult
                {
                    SqlStatement = script,
                    ShortCode = $"batch_{index}",
                    MarginValue = 0,
                    SalesOrganization = "batch",
                    PricingType = "batch"
                }).ToList());

            return Ok(new
            {
                success = true,
                message = "Scripts executed successfully",
                rowsAffected
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing WAM scripts");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("records")]
    public async Task<IActionResult> GetWamRecords([FromQuery] string? salesOrg = null, [FromQuery] string? pricingType = null)
    {
        try
        {
            var records = await _databaseService.GetWamRecordsAsync(salesOrg, pricingType);
            return Ok(new
            {
                success = true,
                recordCount = records.Count,
                records
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WAM records");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("upload-and-execute")]
    public async Task<IActionResult> UploadAndExecuteWamUpdate(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .xlsx files are supported");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var (scripts, errors) = await _processingService.ProcessWamUpdateAsync(stream);

                if (errors.Count > 0)
                    return BadRequest(new { errors });

                if (!scripts.Any())
                    return BadRequest("No valid scripts to execute");

                var rowsAffected = await _databaseService.ExecuteWamScriptsAsync(scripts);

                return Ok(new
                {
                    success = true,
                    message = "WAM update completed successfully",
                    scriptCount = scripts.Count,
                    rowsAffected,
                    details = scripts.Select(s => new
                    {
                        s.ShortCode,
                        s.MarginValue,
                        s.SalesOrganization,
                        s.PricingType
                    })
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during upload and execute");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}