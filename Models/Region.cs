namespace SPARTA_WAM.Models;

/// <summary>
/// Represents a region configuration.
/// </summary>
public class Region
{
    public required string RegionCode { get; set; } // "APAC", "EMEA", "LAO", "NA"
    public required string RegionName { get; set; }
    public required string ConnectionString { get; set; }
    public bool IsActive { get; set; } = true;
}