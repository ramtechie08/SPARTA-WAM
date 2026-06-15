using Microsoft.EntityFrameworkCore;
using SPARTA_WAM.Data;
using SPARTA_WAM.Data.Models;
using SPARTA_WAM.Data.Resilience;

namespace SPARTA_WAM.Data.Repositories;

public class LaoFreezeRepository : ILaoFreezeRepository
{
    private readonly SpartaDbContext _context;
    private readonly IResiliencePolicyProvider _resiliencePolicy;
    private readonly ILogger<LaoFreezeRepository> _logger;

    public LaoFreezeRepository(
        SpartaDbContext context,
        IResiliencePolicyProvider resiliencePolicy,
        ILogger<LaoFreezeRepository> logger)
    {
        _context = context;
        _resiliencePolicy = resiliencePolicy;
        _logger = logger;
    }

    public async Task<List<SpartaPriceIncrease>> GetAllAsync()
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<List<SpartaPriceIncrease>>();
            return await policy.ExecuteAsync(async () =>
            {
                // Note: This assumes you've added DbSet for SpartaPriceIncrease to SpartaDbContext
                var results = await _context.Set<SpartaPriceIncrease>().ToListAsync();
                _logger.LogInformation($"Retrieved {results.Count} LAO Freeze records");
                return results;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all LAO Freeze records");
            throw;
        }
    }

    public async Task<List<SpartaPriceIncrease>> GetByContractNumberAsync(string contractNumber)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<List<SpartaPriceIncrease>>();
            return await policy.ExecuteAsync(async () =>
            {
                var results = await _context.Set<SpartaPriceIncrease>()
                    .Where(x => x.ContractNumber == contractNumber)
                    .ToListAsync();
                _logger.LogInformation($"Retrieved {results.Count} records for contract: {contractNumber}");
                return results;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving LAO Freeze records for contract: {contractNumber}");
            throw;
        }
    }

    public async Task<List<SpartaPriceIncrease>> GetBySalesOrgAsync(string salesOrg)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<List<SpartaPriceIncrease>>();
            return await policy.ExecuteAsync(async () =>
            {
                var results = await _context.Set<SpartaPriceIncrease>()
                    .Where(x => x.SalesOrganization == salesOrg)
                    .ToListAsync();
                _logger.LogInformation($"Retrieved {results.Count} records for sales org: {salesOrg}");
                return results;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving LAO Freeze records for sales org: {salesOrg}");
            throw;
        }
    }

    public async Task<List<SpartaPriceIncrease>> GetByShortCodeAsync(string shortCode)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<List<SpartaPriceIncrease>>();
            return await policy.ExecuteAsync(async () =>
            {
                var results = await _context.Set<SpartaPriceIncrease>()
                    .Where(x => x.ShortCode == shortCode)
                    .ToListAsync();
                _logger.LogInformation($"Retrieved {results.Count} records for short code: {shortCode}");
                return results;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving LAO Freeze records for short code: {shortCode}");
            throw;
        }
    }

    public async Task<List<SpartaPriceIncrease>> GetActiveFrozenItemsAsync(DateTime currentDate)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<List<SpartaPriceIncrease>>();
            return await policy.ExecuteAsync(async () =>
            {
                var results = await _context.Set<SpartaPriceIncrease>()
                    .Where(x => x.BlockDate <= currentDate && currentDate <= x.ReleaseDate)
                    .ToListAsync();
                _logger.LogInformation($"Retrieved {results.Count} active frozen items as of {currentDate:yyyy-MM-dd}");
                return results;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active frozen items");
            throw;
        }
    }

    public async Task CreateAsync(SpartaPriceIncrease freeze)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<bool>();
            await policy.ExecuteAsync(async () =>
            {
                freeze.CreatedDateTime = DateTime.UtcNow;
                _context.Set<SpartaPriceIncrease>().Add(freeze);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created LAO Freeze record");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LAO Freeze record");
            throw;
        }
    }

    public async Task CreateBatchAsync(List<SpartaPriceIncrease> freezes)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<bool>();
            await policy.ExecuteAsync(async () =>
            {
                foreach (var freeze in freezes)
                {
                    freeze.CreatedDateTime = DateTime.UtcNow;
                }

                _context.Set<SpartaPriceIncrease>().AddRange(freezes);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created batch of {freezes.Count} LAO Freeze records");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch of LAO Freeze records");
            throw;
        }
    }

    public async Task DeleteAsync(string contractNumber)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<bool>();
            await policy.ExecuteAsync(async () =>
            {
                var record = await _context.Set<SpartaPriceIncrease>()
                    .FirstOrDefaultAsync(x => x.ContractNumber == contractNumber)
                    ?? throw new InvalidOperationException($"Record not found for contract: {contractNumber}");

                _context.Set<SpartaPriceIncrease>().Remove(record);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted LAO Freeze record for contract: {contractNumber}");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting LAO Freeze record for contract: {contractNumber}");
            throw;
        }
    }
}