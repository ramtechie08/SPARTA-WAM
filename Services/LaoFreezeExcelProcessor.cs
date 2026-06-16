using ClosedXML.Excel;
using SPARTA_WAM.Models;

namespace SPARTA_WAM.Services;

/// <summary>
/// Processes Excel files for LAO Freeze/Block PA and Product Freeze operations.
/// </summary>
public interface ILaoFreezeExcelProcessor
{
    /// <summary>
    /// Processes a LAO Freeze/Block PA Excel file.
    /// </summary>
    Task<List<LaoFreezeRow>> ProcessLaoFreezeAsync(Stream fileStream);

    /// <summary>
    /// Processes a LAO Product Freeze Excel file.
    /// </summary>
    Task<List<LaoProductFreezeRow>> ProcessLaoProductFreezeAsync(Stream fileStream);
}

public class LaoFreezeExcelProcessor : ILaoFreezeExcelProcessor
{
    private readonly ILogger<LaoFreezeExcelProcessor> _logger;

    public LaoFreezeExcelProcessor(ILogger<LaoFreezeExcelProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<List<LaoFreezeRow>> ProcessLaoFreezeAsync(Stream fileStream)
    {
        var rows = new List<LaoFreezeRow>();

        try
        {
            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault()
                    ?? throw new InvalidOperationException("No worksheet found in Excel file");

                var headerRow = worksheet.FirstRowUsed()
                    ?? throw new InvalidOperationException("Worksheet is empty");

                var contractNoColumnIndex = FindColumnIndex(headerRow, "ContractNo");
                var blockDateColumnIndex = FindColumnIndex(headerRow, "BlockDate");
                var releaseDateColumnIndex = FindColumnIndex(headerRow, "ReleaseDate");
                var regionColumnIndex = FindColumnIndex(headerRow, "Region"); // NEW

                var dataRows = worksheet.RowsUsed().Skip(1);
                var processedContracts = new HashSet<string>();

                foreach (var row in dataRows)
                {
                    var contractNo = row.Cell(contractNoColumnIndex).GetString().Trim();
                    var blockDateText = row.Cell(blockDateColumnIndex).GetString().Trim();
                    var releaseDateText = row.Cell(releaseDateColumnIndex).GetString().Trim();
                    var region = row.Cell(regionColumnIndex).GetString().Trim(); // NEW

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(contractNo))
                        continue;

                    // Validate region
                    if (string.IsNullOrWhiteSpace(region))
                        throw new InvalidOperationException($"Region is required for contract: {contractNo}");

                    // Skip duplicates
                    if (processedContracts.Contains($"{contractNo}_{region}"))
                    {
                        _logger.LogWarning($"Duplicate contract-region combination found and skipped: {contractNo} for {region}");
                        continue;
                    }

                    // Parse dates
                    if (!DateTime.TryParse(blockDateText, out var blockDate))
                        throw new InvalidOperationException($"Invalid Block Date format: {blockDateText}");

                    if (!DateTime.TryParse(releaseDateText, out var releaseDate))
                        throw new InvalidOperationException($"Invalid Release Date format: {releaseDateText}");

                    rows.Add(new LaoFreezeRow
                    {
                        ContractNumber = contractNo,
                        BlockDate = blockDate,
                        ReleaseDate = releaseDate,
                        Region = region.ToUpper()
                    });

                    processedContracts.Add($"{contractNo}_{region}");
                }

                _logger.LogInformation($"Processed {rows.Count} LAO Freeze records from Excel file");
            }

            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process LAO Freeze Excel file");
            throw;
        }
    }

    public async Task<List<LaoProductFreezeRow>> ProcessLaoProductFreezeAsync(Stream fileStream)
    {
        var rows = new List<LaoProductFreezeRow>();

        try
        {
            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault()
                    ?? throw new InvalidOperationException("No worksheet found in Excel file");

                var headerRow = worksheet.FirstRowUsed()
                    ?? throw new InvalidOperationException("Worksheet is empty");

                var salesOrgColumnIndex = FindColumnIndex(headerRow, "SalesOrg");
                var blockDateColumnIndex = FindColumnIndex(headerRow, "BlockDate");
                var releaseDateColumnIndex = FindColumnIndex(headerRow, "ReleaseDate");
                var shortCodeColumnIndex = FindColumnIndex(headerRow, "Short_Code");
                var regionColumnIndex = FindColumnIndex(headerRow, "Region"); // NEW

                var dataRows = worksheet.RowsUsed().Skip(1);

                foreach (var row in dataRows)
                {
                    var salesOrg = row.Cell(salesOrgColumnIndex).GetString().Trim();
                    var blockDateText = row.Cell(blockDateColumnIndex).GetString().Trim();
                    var releaseDateText = row.Cell(releaseDateColumnIndex).GetString().Trim();
                    var shortCode = row.Cell(shortCodeColumnIndex).GetString().Trim();
                    var region = row.Cell(regionColumnIndex).GetString().Trim(); // NEW

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(salesOrg))
                        continue;

                    // Validate region
                    if (string.IsNullOrWhiteSpace(region))
                        throw new InvalidOperationException($"Region is required for sales org: {salesOrg}");

                    // Parse dates
                    if (!DateTime.TryParse(blockDateText, out var blockDate))
                        throw new InvalidOperationException($"Invalid Block Date format: {blockDateText}");

                    if (!DateTime.TryParse(releaseDateText, out var releaseDate))
                        throw new InvalidOperationException($"Invalid Release Date format: {releaseDateText}");

                    rows.Add(new LaoProductFreezeRow
                    {
                        SalesOrganization = salesOrg,
                        BlockDate = blockDate,
                        ReleaseDate = releaseDate,
                        ShortCode = shortCode,
                        Region = region.ToUpper()
                    });
                }

                _logger.LogInformation($"Processed {rows.Count} LAO Product Freeze records from Excel file");
            }

            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process LAO Product Freeze Excel file");
            throw;
        }
    }

    private static int FindColumnIndex(IXLRow headerRow, string columnName)
    {
        var columnIndex = headerRow.Cells()
            .FirstOrDefault(c => c.GetString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
            ?.Address.ColumnNumber
            ?? throw new InvalidOperationException($"Column '{columnName}' not found in Excel file");

        return columnIndex;
    }
}