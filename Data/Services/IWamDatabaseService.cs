using SPARTA_WAM.Models;

namespace SPARTA_WAM.Data.Services;

public interface IWamDatabaseService
{
    Task<int> ExecuteWamScriptsAsync(List<SqlScriptResult> scripts);
    Task<List<WalkAwayMarginDto>> GetWamRecordsAsync(string? salesOrg = null, string? pricingType = null);
}

public class WalkAwayMarginDto
{
    public required string ShortCode { get; set; }
    public decimal MarginValue { get; set; }
    public required string SalesOrganization { get; set; }
    public required string PricingType { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime UpdatedDateTime { get; set; }
}