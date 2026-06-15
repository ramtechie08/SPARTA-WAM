using Microsoft.EntityFrameworkCore;
using SPARTA_WAM.Data.Models;
using SPARTA_WAM.Data.Repositories;
using SPARTA_WAM.Models;

namespace SPARTA_WAM.Data.Services;

public class LaoFreezeDatabaseService : ILaoFreezeDatabaseService
{
    private readonly ISqlExecutionService _sqlExecutionService;
    private readonly ILaoFreezeRepository _repository;
    private readonly ILogger<LaoFreezeDatabaseService> _logger;

    public LaoFreezeDatabaseService(
        ISqlExecutionService sqlExecutionService,
        ILaoFreezeRepository repository,
        ILogger<LaoFreezeDatabaseService> logger)
    {
        _sqlExecutionService = sqlExecutionService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> ExecuteLaoFreezeScriptsAsync(List<LaoFreezeScriptResult> scripts)
    {
        if (!scripts.Any())
        {
            _logger.LogWarning("No scripts to execute");
            return 0;
        }

        try
        {
            var combinedScript = string.Join($"{Environment.NewLine}", scripts.Select(s => s.SqlStatement));
            var rowsAffected = await _sqlExecutionService.ExecuteSqlAsync(combinedScript);

            _logger.LogInformation($"Successfully executed {scripts.Count} LAO Freeze scripts. Rows affected: {rowsAffected}");
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing LAO Freeze scripts");
            throw;
        }
    }

    public async Task<List<LaoFreezeRecordDto>> GetLaoFreezeRecordsAsync(string? contractNumber = null, string? salesOrg = null)
    {
        try
        {
            List<SpartaPriceIncrease> records;

            if (!string.IsNullOrEmpty(contractNumber))
            {
                records = await _repository.GetByContractNumberAsync(contractNumber);
            }
            else if (!string.IsNullOrEmpty(salesOrg))
            {
                records = await _repository.GetBySalesOrgAsync(salesOrg);
            }
            else
            {
                records = await _repository.GetAllAsync();
            }

            return records.Select(r => new LaoFreezeRecordDto
            {
                ContractNumber = r.ContractNumber,
                SalesOrganization = r.SalesOrganization,
                ShortCode = r.ShortCode,
                BlockDate = r.BlockDate,
                ReleaseDate = r.ReleaseDate,
                CreatedDateTime = r.CreatedDateTime,
                FreezeType = !string.IsNullOrEmpty(r.ContractNumber) ? "PA" : "Product"
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LAO Freeze records");
            throw;
        }
    }

    public async Task<List<LaoFreezeRecordDto>> GetActiveFrozenItemsAsync()
    {
        try
        {
            var records = await _repository.GetActiveFrozenItemsAsync(DateTime.UtcNow);

            return records.Select(r => new LaoFreezeRecordDto
            {
                ContractNumber = r.ContractNumber,
                SalesOrganization = r.SalesOrganization,
                ShortCode = r.ShortCode,
                BlockDate = r.BlockDate,
                ReleaseDate = r.ReleaseDate,
                CreatedDateTime = r.CreatedDateTime,
                FreezeType = !string.IsNullOrEmpty(r.ContractNumber) ? "PA" : "Product"
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active frozen items");
            throw;
        }
    }
}