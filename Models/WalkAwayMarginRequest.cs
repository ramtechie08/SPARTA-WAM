namespace SPARTA_WAM.Models;

public class WalkAwayMarginRequest
{
    public required string ShortCode { get; set; }
    public decimal WalkAwayMargin { get; set; }
    public required string SalesOrganization { get; set; }
    public required string Type { get; set; }
}