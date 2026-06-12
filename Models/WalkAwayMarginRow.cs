namespace SPARTA_WAM.Models;

public class WalkAwayMarginRow
{
    public required string CustomerShortCode { get; set; }
    public decimal MarginValue { get; set; }
    public required string SalesOrg { get; set; }
    public required string PricingType { get; set; }
}