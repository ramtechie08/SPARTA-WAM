using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SPARTA_WAM.Data;
using SPARTA_WAM.Data.Models;
using SPARTA_WAM.Data.Resilience;

namespace SPARTA_WAM.Data.Repositories;

public class WalkAwayMarginRepository : IWalkAwayMarginRepository
{
    private readonly SpartaDbContext _context;
    private readonly IResiliencePolicyProvider _resiliencePolicy;
    private readonly ILogger<WalkAwayMarginRepository> _logger;

    public WalkAwayMarginRepository(
        SpartaDbContext context,
        IResiliencePolicyProvider resiliencePolicy,
        ILogger<WalkAwayMarginRepository> logger)
    {
        _context = context;
        _resiliencePolicy = resiliencePolicy;
        _logger = logger;
    }

    public async Task<SpartaWalkAwayMargin?> GetByKeyAsync(string shortCode, string salesOrg, string directPricing)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<SpartaWalkAwayMargin?>();
            return await policy.ExecuteAsync(async () =>
            {
                return await _context.WalkAwayMargins
                    .FirstOrDefaultAsync(w => w.ShortCode == shortCode &&
                                            w.SalesOrg == salesOrg &&
                                            w.DirectPricing == directPricing);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving WAM for shortCode: {shortCode}, salesOrg: {salesOrg}, directPricing: {directPricing}");
            throw;
        }
    }

    public async Task<List<SpartaWalkAwayMargin>> GetAllAsync()
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<List<SpartaWalkAwayMargin>>();
            return await policy.ExecuteAsync(async () =>
            {
                return await _context.WalkAwayMargins.ToListAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all WAM records");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string shortCode, string salesOrg, string directPricing)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<bool>();
            return await policy.ExecuteAsync(async () =>
            {
                return await _context.WalkAwayMargins
                    .AnyAsync(w => w.ShortCode == shortCode &&
                                  w.SalesOrg == salesOrg &&
                                  w.DirectPricing == directPricing);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking WAM existence for shortCode: {shortCode}");
            throw;
        }
    }

    public async Task CreateAsync(SpartaWalkAwayMargin wam)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<bool>();
            await policy.ExecuteAsync(async () =>
            {
                wam.CreatedDateTime = DateTime.UtcNow;
                wam.UpdatedDateTime = DateTime.UtcNow;

                _context.WalkAwayMargins.Add(wam);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created WAM record for shortCode: {wam.ShortCode}");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating WAM record for shortCode: {wam.ShortCode}");
            throw;
        }
    }

    public async Task UpdateAsync(SpartaWalkAwayMargin wam)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<bool>();
            await policy.ExecuteAsync(async () =>
            {
                var existing = await _context.WalkAwayMargins
                    .FirstOrDefaultAsync(w => w.ShortCode == wam.ShortCode &&
                                            w.SalesOrg == wam.SalesOrg &&
                                            w.DirectPricing == wam.DirectPricing)
                    ?? throw new InvalidOperationException($"WAM record not found for shortCode: {wam.ShortCode}");

                existing.WalkAwayMargin = wam.WalkAwayMargin;
                existing.UpdatedDateTime = DateTime.UtcNow;

                _context.WalkAwayMargins.Update(existing);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated WAM record for shortCode: {wam.ShortCode}");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating WAM record for shortCode: {wam.ShortCode}");
            throw;
        }
    }

    public async Task DeleteAsync(string shortCode, string salesOrg, string directPricing)
    {
        try
        {
            var policy = _resiliencePolicy.GetCombinedPolicy<bool>();
            await policy.ExecuteAsync(async () =>
            {
                var existing = await _context.WalkAwayMargins
                    .FirstOrDefaultAsync(w => w.ShortCode == shortCode &&
                                            w.SalesOrg == salesOrg &&
                                            w.DirectPricing == directPricing)
                    ?? throw new InvalidOperationException($"WAM record not found for deletion");

                _context.WalkAwayMargins.Remove(existing);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted WAM record for shortCode: {shortCode}");
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting WAM record for shortCode: {shortCode}");
            throw;
        }
    }
}