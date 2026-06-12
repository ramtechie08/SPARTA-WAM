namespace SPARTA_WAM.Data.Services;

public interface ISqlExecutionService
{
    Task<int> ExecuteSqlAsync(string sqlScript);
    Task<List<T>> ExecuteQueryAsync<T>(string sqlQuery, Func<dynamic, T> mapper);
}