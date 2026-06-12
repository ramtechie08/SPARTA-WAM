using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace SPARTA_WAM.Data.Resilience;

public interface IResiliencePolicyProvider
{
    IAsyncPolicy<T> GetRetryPolicy<T>();
    IAsyncPolicy<T> GetCircuitBreakerPolicy<T>();
    IAsyncPolicy<T> GetCombinedPolicy<T>();
}

public class ResiliencePolicyProvider : IResiliencePolicyProvider
{
    private readonly ILogger<ResiliencePolicyProvider> _logger;

    public ResiliencePolicyProvider(ILogger<ResiliencePolicyProvider> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<T> GetRetryPolicy<T>()
    {
        return Policy
            .Handle<SqlException>(ex => IsTransientError(ex))
            .Or<TimeoutException>()
            .OrResult<T>(r => r == null)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100),
                onRetry: (outcome, timespan, attempt, context) =>
                {
                    _logger.LogWarning($"Retry attempt {attempt} after {timespan.TotalMilliseconds}ms");
                });
    }

    public IAsyncPolicy<T> GetCircuitBreakerPolicy<T>()
    {
        return Policy
            .Handle<SqlException>(ex => IsTransientError(ex))
            .Or<TimeoutException>()
            .OrResult<T>(r => r == null)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogError($"Circuit breaker opened for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                });
    }

    public IAsyncPolicy<T> GetCombinedPolicy<T>()
    {
        return Policy.WrapAsync(GetRetryPolicy<T>(), GetCircuitBreakerPolicy<T>());
    }

    private static bool IsTransientError(SqlException ex)
    {
        // Transient error numbers for SQL Server
        var transientErrorNumbers = new[] { -2, 2, 18456, 64, 233, 20 };
        return transientErrorNumbers.Contains(ex.Number) ||
               ex.InnerException is TimeoutException;
    }
}