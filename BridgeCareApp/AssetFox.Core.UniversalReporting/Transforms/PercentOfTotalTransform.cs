using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Transforms;

public sealed class PercentOfTotalTransform : IRowTransform
{
    public bool AppliesTo(ReportComponent component) =>
        GetConfigBool(component, "includePercentages")
        && !string.Equals(component.Type, "workSummary", StringComparison.OrdinalIgnoreCase);

    public (IReadOnlyList<ViewerColumn> Columns, IReadOnlyList<List<object>> Rows) Apply(
        ReportComponent component,
        IReadOnlyList<ViewerColumn> columns,
        IReadOnlyList<List<object>> rows)
    {
        var numericColumnIndexes = columns
            .Select((column, index) => (column, index))
            .Where(item => IsNumericColumn(item.column))
            .Select(item => item.index)
            .ToList();

        if (numericColumnIndexes.Count == 0 || rows.Count == 0)
        {
            return (columns, rows);
        }

        var totalsByColumn = numericColumnIndexes.ToDictionary(
            index => index,
            index => rows.Sum(row => ToDecimal(row.ElementAtOrDefault(index))));

        var transformedColumns = new List<ViewerColumn>();
        var transformedRows = rows.Select(_ => new List<object>()).ToList();

        for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            var column = columns[columnIndex];
            transformedColumns.Add(column);

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                transformedRows[rowIndex].Add(rows[rowIndex].ElementAtOrDefault(columnIndex) ?? null!);
            }

            if (!totalsByColumn.TryGetValue(columnIndex, out var total) || total == 0)
            {
                continue;
            }

            transformedColumns.Add(new ViewerColumn
            {
                Key = $"{column.Key}_percentOfTotal",
                Header = $"{column.Header ?? column.Key} %",
                Format = "percent",
                Sortable = false,
                Filterable = false
            });

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var value = ToDecimal(rows[rowIndex].ElementAtOrDefault(columnIndex));
                transformedRows[rowIndex].Add(Math.Round(value / total * 100, 2));
            }
        }

        return (transformedColumns, transformedRows);
    }

    private static bool IsNumericColumn(ViewerColumn column) =>
        string.Equals(column.Format, "number", StringComparison.OrdinalIgnoreCase)
        || string.Equals(column.Format, "currency", StringComparison.OrdinalIgnoreCase);

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
