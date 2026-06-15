namespace SPARTA_WAM.Models;

/// <summary>
/// Represents a LAO Freeze/Block PA request for a contract.
/// </summary>
public class LaoFreezeRequest
{
    public required string ContractNumber { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
}

/// <summary>
/// Represents a LAO Freeze/Block PA row from Excel.
/// </summary>
public class LaoFreezeRow
{
    public required string ContractNumber { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
}

/// <summary>
/// Represents a LAO Product Freeze request.
/// </summary>
public class LaoProductFreezeRequest
{
    public required string SalesOrganization { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public required string ShortCode { get; set; }
}

/// <summary>
/// Represents a LAO Product Freeze row from Excel.
/// </summary>
public class LaoProductFreezeRow
{
    public required string SalesOrganization { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public required string ShortCode { get; set; }
}

/// <summary>
/// Result of LAO Freeze/Block PA or Product Freeze operation.
/// </summary>
public class LaoFreezeScriptResult
{
    public required string SqlStatement { get; set; }
    public string? ContractNumber { get; set; }
    public string? SalesOrganization { get; set; }
    public string? ShortCode { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public required string FreezeType { get; set; } // "PA" or "Product"
}

/// <summary>
/// DTO for LAO Freeze records from database.
/// </summary>
public class LaoFreezeRecordDto
{
    public string? ContractNumber { get; set; }
    public string? SalesOrganization { get; set; }
    public string? ShortCode { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string FreezeType { get; set; } = string.Empty;
}