using SPARTA_WAM.Models;

namespace SPARTA_WAM.Services;

public interface IRegionProvider
{
    /// <summary>
    /// Gets all available regions.
    /// </summary>
    Task<List<Region>> GetAllRegionsAsync();

    /// <summary>
    /// Gets a specific region by code.
    /// </summary>
    Task<Region?> GetRegionByCodeAsync(string regionCode);

    /// <summary>
    /// Validates if a region code is valid.
    /// </summary>
    Task<bool> IsValidRegionAsync(string regionCode);
}