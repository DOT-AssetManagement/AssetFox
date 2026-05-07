using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Transforms;

public sealed class TotalsRowTransform : IRowTransform
{
    public bool AppliesTo(ReportComponent component) => GetConfigBool(component, "showTotals");

    public (IReadOnlyList<ViewerColumn> Columns, IReadOnlyList<List<object>> Rows) Apply(
        ReportComponent component,
        IReadOnlyList<ViewerColumn> columns,
        IReadOnlyList<List<object>> rows)
    {
        if (columns.Count == 0 || rows.Count == 0)
        {
            return (columns, rows);
        }

        var totalRow = new List<object>(columns.Count);
        for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            var column = columns[columnIndex];
            if (columnIndex == 0)
            {
                totalRow.Add("Total");
                continue;
            }

            totalRow.Add(IsNumericColumn(column)
                ? rows.Sum(row => ToDecimal(row.ElementAtOrDefault(columnIndex)))
                : null!);
        }

        var transformedRows = rows.Select(row => row.ToList()).ToList();
        transformedRows.Add(totalRow);
        return (columns, transformedRows);
    }

    private static bool IsNumericColumn(ViewerColumn column) =>
        string.Equals(column.Format, "number", StringComparison.OrdinalIgnoreCase)
        || string.Equals(column.Format, "currency", StringComparison.OrdinalIgnoreCase)
        || string.Equals(column.Format, "percent", StringComparison.OrdinalIgnoreCase);

    private static decimal ToDecimal(object? value)
    {
        if (value == null || value == DBNull.Value)
        {
            return 0;
        }

        try
        {
            return Convert.ToDecimal(value);
        }
        catch
        {
            return 0;
        }
    }

    private static bool GetConfigBool(ReportComponent component, string key)
    {
        if (component.Config == null || !component.Config.TryGetValue(key, out var value) || value == null)
        {
            return false;
        }

        return value is bool boolValue
            ? boolValue
            : bool.TryParse(value.ToString(), out var parsed) && parsed;
    }
}
