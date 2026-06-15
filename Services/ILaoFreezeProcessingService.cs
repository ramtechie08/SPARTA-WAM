using SPARTA_WAM.Models;

namespace SPARTA_WAM.Services;

/// <summary>
/// Orchestrates the LAO Freeze/Block PA and Product Freeze processing pipeline.
/// </summary>
public interface ILaoFreezeProcessingService
{
    Task<(List<LaoFreezeScriptResult> Scripts, List<string> Errors)> ProcessLaoFreezeAsync(Stream excelFileStream);
    Task<(List<LaoFreezeScriptResult> Scripts, List<string> Errors)> ProcessLaoProductFreezeAsync(Stream excelFileStream);
}

public class LaoFreezeProcessingService : ILaoFreezeProcessingService
{
    private readonly ILaoFreezeExcelProcessor _excelProcessor;
    private readonly ILaoFreezeInputValidationService _validationService;
    private readonly ILaoFreezeSqlScriptGenerator _scriptGenerator;
    private readonly ILogger<LaoFreezeProcessingService> _logger;

    public LaoFreezeProcessingService(
        ILaoFreezeExcelProcessor excelProcessor,
        ILaoFreezeInputValidationService validationService,
        ILaoFreezeSqlScriptGenerator scriptGenerator,
        ILogger<LaoFreezeProcessingService> logger)
    {
        _excelProcessor = excelProcessor;
        _validationService = validationService;
        _scriptGenerator = scriptGenerator;
        _logger = logger;
    }

    public async Task<(List<LaoFreezeScriptResult> Scripts, List<string> Errors)> ProcessLaoFreezeAsync(Stream excelFileStream)
    {
        var errors = new List<string>();
        var scripts = new List<LaoFreezeScriptResult>();

        try
        {
            _logger.LogInformation("Starting LAO Freeze/Block PA processing");

            var rows = await _excelProcessor.ProcessLaoFreezeAsync(excelFileStream);

            if (rows.Count == 0)
            {
                errors.Add("No valid data rows found in Excel file");
                return (scripts, errors);
            }

            _logger.LogInformation($"Parsed {rows.Count} rows from Excel file");

            var validationResult = await _validationService.ValidateLaoFreezeRowsAsync(rows);

            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning($"Validation failed with {errors.Count} errors");
                return (scripts, errors);
            }

            scripts = _scriptGenerator.GenerateLaoFreezeScripts(rows);
            _logger.LogInformation($"Generated {scripts.Count} SQL scripts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LAO Freeze processing failed");
            errors.Add($"Processing failed: {ex.Message}");
        }

        return (scripts, errors);
    }

    public async Task<(List<LaoFreezeScriptResult> Scripts, List<string> Errors)> ProcessLaoProductFreezeAsync(Stream excelFileStream)
    {
        var errors = new List<string>();
        var scripts = new List<LaoFreezeScriptResult>();

        try
        {
            _logger.LogInformation("Starting LAO Product Freeze processing");

            var rows = await _excelProcessor.ProcessLaoProductFreezeAsync(excelFileStream);

            if (rows.Count == 0)
            {
                errors.Add("No valid data rows found in Excel file");
                return (scripts, errors);
            }

            _logger.LogInformation($"Parsed {rows.Count} rows from Excel file");

            var validationResult = await _validationService.ValidateLaoProductFreezeRowsAsync(rows);

            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning($"Validation failed with {errors.Count} errors");
                return (scripts, errors);
            }

            scripts = _scriptGenerator.GenerateLaoProductFreezeScripts(rows);
            _logger.LogInformation($"Generated {scripts.Count} SQL scripts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LAO Product Freeze processing failed");
            errors.Add($"Processing failed: {ex.Message}");
        }

        return (scripts, errors);
    }
}