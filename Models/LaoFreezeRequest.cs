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
/// LAO Freeze/Block PA request with region information.
/// </summary>
public class LaoFreezeRow
{
    public required string ContractNumber { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public required string Region { get; set; } // NEW: Region identifier
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
/// LAO Product Freeze request with region information.
/// </summary>
public class LaoProductFreezeRow
{
    public required string SalesOrganization { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public required string ShortCode { get; set; }
    public required string Region { get; set; } // NEW: Region identifier
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
    public required string Region { get; set; } // NEW: Region identifier (APAC, EMEA, LAO, NA)
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

/// <summary>
/// Request to execute LAO Freeze scripts for a specific region.
/// </summary>
public class ExecuteLaoFreezeRequest
{
    public required string RegionCode { get; set; }
    public required List<string> SqlScripts { get; set; }
}

/// <summary>
/// Response for upload and execute operations.
/// </summary>
public class LaoFreezeUploadExecuteResponse
{
    public bool Success { get; set; }
    public string FreezeType { get; set; } = string.Empty; // "PA" or "Product"
    public string Message { get; set; } = string.Empty;
    public int ScriptCount { get; set; }
    public Dictionary<string, int> RowsAffectedByRegion { get; set; } = new();
    public List<LaoFreezeExecutionDetail> Details { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Details of each freeze record executed.
/// </summary>
public class LaoFreezeExecutionDetail
{
    public string? ContractNumber { get; set; }
    public string? SalesOrganization { get; set; }
    public string? ShortCode { get; set; }
    public DateTime BlockDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Region { get; set; } = string.Empty;
    public string FreezeType { get; set; } = string.Empty;
}