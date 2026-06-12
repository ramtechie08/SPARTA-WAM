using Microsoft.Data.SqlClient;

namespace SPARTA_WAM.Data.Connections;

public class SqlConnectionWrapper : IAsyncDisposable
{
    private readonly SqlConnection _connection;

    public SqlConnectionWrapper(SqlConnection connection)
    {
        _connection = connection;
    }

    public SqlConnection Connection => _connection;

    public async Task OpenAsync()
    {
        await _connection.OpenAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.State == System.Data.ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
        await _connection.DisposeAsync();
    }
}