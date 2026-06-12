using SPARTA_WAM.Models;

namespace SPARTA_WAM.Services;

public interface IWamProcessingService
{
    Task<(List<SqlScriptResult> Scripts, List<string> Errors)> ProcessWamUpdateAsync(Stream excelFileStream);
}

public class WamProcessingService : IWamProcessingService
{
    private readonly IExcelFileProcessor _excelProcessor;
    private readonly IInputValidationService _validationService;
    private readonly ISqlScriptGenerator _scriptGenerator;

    public WamProcessingService(
        IExcelFileProcessor excelProcessor,
        IInputValidationService validationService,
        ISqlScriptGenerator scriptGenerator)
    {
        _excelProcessor = excelProcessor;
        _validationService = validationService;
        _scriptGenerator = scriptGenerator;
    }

    public async Task<(List<SqlScriptResult> Scripts, List<string> Errors)> ProcessWamUpdateAsync(Stream excelFileStream)
    {
        var errors = new List<string>();
        var scripts = new List<SqlScriptResult>();

        try
        {
            var rows = await _excelProcessor.ProcessAsync(excelFileStream);

            if (rows.Count == 0)
            {
                errors.Add("No valid data rows found in Excel file");
                return (scripts, errors);
            }

            var validationResult = await _validationService.ValidateWamRowsAsync(rows);

            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
                return (scripts, errors);
            }

            scripts = _scriptGenerator.GenerateScripts(rows);
        }
        catch (Exception ex)
        {
            errors.Add($"Processing failed: {ex.Message}");
        }

        return (scripts, errors);
    }
}