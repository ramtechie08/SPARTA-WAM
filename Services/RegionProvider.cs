using SPARTA_WAM.Models;
using Microsoft.Extensions.Configuration;

namespace SPARTA_WAM.Services;

public class RegionProvider : IRegionProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegionProvider> _logger;
    private List<Region>? _cachedRegions;

    public RegionProvider(IConfiguration configuration, ILogger<RegionProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<Region>> GetAllRegionsAsync()
    {
        if (_cachedRegions != null)
            return _cachedRegions;

        try
        {
            _cachedRegions = new List<Region>
            {
                new Region
                {
                    RegionCode = "APAC",
                    RegionName = "Asia Pacific",
                    ConnectionString = _configuration.GetConnectionString("SpartaDatabase_APAC") ?? throw new InvalidOperationException("Missing connection string for APAC"),
                    IsActive = true
                },
                new Region
                {
                    RegionCode = "EMEA",
                    RegionName = "Europe, Middle East, Africa",
                    ConnectionString = _configuration.GetConnectionString("SpartaDatabase_EMEA") ?? throw new InvalidOperationException("Missing connection string for EMEA"),
                    IsActive = true
                },
                new Region
                {
                    RegionCode = "LAO",
                    RegionName = "Latin America Operations",
                    ConnectionString = _configuration.GetConnectionString("SpartaDatabase_LAO") ?? throw new InvalidOperationException("Missing connection string for LAO"),
                    IsActive = true
                },
                //new Region
                //{
                //    RegionCode = "NA",
                //    RegionName = "North America",
                //    ConnectionString = _configuration.GetConnectionString("SpartaDatabase_NA") ?? throw new InvalidOperationException("Missing connection string for NA"),
                //    IsActive = true
                //}
            };

            _logger.LogInformation($"Loaded {_cachedRegions.Count} regions");
            return _cachedRegions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading regions");
            throw;
        }
    }

    public async Task<Region?> GetRegionByCodeAsync(string regionCode)
    {
        var regions = await GetAllRegionsAsync();
        return regions.FirstOrDefault(r => r.RegionCode.Equals(regionCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> IsValidRegionAsync(string regionCode)
    {
        var region = await GetRegionByCodeAsync(regionCode);
        return region?.IsActive ?? false;
    }
}