using Microsoft.AspNetCore.Mvc;
using SPARTA_WAM.Data.Services;
using SPARTA_WAM.Services;
using SPARTA_WAM.Models;

namespace SPARTA_WAM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LaoFreezeController : ControllerBase
{
    private readonly ILaoFreezeProcessingService _processingService;
    private readonly IRegionAwareLaoFreezeDatabaseService _regionAwareDatabaseService;
    private readonly IRegionProvider _regionProvider;
    private readonly ILogger<LaoFreezeController> _logger;

    public LaoFreezeController(
        ILaoFreezeProcessingService processingService,
        IRegionAwareLaoFreezeDatabaseService regionAwareDatabaseService,
        IRegionProvider regionProvider,
        ILogger<LaoFreezeController> logger)
    {
        _processingService = processingService;
        _regionAwareDatabaseService = regionAwareDatabaseService;
        _regionProvider = regionProvider;
        _logger = logger;
    }

    /// <summary>
    /// Generate SQL scripts for LAO Freeze/Block PA operation.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
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
                        s.ReleaseDate,
                        s.Region
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
    [ApiExplorerSettings(IgnoreApi = true)]
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
                        s.ReleaseDate,
                        s.Region
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
    /// Upload and execute LAO Freeze/Block PA scripts in one step.
    /// Supports single region or multi-region execution based on Excel data.
    /// </summary>
    [HttpPost("freeze-pa/upload-and-execute")]
    public async Task<IActionResult> UploadAndExecuteLaoFreeze(IFormFile file, [FromQuery] string? regionCode = null)
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

                // Filter by region if specified
                var scriptsToExecute = !string.IsNullOrEmpty(regionCode)
                    ? scripts.Where(s => s.Region.Equals(regionCode, StringComparison.OrdinalIgnoreCase)).ToList()
                    : scripts;

                if (!scriptsToExecute.Any())
                    return BadRequest($"No scripts found for region: {regionCode}");

                var rowsAffectedByRegion = new Dictionary<string, int>();
                var executedDetails = new List<LaoFreezeExecutionDetail>();

                // Group scripts by region
                var groupedByRegion = scriptsToExecute.GroupBy(s => s.Region);

                foreach (var regionGroup in groupedByRegion)
                {
                    try
                    {
                        var region = regionGroup.Key;
                        var isValidRegion = await _regionProvider.IsValidRegionAsync(region);

                        if (!isValidRegion)
                        {
                            _logger.LogWarning($"Invalid or inactive region: {region}");
                            rowsAffectedByRegion[region] = -1;
                            continue;
                        }

                        var rowsAffected = await _regionAwareDatabaseService.ExecuteLaoFreezeScriptsAsync(
                            regionGroup.ToList(),
                            region);

                        rowsAffectedByRegion[region] = rowsAffected;

                        // Add execution details
                        foreach (var script in regionGroup)
                        {
                            executedDetails.Add(new LaoFreezeExecutionDetail
                            {
                                ContractNumber = script.ContractNumber,
                                BlockDate = script.BlockDate,
                                ReleaseDate = script.ReleaseDate,
                                Region = script.Region,
                                FreezeType = script.FreezeType
                            });
                        }

                        _logger.LogInformation($"Successfully executed LAO Freeze for region {region}. Rows affected: {rowsAffected}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error executing LAO Freeze for region: {regionGroup.Key}");
                        rowsAffectedByRegion[regionGroup.Key] = -1;
                    }
                }

