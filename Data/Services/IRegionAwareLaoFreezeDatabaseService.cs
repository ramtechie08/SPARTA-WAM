using SPARTA_WAM.Models;

namespace SPARTA_WAM.Data.Services;

public interface IRegionAwareLaoFreezeDatabaseService
{
    /// <summary>
    /// Executes LAO Freeze scripts for a specific region.
    /// </summary>
    Task<int> ExecuteLaoFreezeScriptsAsync(List<LaoFreezeScriptResult> scripts, string regionCode);

    /// <summary>
    /// Executes LAO Freeze scripts for multiple regions.
    /// </summary>
    Task<Dictionary<string, int>> ExecuteLaoFreezeScriptsMultiRegionAsync(List<LaoFreezeScriptResult> scripts);

    /// <summary>
    /// Gets LAO Freeze records for a specific region.
    /// </summary>
    Task<List<LaoFreezeRecordDto>> GetLaoFreezeRecordsAsync(string regionCode, string? contractNumber = null, string? salesOrg = null);

    /// <summary>
    /// Gets active frozen items for a specific region.
    /// </summary>
    Task<List<LaoFreezeRecordDto>> GetActiveFrozenItemsAsync(string regionCode);
}