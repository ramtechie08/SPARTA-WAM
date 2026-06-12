using SPARTA_WAM.Models;

namespace SPARTA_WAM.Services;

public interface ISqlScriptGenerator
{
    List<SqlScriptResult> GenerateScripts(List<WalkAwayMarginRow> rows);
    string FormatScript(List<SqlScriptResult> scripts);
}

public class SqlScriptGenerator : ISqlScriptGenerator
{
    private const string ShortCodePrefix = "0000000000";
    private const string TableName = "tb_SPARTA_WalkAwayfloorMargin_Mst";

    public List<SqlScriptResult> GenerateScripts(List<WalkAwayMarginRow> rows)
    {
        return rows.Select(row => GenerateSingleScript(row)).ToList();
    }

    public string FormatScript(List<SqlScriptResult> scripts)
    {
        if (scripts.Count == 0)
            return string.Empty;

        var scriptLines = scripts.Select(s => s.SqlStatement);
        return string.Join("\n\n", scriptLines);
    }

    private static SqlScriptResult GenerateSingleScript(WalkAwayMarginRow row)
    {
        var fullShortCode = $"{ShortCodePrefix}{row.CustomerShortCode}";
        var pricingType = row.PricingType.ToUpper() == "D" ? "D" : "I";

        var sqlStatement = $@"if exists(
    select *
    from {TableName}
    where Sales_Org='{row.SalesOrg}'
    and Direct_Pricing='{pricingType}'
    and Short_Code='{fullShortCode}'
)
begin
    update {TableName}
    set WalkAwayMargin={row.MarginValue},
    Updated_Datetime=getutcdate()
    where Sales_Org='{row.SalesOrg}'
    and Direct_Pricing='{pricingType}'
    and Short_Code='{fullShortCode}'
end
else
begin
    insert into {TableName}
    values
    (
        '{fullShortCode}',
        {row.MarginValue},
        getutcdate(),
        '{row.SalesOrg}',
        '{pricingType}'
    )
end";

        return new SqlScriptResult
        {
            SqlStatement = sqlStatement,
            ShortCode = row.CustomerShortCode,
            MarginValue = row.MarginValue,
            SalesOrganization = row.SalesOrg,
            PricingType = pricingType
        };
    }
}