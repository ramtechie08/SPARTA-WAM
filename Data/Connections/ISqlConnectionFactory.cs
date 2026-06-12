namespace SPARTA_WAM.Data.Connections;

public interface ISqlConnectionFactory
{
    Task<SqlConnectionWrapper> CreateConnectionAsync();
}