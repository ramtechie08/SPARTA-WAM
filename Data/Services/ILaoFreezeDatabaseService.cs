using SPARTA_WAM.Models;

namespace SPARTA_WAM.Data.Services;

/// <summary>
/// Database service for LAO Freeze/Block PA and Product Freeze operations.
/// </summary>
public interface ILaoFreezeDatabaseService
{
    Task<int> ExecuteLaoFreezeScriptsAsync(List<LaoFreezeScriptResult> scripts);
    Task<List<LaoFreezeRecordDto>> GetLaoFreezeRecordsAsync(string? contractNumber = null, string? salesOrg = null);
    Task<List<LaoFreezeRecordDto>> GetActiveFrozenItemsAsync();
}