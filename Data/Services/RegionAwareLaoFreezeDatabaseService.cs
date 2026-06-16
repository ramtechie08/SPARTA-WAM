using System.Data;
using Microsoft.Data.SqlClient;
using SPARTA_WAM.Data.Connections;
using SPARTA_WAM.Data.Models;
using SPARTA_WAM.Data.Repositories;
using SPARTA_WAM.Models;
using SPARTA_WAM.Services;

namespace SPARTA_WAM.Data.Services;

public class RegionAwareLaoFreezeDatabaseService : IRegionAwareLaoFreezeDatabaseService
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILaoFreezeRepository _repository;
    private readonly IRegionProvider _regionProvider;
    private readonly ILogger<RegionAwareLaoFreezeDatabaseService> _logger;

    public RegionAwareLaoFreezeDatabaseService(
        ISqlConnectionFactory sqlConnectionFactory,
        ILaoFreezeRepository repository,
        IRegionProvider regionProvider,
        ILogger<RegionAwareLaoFreezeDatabaseService> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _repository = repository;
        _regionProvider = regionProvider;
        _logger = logger;
    }

    public async Task<int> ExecuteLaoFreezeScriptsAsync(List<LaoFreezeScriptResult> scripts, string regionCode)
    {
        if (!scripts.Any())
        {
            _logger.LogWarning($"No scripts to execute for region: {regionCode}");
            return 0;
        }

        try
        {
            var region = await _regionProvider.GetRegionByCodeAsync(regionCode);
            if (region == null || !region.IsActive)
            {
                throw new InvalidOperationException($"Invalid or inactive region: {regionCode}");
            }

            // Filter scripts for this region
            var regionalScripts = scripts.Where(s => s.Region.Equals(regionCode, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!regionalScripts.Any())
            {
                _logger.LogWarning($"No scripts found for region: {regionCode}");
                return 0;
            }

            var combinedScript = string.Join($"{Environment.NewLine}", regionalScripts.Select(s => s.SqlStatement));
            var connection = await CreateConnectionAsync(region.ConnectionString);

            await using (connection)
            {
                using (var command = connection.Connection.CreateCommand())
                {
                    command.CommandText = combinedScript;
                    command.CommandTimeout = 300;
                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    _logger.LogInformation($"Successfully executed {regionalScripts.Count} LAO Freeze scripts for region {regionCode}. Rows affected: {rowsAffected}");
                    return rowsAffected;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing LAO Freeze scripts for region: {regionCode}");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> ExecuteLaoFreezeScriptsMultiRegionAsync(List<LaoFreezeScriptResult> scripts)
    {
        var results = new Dictionary<string, int>();

        try
        {
            var regions = await _regionProvider.GetAllRegionsAsync();
            var activeRegions = regions.Where(r => r.IsActive).ToList();

            foreach (var region in activeRegions)
            {
                try
                {
                    var rowsAffected = await ExecuteLaoFreezeScriptsAsync(scripts, region.RegionCode);
                    results[region.RegionCode] = rowsAffected;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error executing scripts for region: {region.RegionCode}");
                    results[region.RegionCode] = -1; // Error indicator
                }
            }

            _logger.LogInformation($"Multi-region execution completed. Results: {string.Join(", ", results.Select(r => $"{r.Key}={r.Value}"))}");
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multi-region LAO Freeze execution");
            throw;
        }
    }

    public async Task<List<LaoFreezeRecordDto>> GetLaoFreezeRecordsAsync(string regionCode, string? contractNumber = null, string? salesOrg = null)
    {
        try
        {
            var region = await _regionProvider.GetRegionByCodeAsync(regionCode);
            if (region == null || !region.IsActive)
            {
                throw new InvalidOperationException($"Invalid or inactive region: {regionCode}");
            }

            var connection = await CreateConnectionAsync(region.ConnectionString);

            await using (connection)
            {
                //await connection.OpenAsync();

                var query = "SELECT ContractNumber, Sales_Org, Short_Code, BlockDate, ReleaseDate, Created_Datetime FROM tb_SPARTA_PriceIncrease WHERE 1=1";
                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(contractNumber))
                {
                    query += " AND ContractNumber = @ContractNumber";
                    parameters.Add(new SqlParameter("@ContractNumber", contractNumber));
                }

                if (!string.IsNullOrEmpty(salesOrg))
                {
                    query += " AND Sales_Org = @SalesOrg";
                    parameters.Add(new SqlParameter("@SalesOrg", salesOrg));
                }

                using (var command = connection.Connection.CreateCommand())
                {
                    command.CommandText = query;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    var records = new List<LaoFreezeRecordDto>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            records.Add(new LaoFreezeRecordDto
                            {
                                ContractNumber = reader["ContractNumber"].ToString(),
                                SalesOrganization = reader["Sales_Org"].ToString(),
                                ShortCode = reader["Short_Code"].ToString(),
                                BlockDate = (DateTime)reader["BlockDate"],
                                ReleaseDate = (DateTime)reader["ReleaseDate"],
                                CreatedDateTime = (DateTime)reader["Created_Datetime"],
                                FreezeType = !string.IsNullOrEmpty(reader["ContractNumber"].ToString()) ? "PA" : "Product"
                            });
                        }
                    }

                    return records;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving LAO Freeze records for region: {regionCode}");
            throw;
        }
    }

    public async Task<List<LaoFreezeRecordDto>> GetActiveFrozenItemsAsync(string regionCode)
    {
        try
        {
            var region = await _regionProvider.GetRegionByCodeAsync(regionCode);
            if (region == null || !region.IsActive)
            {
                throw new InvalidOperationException($"Invalid or inactive region: {regionCode}");
            }

            var connection = await CreateConnectionAsync(region.ConnectionString);

            await using (connection)
            {
                //await connection.OpenAsync();

                var query = "SELECT ContractNumber, Sales_Org, Short_Code, BlockDate, ReleaseDate, Created_Datetime FROM tb_SPARTA_PriceIncrease WHERE BlockDate <= @CurrentDate AND ReleaseDate > @CurrentDate";

                using (var command = connection.Connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@CurrentDate", DateTime.UtcNow));

                    var records = new List<LaoFreezeRecordDto>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            records.Add(new LaoFreezeRecordDto
                            {
                                ContractNumber = reader["ContractNumber"].ToString(),
                                SalesOrganization = reader["Sales_Org"].ToString(),
                                ShortCode = reader["Short_Code"].ToString(),
                                BlockDate = (DateTime)reader["BlockDate"],
                                ReleaseDate = (DateTime)reader["ReleaseDate"],
                                CreatedDateTime = (DateTime)reader["Created_Datetime"],
                                FreezeType = !string.IsNullOrEmpty(reader["ContractNumber"].ToString()) ? "PA" : "Product"
                            });
                        }
                    }

                    return records;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving active frozen items for region: {regionCode}");
            throw;
        }
    }
    
    public async Task<SqlConnectionWrapper> CreateConnectionAsync(string connectionString)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return new SqlConnectionWrapper(connection);
    }
}