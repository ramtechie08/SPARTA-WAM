using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SPARTA_WAM.Data.Connections;
using SPARTA_WAM.Data.Resilience;

namespace SPARTA_WAM.Data.Services;

public class SqlExecutionService : ISqlExecutionService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IResiliencePolicyProvider _resiliencePolicy;
    private readonly ILogger<SqlExecutionService> _logger;

    public SqlExecutionService(
        ISqlConnectionFactory connectionFactory,
        IResiliencePolicyProvider resiliencePolicy,
        ILogger<SqlExecutionService> logger)
    {
        _connectionFactory = connectionFactory;
        _resiliencePolicy = resiliencePolicy;
        _logger = logger;
    }

    public async Task<int> ExecuteSqlAsync(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            throw new ArgumentException("SQL script cannot be empty", nameof(sqlScript));

        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<int>();
            return await policy.ExecuteAsync(async () =>
            {
                await using (var connectionWrapper = await _connectionFactory.CreateConnectionAsync())
                {
                    using (var command = new SqlCommand(sqlScript, connectionWrapper.Connection))
                    {
                        command.CommandTimeout = 300; // 5 minutes
                        var result = await command.ExecuteNonQueryAsync();
                        _logger.LogInformation($"SQL script executed successfully. Rows affected: {result}");
                        return result;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL script");
            throw;
        }
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, Func<dynamic, T> mapper)
    {
        if (string.IsNullOrWhiteSpace(sqlQuery))
            throw new ArgumentException("SQL query cannot be empty", nameof(sqlQuery));

        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<List<T>>();
            return await policy.ExecuteAsync(async () =>
            {
                var results = new List<T>();

                await using (var connectionWrapper = await _connectionFactory.CreateConnectionAsync())
                {
                    using (var command = new SqlCommand(sqlQuery, connectionWrapper.Connection))
                    {
                        command.CommandTimeout = 300;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = ConvertToExpandoObject(reader);
                                results.Add(mapper(row));
                            }
                        }
                    }
                }

                _logger.LogInformation($"Query executed successfully. Rows retrieved: {results.Count}");
                return results;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL query");
            throw;
        }
    }

    private static dynamic ConvertToExpandoObject(SqlDataReader reader)
    {
        dynamic expandoObject = new System.Dynamic.ExpandoObject();
        var expandoDictionary = (IDictionary<string, object>)expandoObject;

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var value = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            expandoDictionary.Add(reader.GetName(i), value);
        }

        return expandoObject;
    }
}   