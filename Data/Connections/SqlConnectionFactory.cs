using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SPARTA_WAM.Data.Connections;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<SqlConnectionFactory> _logger;

    public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
    {
        _connectionString = configuration.GetConnectionString("SpartaDatabase")
            ?? throw new InvalidOperationException("Connection string 'SpartaDatabase' not found in configuration");
        _logger = logger;
    }

    public async Task<SqlConnectionWrapper> CreateConnectionAsync()
    {
        try
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("Database connection established successfully");
            return new SqlConnectionWrapper(connection);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to establish database connection");
            throw;
        }
    }
}