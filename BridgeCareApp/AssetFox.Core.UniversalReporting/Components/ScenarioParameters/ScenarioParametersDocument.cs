using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Components.ScenarioParameters;

public static class ScenarioParameterSectionKeys
{
    public const string Scenario = "scenario";
    public const string Investment = "investment";
    public const string Analysis = "analysis";
    public const string ScenarioCriteria = "scenarioCriteria";
    public const string Priorities = "priorities";
    public const string CashFlow = "cashFlow";
    public const string Funding = "funding";
    public const string BudgetCriteria = "budgetCriteria";
    public const string Treatments = "treatments";

    public static IReadOnlyList<string> All { get; } = new[]
    {
        Scenario,
        Investment,
        Analysis,
        ScenarioCriteria,
        Priorities,
        CashFlow,
        Funding,
        BudgetCriteria,
        Treatments
    };
}

public static class ScenarioParameterSectionKinds
{
    public const string KeyValue = "keyValue";
    public const string Table = "table";
    public const string Matrix = "matrix";
}

public sealed class ScenarioParametersDocument
{
    public List<ScenarioParameterSection> Sections { get; } = new();
}

public sealed class ScenarioParameterSection
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Kind { get; init; } = ScenarioParameterSectionKinds.Table;
    public int Order { get; init; }
    public string RowHeader { get; init; } = string.Empty;
    public List<ScenarioParameterColumn> Columns { get; } = new();
    public List<ScenarioParameterRow> Rows { get; } = new();
}

public sealed class ScenarioParameterColumn
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int Order { get; init; }
}

public sealed class ScenarioParameterRow
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int Order { get; init; }
    public List<ScenarioParameterCell> Cells { get; } = new();
}

public sealed class ScenarioParameterCell
{
    public string ColumnKey { get; init; } = string.Empty;
    public object? Value { get; init; }
    public string Format { get; init; } = "text";
}

public sealed record ScenarioParameterRecord(
    string SectionKey,
    string SectionTitle,
    string SectionKind,
    int SectionOrder,
    string RowHeader,
    string RowKey,
    string RowLabel,
    int RowOrder,
    string ColumnKey,
    string ColumnLabel,
    int ColumnOrder,
    object? Value,
    string Format);

public static class ScenarioParametersDataContract
{
    public static IReadOnlyList<ViewerColumn> Columns { get; } = new List<ViewerColumn>
    {
        Column("sectionKey"),
        Column("sectionTitle"),
        Column("sectionKind"),
        Column("sectionOrder", "number"),
        Column("rowHeader"),
        Column("rowKey"),
        Column("rowLabel"),
        Column("rowOrder", "number"),
        Column("columnKey"),
        Column("columnLabel"),
        Column("columnOrder", "number"),
        Column("value"),
        Column("format")
    };

    public static IEnumerable<List<object>> Encode(ScenarioParametersDocument document)
    {
        foreach (var section in document.Sections.OrderBy(section => section.Order))
        {
            var columns = section.Columns.ToDictionary(column => column.Key, StringComparer.OrdinalIgnoreCase);
            foreach (var row in section.Rows.OrderBy(row => row.Order))
            {
                foreach (var cell in row.Cells
                             .OrderBy(cell => columns.TryGetValue(cell.ColumnKey, out var column) ? column.Order : int.MaxValue))
                {
                    columns.TryGetValue(cell.ColumnKey, out var column);
                    yield return new List<object>
                    {
                        section.Key,
                        section.Title,
                        section.Kind,
                        section.Order,
                        section.RowHeader,
                        row.Key,
                        row.Label,
                        row.Order,
                        cell.ColumnKey,
                        column?.Label ?? cell.ColumnKey,
                        column?.Order ?? int.MaxValue,
                        cell.Value ?? string.Empty,
                        cell.Format
                    };
                }
            }
        }
    }

    public static ScenarioParametersDocument Decode(
        IReadOnlyList<ViewerColumn> columns,
        IEnumerable<List<object>> rows)
    {
        var indexes = columns
            .Select((column, index) => new { column.Key, Index = index })
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .ToDictionary(item => item.Key!, item => item.Index, StringComparer.OrdinalIgnoreCase);

        var records = rows.Select(row => ReadRecord(indexes, row)).ToList();
        var document = new ScenarioParametersDocument();

        foreach (var sectionGroup in records
                     .GroupBy(record => record.SectionKey, StringComparer.OrdinalIgnoreCase)
                     .OrderBy(group => group.Min(record => record.SectionOrder)))
        {
            var first = sectionGroup.First();
            var section = new ScenarioParameterSection
            {
                Key = first.SectionKey,
                Title = first.SectionTitle,
                Kind = first.SectionKind,
                Order = first.SectionOrder,
                RowHeader = first.RowHeader
            };

            foreach (var columnRecord in sectionGroup
                         .GroupBy(record => record.ColumnKey, StringComparer.OrdinalIgnoreCase)
                         .Select(group => group.OrderBy(record => record.ColumnOrder).First())
                         .OrderBy(record => record.ColumnOrder))
            {
                section.Columns.Add(new ScenarioParameterColumn
                {
                    Key = columnRecord.ColumnKey,
                    Label = columnRecord.ColumnLabel,
                    Order = columnRecord.ColumnOrder
                });
            }

            foreach (var rowGroup in sectionGroup
                         .GroupBy(record => record.RowKey, StringComparer.OrdinalIgnoreCase)
                         .OrderBy(group => group.Min(record => record.RowOrder)))
            {
                var firstRow = rowGroup.First();
                var row = new ScenarioParameterRow
                {
                    Key = firstRow.RowKey,
                    Label = firstRow.RowLabel,
                    Order = firstRow.RowOrder
                };

                foreach (var record in rowGroup.OrderBy(record => record.ColumnOrder))
                {
                    row.Cells.Add(new ScenarioParameterCell
                    {
                        ColumnKey = record.ColumnKey,
                        Value = record.Value,
                        Format = record.Format
                    });
                }

                section.Rows.Add(row);
            }

            document.Sections.Add(section);
        }

        return document;
    }

    private static ScenarioParameterRecord ReadRecord(
        IReadOnlyDictionary<string, int> indexes,
        IReadOnlyList<object> row)
    {
        object? Value(string key) =>
            indexes.TryGetValue(key, out var index) && index < row.Count ? row[index] : null;

        string Text(string key) => Convert.ToString(Value(key)) ?? string.Empty;

        int Number(string key) => int.TryParse(Text(key), out var number) ? number : 0;

        return new ScenarioParameterRecord(
            Text("sectionKey"),
            Text("sectionTitle"),
            Text("sectionKind"),
            Number("sectionOrder"),
            Text("rowHeader"),
            Text("rowKey"),
            Text("rowLabel"),
            Number("rowOrder"),
            Text("columnKey"),
            Text("columnLabel"),
            Number("columnOrder"),
            Value("value"),
            Text("format"));
    }

    private static ViewerColumn Column(string key, string format = "text") => new()
    {
        Key = key,
        Header = key,
        Format = format,
        Sortable = false,
        Filterable = false
    };
}
