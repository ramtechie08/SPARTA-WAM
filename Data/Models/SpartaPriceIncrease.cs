namespace SPARTA_WAM.Data.Models;

/// <summary>
/// Represents a record in the SPARTA Price Increase table used for freeze/block operations.
/// </summary>
public class SpartaPriceIncrease
{
    public string? ContractNumber { get; set; }
    public string? SalesOrganization { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? ShortCode { get; set; }
    public DateTime CreatedDateTime { get; set; }
}