using SPARTA_WAM.Data.Models;

namespace SPARTA_WAM.Data.Repositories;

/// <summary>
/// Repository for LAO Freeze/Block PA and Product Freeze operations.
/// </summary>
public interface ILaoFreezeRepository
{
    Task<List<SpartaPriceIncrease>> GetAllAsync();
    Task<List<SpartaPriceIncrease>> GetByContractNumberAsync(string contractNumber);
    Task<List<SpartaPriceIncrease>> GetBySalesOrgAsync(string salesOrg);
    Task<List<SpartaPriceIncrease>> GetByShortCodeAsync(string shortCode);
    Task<List<SpartaPriceIncrease>> GetActiveFrozenItemsAsync(DateTime currentDate);
    Task CreateAsync(SpartaPriceIncrease freeze);
    Task CreateBatchAsync(List<SpartaPriceIncrease> freezes);
    Task DeleteAsync(string contractNumber);
}