                return Ok(new LaoFreezeUploadExecuteResponse
                {
                    Success = true,
                    FreezeType = "PA",
                    Message = $"{regionCode} Freeze/Block PA completed successfully",
                    ScriptCount = scriptsToExecute.Count,
                    RowsAffectedByRegion = rowsAffectedByRegion,
                    Details = executedDetails
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
    /// Upload and execute LAO Product Freeze scripts in one step.
    /// Supports single region or multi-region execution based on Excel data.
    /// </summary>
    [HttpPost("freeze-product/upload-and-execute")]
    public async Task<IActionResult> UploadAndExecuteLaoProductFreeze(IFormFile file, [FromQuery] string? regionCode = null)
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

                // Filter by region if specified
                var scriptsToExecute = !string.IsNullOrEmpty(regionCode)
                    ? scripts.Where(s => s.Region.Equals(regionCode, StringComparison.OrdinalIgnoreCase)).ToList()
                    : scripts;

                if (!scriptsToExecute.Any())
                    return BadRequest($"No scripts found for region: {regionCode}");

                var rowsAffectedByRegion = new Dictionary<string, int>();
                var executedDetails = new List<LaoFreezeExecutionDetail>();

                // Group scripts by region
                var groupedByRegion = scriptsToExecute.GroupBy(s => s.Region);

                foreach (var regionGroup in groupedByRegion)
                {
                    try
                    {
                        var region = regionGroup.Key;
                        var isValidRegion = await _regionProvider.IsValidRegionAsync(region);

                        if (!isValidRegion)
                        {
                            _logger.LogWarning($"Invalid or inactive region: {region}");
                            rowsAffectedByRegion[region] = -1;
                            continue;
                        }

                        var rowsAffected = await _regionAwareDatabaseService.ExecuteLaoFreezeScriptsAsync(
                            regionGroup.ToList(),
                            region);

                        rowsAffectedByRegion[region] = rowsAffected;

                        // Add execution details
                        foreach (var script in regionGroup)
                        {
                            executedDetails.Add(new LaoFreezeExecutionDetail
                            {
                                SalesOrganization = script.SalesOrganization,
                                ShortCode = script.ShortCode,
                                BlockDate = script.BlockDate,
                                ReleaseDate = script.ReleaseDate,
                                Region = script.Region,
                                FreezeType = script.FreezeType
                            });
                        }

                        _logger.LogInformation($"Successfully executed LAO Product Freeze for region {region}. Rows affected: {rowsAffected}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error executing LAO Product Freeze for region: {regionGroup.Key}");
                        rowsAffectedByRegion[regionGroup.Key] = -1;
                    }
                }

                return Ok(new LaoFreezeUploadExecuteResponse
                {
                    Success = true,
                    FreezeType = "Product",
                    Message = "LAO Product Freeze completed successfully",
                    ScriptCount = scriptsToExecute.Count,
                    RowsAffectedByRegion = rowsAffectedByRegion,
                    Details = executedDetails
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LAO Product Freeze upload and execute");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute LAO Freeze scripts for a specific region.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("freeze-pa/execute-by-region")]
    public async Task<IActionResult> ExecuteScriptsByRegion([FromBody] ExecuteLaoFreezeRequest request)
    {
        if (string.IsNullOrEmpty(request.RegionCode))
            return BadRequest("Region code is required");

        if (request.SqlScripts == null || request.SqlScripts.Count == 0)
            return BadRequest("At least one SQL script is required");

        try
        {
            var isValidRegion = await _regionProvider.IsValidRegionAsync(request.RegionCode);
            if (!isValidRegion)
                return BadRequest($"Invalid or inactive region: {request.RegionCode}");

            var scriptResults = request.SqlScripts.Select((script, index) => new LaoFreezeScriptResult
            {
                SqlStatement = script,
                BlockDate = DateTime.Now,
                ReleaseDate = DateTime.Now,
                FreezeType = "Manual",
                Region = request.RegionCode
            }).ToList();

            var rowsAffected = await _regionAwareDatabaseService.ExecuteLaoFreezeScriptsAsync(scriptResults, request.RegionCode);

            return Ok(new
            {
                success = true,
                region = request.RegionCode,
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
    /// Execute LAO Freeze scripts for all active regions.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("freeze-pa/execute-all-regions")]
    public async Task<IActionResult> ExecuteScriptsAllRegions([FromBody] List<string> sqlScripts)
    {
        if (sqlScripts == null || sqlScripts.Count == 0)
            return BadRequest("At least one SQL script is required");

        try
        {
            var regions = await _regionProvider.GetAllRegionsAsync();
            var results = new Dictionary<string, int>();

            foreach (var region in regions.Where(r => r.IsActive))
            {
                try
                {
                    var scriptResults = sqlScripts.Select(script => new LaoFreezeScriptResult
                    {
                        SqlStatement = script,
                        BlockDate = DateTime.Now,
                        ReleaseDate = DateTime.Now,
                        FreezeType = "Manual",
                        Region = region.RegionCode
                    }).ToList();

                    var rowsAffected = await _regionAwareDatabaseService.ExecuteLaoFreezeScriptsAsync(scriptResults, region.RegionCode);
                    results[region.RegionCode] = rowsAffected;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error executing scripts for region: {region.RegionCode}");
                    results[region.RegionCode] = -1;
                }
            }

            return Ok(new
            {
                success = true,
                message = "Scripts executed across all regions",
                results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scripts across regions");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get LAO Freeze records for a specific region.
    /// </summary>
    [HttpGet("records/{regionCode}")]
    public async Task<IActionResult> GetRecordsByRegion(
        string regionCode,
        [FromQuery] string? contractNumber = null,
        [FromQuery] string? salesOrg = null)
    {
        try
        {
            var isValidRegion = await _regionProvider.IsValidRegionAsync(regionCode);
            if (!isValidRegion)
                return BadRequest($"Invalid or inactive region: {regionCode}");

            var records = await _regionAwareDatabaseService.GetLaoFreezeRecordsAsync(regionCode, contractNumber, salesOrg);
            return Ok(new
            {
                success = true,
                region = regionCode,
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
    /// Get active frozen items for a specific region.
    /// </summary>
    [HttpGet("active-frozen-items/{regionCode}")]
    public async Task<IActionResult> GetActiveFrozenItemsByRegion(string regionCode)
    {
        try
        {
            var isValidRegion = await _regionProvider.IsValidRegionAsync(regionCode);
            if (!isValidRegion)
                return BadRequest($"Invalid or inactive region: {regionCode}");

            var records = await _regionAwareDatabaseService.GetActiveFrozenItemsAsync(regionCode);
            return Ok(new
            {
                success = true,
                region = regionCode,
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
    /// Get all available regions.
    /// </summary>
    [HttpGet("regions")]
    public async Task<IActionResult> GetAvailableRegions()
    {
        try
        {
            var regions = await _regionProvider.GetAllRegionsAsync();
            return Ok(new
            {
                success = true,
                regionCount = regions.Count,
                regions = regions.Select(r => new
                {
                    r.RegionCode,
                    r.RegionName,
                    r.IsActive
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving regions");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}