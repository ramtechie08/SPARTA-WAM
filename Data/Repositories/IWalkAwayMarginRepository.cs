using SPARTA_WAM.Data.Models;

namespace SPARTA_WAM.Data.Repositories;

public interface IWalkAwayMarginRepository
{
    Task<SpartaWalkAwayMargin?> GetByKeyAsync(string shortCode, string salesOrg, string directPricing);
    Task<List<SpartaWalkAwayMargin>> GetAllAsync();
    Task<bool> ExistsAsync(string shortCode, string salesOrg, string directPricing);
    Task CreateAsync(SpartaWalkAwayMargin wam);
    Task UpdateAsync(SpartaWalkAwayMargin wam);
    Task DeleteAsync(string shortCode, string salesOrg, string directPricing);
}