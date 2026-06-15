using SPARTA_WAM.Models;

namespace SPARTA_WAM.Services;

/// <summary>
/// Generates SQL scripts for LAO Freeze/Block PA and Product Freeze operations.
/// </summary>
public interface ILaoFreezeSqlScriptGenerator
{
    List<LaoFreezeScriptResult> GenerateLaoFreezeScripts(List<LaoFreezeRow> rows);
    List<LaoFreezeScriptResult> GenerateLaoProductFreezeScripts(List<LaoProductFreezeRow> rows);
    string FormatScripts(List<LaoFreezeScriptResult> scripts);
}

public class LaoFreezeSqlScriptGenerator : ILaoFreezeSqlScriptGenerator
{
    private const string TableName = "tb_SPARTA_PriceIncrease";
    private const string ContractNumberPrefix = "00";
    private const string ShortCodePrefix = "0000000000";
    private readonly ILogger<LaoFreezeSqlScriptGenerator> _logger;

    public LaoFreezeSqlScriptGenerator(ILogger<LaoFreezeSqlScriptGenerator> logger)
    {
        _logger = logger;
    }

    public List<LaoFreezeScriptResult> GenerateLaoFreezeScripts(List<LaoFreezeRow> rows)
    {
        var scripts = new List<LaoFreezeScriptResult>();

        foreach (var row in rows)
        {
            var fullContractNumber = $"{ContractNumberPrefix}{row.ContractNumber}";

            var blockDateFormatted = row.BlockDate.ToString("M/d/yyyy");
            var releaseDateFormatted = row.ReleaseDate.ToString("M/d/yyyy");

            var sqlStatement = $@"insert into {TableName}
(ContractNo, SalesOrg, BlockDate, ReleaseDate, Short_Code, Updated_Date)
values
(
    '{fullContractNumber}',
    '',
    CAST('{blockDateFormatted}' as datetime),
    CAST('{releaseDateFormatted}' as datetime),
    '',
    GETUTCDATE()
)";

            scripts.Add(new LaoFreezeScriptResult
            {
                SqlStatement = sqlStatement,
                ContractNumber = row.ContractNumber,
                BlockDate = row.BlockDate,
                ReleaseDate = row.ReleaseDate,
                FreezeType = "PA"
            });
        }

        _logger.LogInformation($"Generated {scripts.Count} LAO Freeze scripts");
        return scripts;
    }

    public List<LaoFreezeScriptResult> GenerateLaoProductFreezeScripts(List<LaoProductFreezeRow> rows)
    {
        var scripts = new List<LaoFreezeScriptResult>();

        foreach (var row in rows)
        {
            var fullShortCode = $"{ShortCodePrefix}{row.ShortCode}";

            var blockDateFormatted = row.BlockDate.ToString("M/d/yyyy");
            var releaseDateFormatted = row.ReleaseDate.ToString("M/d/yyyy");

            var sqlStatement = $@"insert into {TableName}
(ContractNo, SalesOrg, BlockDate, ReleaseDate, Short_Code, Updated_Date)
values
(
    '',
    '{row.SalesOrganization}',
    CAST('{blockDateFormatted}' as datetime),
    CAST('{releaseDateFormatted}' as datetime),
    '{fullShortCode}',
    GETUTCDATE()
)";

            scripts.Add(new LaoFreezeScriptResult
            {
                SqlStatement = sqlStatement,
                SalesOrganization = row.SalesOrganization,
                ShortCode = row.ShortCode,
                BlockDate = row.BlockDate,
                ReleaseDate = row.ReleaseDate,
                FreezeType = "Product"
            });
        }

        _logger.LogInformation($"Generated {scripts.Count} LAO Product Freeze scripts");
        return scripts;
    }

    public string FormatScripts(List<LaoFreezeScriptResult> scripts)
    {
        if (scripts.Count == 0)
            return string.Empty;

        var scriptLines = scripts.Select(s => s.SqlStatement);
        return string.Join("\n\nGO\n\n", scriptLines);
    }
}