namespace SPARTA_WAM.Models;

public class SqlScriptResult
{
    public required string SqlStatement { get; set; }
    public required string ShortCode { get; set; }
    public decimal MarginValue { get; set; }
    public required string SalesOrganization { get; set; }
    public required string PricingType { get; set; }
}