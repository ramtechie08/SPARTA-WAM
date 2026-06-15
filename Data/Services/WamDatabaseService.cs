using Microsoft.Extensions.Logging;
using SPARTA_WAM.Data.Models;
using SPARTA_WAM.Data.Repositories;
using SPARTA_WAM.Models;

namespace SPARTA_WAM.Data.Services;

public class WamDatabaseService : IWamDatabaseService
{
    private readonly ISqlExecutionService _sqlExecutionService;
    private readonly IWalkAwayMarginRepository _repository;
    private readonly ILogger<WamDatabaseService> _logger;

    public WamDatabaseService(
        ISqlExecutionService sqlExecutionService,
        IWalkAwayMarginRepository repository,
        ILogger<WamDatabaseService> logger)
    {
        _sqlExecutionService = sqlExecutionService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> ExecuteWamScriptsAsync(List<SqlScriptResult> scripts)
    {
        if (!scripts.Any())
        {
            _logger.LogWarning("No scripts to execute");
            return 0;
        }

        try
        {
            var combinedScript = string.Join($"{Environment.NewLine}", scripts.Select(s => s.SqlStatement));//GO{Environment.NewLine}
            var rowsAffected = await _sqlExecutionService.ExecuteSqlAsync(combinedScript);

            _logger.LogInformation($"Successfully executed {scripts.Count} WAM update scripts. Rows affected: {rowsAffected}");
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing WAM scripts");
            throw;
        }
    }

    public async Task<List<WalkAwayMarginDto>> GetWamRecordsAsync(string? salesOrg = null, string? pricingType = null)
    {
        try
        {
            var records = await _repository.GetAllAsync();

            var filtered = records
                .Where(r => string.IsNullOrEmpty(salesOrg) || r.SalesOrg == salesOrg)
                .Where(r => string.IsNullOrEmpty(pricingType) || r.DirectPricing == pricingType)
                .ToList();

            return filtered.Select(r => new WalkAwayMarginDto
            {
                ShortCode = r.ShortCode,
                MarginValue = r.WalkAwayMargin,
                SalesOrganization = r.SalesOrg,
                PricingType = r.DirectPricing,
                CreatedDateTime = r.CreatedDateTime,
                UpdatedDateTime = r.UpdatedDateTime
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WAM records");
            throw;
        }
    }
}