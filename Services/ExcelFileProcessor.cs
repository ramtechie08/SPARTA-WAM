using ClosedXML.Excel;
using SPARTA_WAM.Models;

namespace SPARTA_WAM.Services;

public interface IExcelFileProcessor
{
    Task<List<WalkAwayMarginRow>> ProcessAsync(Stream fileStream);
}

public class ExcelFileProcessor : IExcelFileProcessor
{
    private const string ShortCodeColumn = "Short_Code";
    private const string WalkAwayMarginColumn = "WalkAwayMargin";
    private const string SalesOrgColumn = "Sales_Org";
    private const string TypeColumn = "Channel (Direct/Indirect)";

    public async Task<List<WalkAwayMarginRow>> ProcessAsync(Stream fileStream)
    {
        var rows = new List<WalkAwayMarginRow>();

        try
        {
            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault()
                    ?? throw new InvalidOperationException("No worksheet found in Excel file");

                var headerRow = worksheet.FirstRowUsed()
                    ?? throw new InvalidOperationException("Worksheet is empty");

                var shortCodeColumnIndex = FindColumnIndex(headerRow, ShortCodeColumn);
                var wamColumnIndex = FindColumnIndex(headerRow, WalkAwayMarginColumn);
                var salesOrgColumnIndex = FindColumnIndex(headerRow, SalesOrgColumn);
                var typeColumnIndex = FindColumnIndex(headerRow, TypeColumn);

                var dataRows = worksheet.RowsUsed().Skip(1);

                foreach (var row in dataRows)
                {
                    var shortCode = row.Cell(shortCodeColumnIndex).GetString().Trim();
                    var marginValueText = row.Cell(wamColumnIndex).GetString().Trim();
                    var salesOrg = row.Cell(salesOrgColumnIndex).GetString().Trim();
                    var type = row.Cell(typeColumnIndex).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(shortCode) || string.IsNullOrWhiteSpace(marginValueText))
                        continue;

                    var marginValue = ParseMarginValue(marginValueText);

                    rows.Add(new WalkAwayMarginRow
                    {
                        CustomerShortCode = shortCode,
                        MarginValue = marginValue,
                        SalesOrg = salesOrg,
                        PricingType = type
                    });
                }
            }

            return rows;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to process Excel file", ex);
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

    private static decimal ParseMarginValue(string value)
    {
        var cleanValue = value.Replace("%", string.Empty).Trim();

        if (!decimal.TryParse(cleanValue, out var marginValue))
        {
            throw new InvalidOperationException($"Invalid margin value: '{value}'. Must be a valid decimal number.");
        }

        return marginValue*100;
    }
}