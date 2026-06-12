namespace SPARTA_WAM.Data.Models;

public class SpartaWalkAwayMargin
{
    public string ShortCode { get; set; } = string.Empty;
    public decimal WalkAwayMargin { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string SalesOrg { get; set; } = string.Empty;
    public string DirectPricing { get; set; } = string.Empty;
    public DateTime UpdatedDateTime { get; set; }
}