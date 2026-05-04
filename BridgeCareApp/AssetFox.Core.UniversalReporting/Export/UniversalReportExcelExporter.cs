using System.Drawing;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace AssetFox.Core.UniversalReporting.Export;

public class UniversalReportExcelExporter : IUniversalReportExporter
{
    private const int TitleRow = 1;
    private const int HeaderRow = 2;
    private const int DataStartRow = 3;
    private const double MaxColumnWidth = 45;
    private const double MinColumnWidth = 10;
    private const string CompactCurrencyFormat = "[>=1000000000]$0.0,,,\"B\";[>=1000000]$0.0,,\"M\";$#,##0.00";
    private static readonly Color TitleFillColor = Color.FromArgb(217, 225, 242);
    private static readonly Color HeaderFillColor = Color.FromArgb(180, 198, 231);
    private static readonly Color AlternatingRowFillColor = Color.LightGray;
    private static readonly Color TotalFillColor = Color.FromArgb(217, 217, 217);
    private static readonly Color BorderColor = Color.FromArgb(166, 166, 166);
    private static readonly Color[] StandardChartColors =
    {
        Color.FromArgb(42, 87, 141),
        Color.FromArgb(47, 111, 78),
        Color.FromArgb(199, 119, 0),
        Color.FromArgb(122, 78, 157),
        Color.FromArgb(47, 111, 126),
        Color.FromArgb(178, 58, 72),
        Color.FromArgb(91, 102, 122),
        Color.FromArgb(138, 106, 0)
    };

    private readonly IUniversalQueryEngine _queryEngine;

    public UniversalReportExcelExporter(IUniversalQueryEngine queryEngine)
    {
        _queryEngine = queryEngine ?? throw new ArgumentNullException(nameof(queryEngine));
    }

    public async Task<string> ExportAsync(
        ReportSchemaDefinition schema,
        Guid simulationId,
        int runId,
        CancellationToken cancellationToken,
        IExportProgressReporter? progressReporter = null)
    {
        progressReporter ??= NullExportProgressReporter.Instance;

        var components = schema.AllComponents
            .Where(component => component.Visible)
            .OrderBy(component => component.Order)
            .ToList();

        using var package = new ExcelPackage();
        var worksheetNamer = new WorksheetNamer();

        for (var i = 0; i < components.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var component = components[i];
            var title = string.IsNullOrWhiteSpace(component.Title)
                ? component.Type ?? $"Component {i + 1}"
                : component.Title;

            await progressReporter.ReportAsync(
                $"Exporting component {i + 1} of {components.Count}: {title}",
                cancellationToken);

            var worksheetName = worksheetNamer.Name(title, i);
            if (string.Equals(component.Type, "textBlock", StringComparison.OrdinalIgnoreCase))
            {
                WriteTextBlock(package, worksheetName, component);
                continue;
            }

            if (ChartTemplateValidator.IsChartComponent(component.Type))
            {
                await WriteChartWorksheetAsync(
                    package,
                    worksheetName,
                    title,
                    component,
                    simulationId,
                    runId,
                    cancellationToken);
                continue;
            }

            var request = new ViewerDataRequest
            {
                SimulationId = simulationId,
                Page = 1,
                PageSize = int.MaxValue,
                SortBy = null,
                SortDirection = "asc",
                ColumnFilters = new Dictionary<string, string>()
            };

            var result = await _queryEngine.StreamAllAsync(component, request, runId, cancellationToken);
            var worksheet = package.Workbook.Worksheets.Add(worksheetName);
            var columnCount = Math.Max(1, result.Columns.Count);

            WriteWorksheetTitle(worksheet, title, columnCount);

            for (var c = 0; c < result.Columns.Count; c++)
            {
                var column = result.Columns[c];
                var excelColumn = c + 1;

                worksheet.Cells[HeaderRow, excelColumn].Value = column.Header ?? column.Key;

                var numberFormat = ExcelColumnFormatMap.Resolve(column.Format);
                if (!string.IsNullOrEmpty(numberFormat))
                {
                    worksheet.Column(excelColumn).Style.Numberformat.Format = numberFormat;
                }
            }

            var rowIndex = DataStartRow;
            await foreach (var row in result.Rows.WithCancellation(cancellationToken))
            {
                for (var c = 0; c < row.Count; c++)
                {
                    worksheet.Cells[rowIndex, c + 1].Value = row[c];
                }

                rowIndex++;
            }

            ApplyDataWorksheetStyle(worksheet, result.Columns, rowIndex - 1);
        }

        var folderPathForSimulation = Path.Combine("Reports", simulationId.ToString());
        Directory.CreateDirectory(folderPathForSimulation);
        var filePath = Path.Combine(folderPathForSimulation, "UniversalReport.xlsx");

        await File.WriteAllBytesAsync(filePath, package.GetAsByteArray(), cancellationToken);
        return filePath;
    }

    private async Task WriteChartWorksheetAsync(
        ExcelPackage package,
        string worksheetName,
        string title,
        ReportComponent component,
        Guid simulationId,
        int runId,
        CancellationToken cancellationToken)
    {
        var request = new ViewerDataRequest
        {
            SimulationId = simulationId,
            Page = 1,
            PageSize = int.MaxValue,
            SortBy = null,
            SortDirection = "asc",
            ColumnFilters = new Dictionary<string, string>()
        };

        var result = await _queryEngine.StreamAllAsync(component, request, runId, cancellationToken);
        var rows = new List<List<object>>();
        await foreach (var row in result.Rows.WithCancellation(cancellationToken))
        {
            rows.Add(row);
        }

        var worksheet = package.Workbook.Worksheets.Add(worksheetName);
        WriteWorksheetTitle(worksheet, title, 8);

        var categoryIndex = ColumnIndex(result.Columns, "category");
        var seriesIndex = ColumnIndex(result.Columns, "series");
        var valueIndex = ColumnIndex(result.Columns, "value");
        if (categoryIndex < 0 || valueIndex < 0)
        {
            throw new InvalidOperationException($"Chart component '{title}' did not return category/value columns.");
        }

        var chartRows = rows
            .Select(row => new ChartExportRow(
                GetCellText(row, categoryIndex, "(None)"),
                seriesIndex >= 0 ? GetCellText(row, seriesIndex, "") : "",
                GetCellDouble(row, valueIndex)))
            .ToList();

        var dataHeaderRow = 20;
        var dataStartRow = dataHeaderRow + 1;
        var config = ChartConfigReader.Read(component.Config);
        var hasSeries = HasBinding(config.Bindings.Series);
        var categoryLabel = BindingLabel(config.Bindings.Category, "Category");
        var valueLabel = BindingLabel(config.Bindings.Value, "Value");
        var categories = chartRows
            .Select(row => row.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var seriesNames = chartRows
            .Select(row => ResolveSeriesName(row.Series, hasSeries, valueLabel))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        worksheet.Cells[dataHeaderRow, 1].Value = categoryLabel;
        for (var s = 0; s < seriesNames.Count; s++)
        {
            worksheet.Cells[dataHeaderRow, s + 2].Value = hasSeries ? seriesNames[s] : valueLabel;
        }

        for (var r = 0; r < categories.Count; r++)
        {
            var category = categories[r];
            worksheet.Cells[dataStartRow + r, 1].Value = category;
            for (var s = 0; s < seriesNames.Count; s++)
            {
                var seriesName = seriesNames[s];
                var value = chartRows
                    .Where(row => string.Equals(row.Category, category, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(ResolveSeriesName(row.Series, hasSeries, valueLabel), seriesName, StringComparison.OrdinalIgnoreCase))
                    .Sum(row => row.Value);
                worksheet.Cells[dataStartRow + r, s + 2].Value = value;
            }
        }

        if (categories.Count == 0 || seriesNames.Count == 0)
        {
            return;
        }

        var lastDataRow = Math.Max(dataHeaderRow, dataStartRow + categories.Count - 1);
        var lastDataColumn = Math.Max(2, seriesNames.Count + 1);
        var valueNumberFormat = ResolveChartValueNumberFormat(config.Bindings.Value?.Format);
        if (!string.IsNullOrWhiteSpace(valueNumberFormat))
        {
            worksheet.Cells[dataStartRow, 2, lastDataRow, lastDataColumn].Style.Numberformat.Format = valueNumberFormat;
        }

        var categoryRangeColumn = 1;
        if (string.Equals(component.Type, "pieChart", StringComparison.OrdinalIgnoreCase))
        {
            categoryRangeColumn = lastDataColumn + 1;
            worksheet.Cells[dataHeaderRow, categoryRangeColumn].Value = "Legend Label";
            for (var r = 0; r < categories.Count; r++)
            {
                worksheet.Cells[dataStartRow + r, categoryRangeColumn].Value = categories[r];
            }

            worksheet.Column(categoryRangeColumn).Hidden = true;
        }

        var chart = worksheet.Drawings.AddChart(
            SanitizeChartName(title),
            ResolveExcelChartType(component, config));
        chart.Title.Text = title;
        chart.SetPosition(1, 0, 0, 0);
        chart.SetSize(900, 320);
        chart.ShowHiddenData = categoryRangeColumn != 1;
        ApplyChartValueAxisFormat(chart, component, config, valueNumberFormat);
        ApplyChartAxisLabels(chart, component, config, categoryLabel, valueLabel);

        var categoryRange = worksheet.Cells[dataStartRow, categoryRangeColumn, dataStartRow + categories.Count - 1, categoryRangeColumn];
        for (var s = 0; s < seriesNames.Count; s++)
        {
            var valueRange = worksheet.Cells[dataStartRow, s + 2, dataStartRow + categories.Count - 1, s + 2];
            var serie = chart.Series.Add(valueRange, categoryRange);
            serie.Header = hasSeries ? seriesNames[s] : valueLabel;
            TryApplySeriesColor(serie, s);
        }

        if (ShouldHideExcelLegend(component, config, hasSeries))
        {
            TryRemoveLegend(chart);
        }

        var headerRange = worksheet.Cells[dataHeaderRow, 1, dataHeaderRow, lastDataColumn];
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(HeaderFillColor);
        ApplyBorder(headerRange);

        if (categories.Count > 0)
        {
            var dataRange = worksheet.Cells[dataStartRow, 1, lastDataRow, lastDataColumn];
            ApplyBorder(dataRange);
        }

        worksheet.Cells[1, 1, lastDataRow, lastDataColumn].AutoFitColumns();
    }

    private static void WriteTextBlock(ExcelPackage package, string worksheetName, ReportComponent component)
    {
        var worksheet = package.Workbook.Worksheets.Add(worksheetName);
        var config = component.Config ?? new Dictionary<string, object>();

        worksheet.Cells[1, 1].Value = GetConfigString(config, "content");
        worksheet.Cells[1, 1, 1, 6].Merge = true;
        worksheet.Cells[1, 1, 1, 6].Style.WrapText = true;
        worksheet.Cells[1, 1, 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[1, 1, 1, 6].Style.Fill.BackgroundColor.SetColor(TitleFillColor);
        ApplyBorder(worksheet.Cells[1, 1, 1, 6]);
        worksheet.Column(1).Width = 80;
        worksheet.Row(1).Height = 45;

        switch (GetConfigString(config, "style").ToLowerInvariant())
        {
            case "heading":
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 18;
                break;
            case "subheading":
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                break;
            case "note":
                worksheet.Cells[1, 1].Style.Font.Italic = true;
                worksheet.Cells[1, 1].Style.Font.Size = 10;
                break;
            default:
                worksheet.Cells[1, 1].Style.Font.Size = 11;
                break;
        }
    }

    private static void WriteWorksheetTitle(ExcelWorksheet worksheet, string title, int columnCount)
    {
        var titleRange = worksheet.Cells[TitleRow, 1, TitleRow, columnCount];
        titleRange.Merge = columnCount > 1;
        titleRange.Value = title;
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.Size = 14;
        titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        titleRange.Style.Fill.BackgroundColor.SetColor(TitleFillColor);
        ApplyBorder(titleRange);
        worksheet.Row(TitleRow).Height = 24;
    }

    private static void ApplyDataWorksheetStyle(
        ExcelWorksheet worksheet,
        IReadOnlyList<ViewerColumn> columns,
        int lastDataRow)
    {
        var columnCount = Math.Max(1, columns.Count);
        var lastRow = Math.Max(HeaderRow, lastDataRow);
        var headerRange = worksheet.Cells[HeaderRow, 1, HeaderRow, columnCount];

        headerRange.Style.Font.Bold = true;
        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        headerRange.Style.WrapText = true;
        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(HeaderFillColor);
        ApplyBorder(headerRange);

        worksheet.Cells[HeaderRow, 1, lastRow, columnCount].AutoFilter = true;
        worksheet.View.FreezePanes(DataStartRow, 1);
        worksheet.Row(HeaderRow).Height = 30;

        if (lastDataRow >= DataStartRow)
        {
            var dataRange = worksheet.Cells[DataStartRow, 1, lastDataRow, columnCount];
            dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
            ApplyBorder(dataRange);
            ApplyAlternatingRowStyle(worksheet, columnCount, lastDataRow);
            ApplyTotalRowStyleIfPresent(worksheet, columns, lastDataRow);
        }

        worksheet.Cells[TitleRow, 1, lastRow, columnCount].AutoFitColumns();
        for (var columnIndex = 1; columnIndex <= columnCount; columnIndex++)
        {
            var column = worksheet.Column(columnIndex);
            var minimumWidth = GetMinimumColumnWidth(columns, columnIndex);
            var maximumWidth = Math.Max(MaxColumnWidth, minimumWidth);

            if (column.Width > maximumWidth)
            {
                column.Width = maximumWidth;
            }

            if (column.Width < minimumWidth)
            {
                column.Width = minimumWidth;
            }
        }
    }

    private static void ApplyAlternatingRowStyle(ExcelWorksheet worksheet, int columnCount, int lastDataRow)
    {
        for (var row = DataStartRow + 1; row <= lastDataRow; row += 2)
        {
            var rowRange = worksheet.Cells[row, 1, row, columnCount];
            rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            rowRange.Style.Fill.BackgroundColor.SetColor(AlternatingRowFillColor);
        }
    }

    private static double GetMinimumColumnWidth(IReadOnlyList<ViewerColumn> columns, int columnIndex)
    {
        if (columnIndex > columns.Count)
        {
            return MinColumnWidth;
        }

        var column = columns[columnIndex - 1];
        var header = column.Header ?? column.Key ?? string.Empty;
        var longestHeaderLine = header
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim().Length)
            .DefaultIfEmpty(0)
            .Max();

        return Math.Max(MinColumnWidth, Math.Ceiling(longestHeaderLine * 1.15) + 2);
    }

    private static void ApplyTotalRowStyleIfPresent(
        ExcelWorksheet worksheet,
        IReadOnlyList<ViewerColumn> columns,
        int lastDataRow)
    {
        var firstCellValue = worksheet.Cells[lastDataRow, 1].Value?.ToString();
        if (!string.Equals(firstCellValue, "Total", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var totalRange = worksheet.Cells[lastDataRow, 1, lastDataRow, Math.Max(1, columns.Count)];
        totalRange.Style.Font.Bold = true;
        totalRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        totalRange.Style.Fill.BackgroundColor.SetColor(TotalFillColor);
        ApplyBorder(totalRange);
    }

    private static void ApplyBorder(ExcelRange range)
    {
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Top.Color.SetColor(BorderColor);
        range.Style.Border.Left.Color.SetColor(BorderColor);
        range.Style.Border.Right.Color.SetColor(BorderColor);
        range.Style.Border.Bottom.Color.SetColor(BorderColor);
    }

    private static int ColumnIndex(IReadOnlyList<ViewerColumn> columns, string key)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            if (string.Equals(columns[i].Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static string GetCellText(List<object> row, int index, string fallback)
    {
        if (index < 0 || index >= row.Count)
        {
            return fallback;
        }

        return row[index]?.ToString() ?? fallback;
    }

    private static double GetCellDouble(List<object> row, int index)
    {
        if (index < 0 || index >= row.Count || row[index] == null)
        {
            return 0;
        }

        return row[index] switch
        {
            double d => d,
            float f => f,
            decimal m => (double)m,
            int i => i,
            long l => l,
            _ => double.TryParse(row[index].ToString(), out var parsed) ? parsed : 0
        };
    }

    private static bool HasBinding(ChartFieldBindingDefinition? binding) =>
        binding != null
        && !string.IsNullOrWhiteSpace(binding.Source)
        && !string.IsNullOrWhiteSpace(binding.Field)
        && !string.IsNullOrWhiteSpace(binding.Label);

    private static string BindingLabel(ChartFieldBindingDefinition? binding, string fallback) =>
        string.IsNullOrWhiteSpace(binding?.Label) ? fallback : binding.Label;

    private static string ResolveSeriesName(string series, bool hasSeries, string valueLabel)
    {
        if (!hasSeries)
        {
            return valueLabel;
        }

        return string.IsNullOrWhiteSpace(series) ? "(None)" : series;
    }

    private static string? ResolveChartValueNumberFormat(string? format) =>
        string.Equals(format, "currency", StringComparison.OrdinalIgnoreCase)
            ? CompactCurrencyFormat
            : ExcelColumnFormatMap.Resolve(format);

    private static eChartType ResolveExcelChartType(
        ReportComponent component,
        ChartConfigDefinition config)
    {
        if (string.Equals(component.Type, "pieChart", StringComparison.OrdinalIgnoreCase))
        {
            return config.Display.Donut || string.Equals(config.Mode, "donut", StringComparison.OrdinalIgnoreCase)
                ? eChartType.Doughnut
                : eChartType.Pie;
        }

        if (string.Equals(component.Type, "lineChart", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(config.Mode, "area", StringComparison.OrdinalIgnoreCase)
                ? eChartType.Area
                : eChartType.LineMarkers;
        }

        var stacked = config.Display.Stacked
            || string.Equals(config.Mode, "stacked", StringComparison.OrdinalIgnoreCase)
            || string.Equals(config.Mode, "stacked100", StringComparison.OrdinalIgnoreCase);
        var horizontal = config.Display.Horizontal;

        return (horizontal, stacked) switch
        {
            (true, true) => eChartType.BarStacked,
            (true, false) => eChartType.BarClustered,
            (false, true) => eChartType.ColumnStacked,
            _ => eChartType.ColumnClustered
        };
    }

    private static bool ShouldHideExcelLegend(
        ReportComponent component,
        ChartConfigDefinition config,
        bool hasSeries)
    {
        if (hasSeries)
        {
            return false;
        }

        return string.Equals(component.Type, "barChart", StringComparison.OrdinalIgnoreCase)
            || string.Equals(component.Type, "lineChart", StringComparison.OrdinalIgnoreCase)
            || (string.Equals(config.ChartKind, "bar", StringComparison.OrdinalIgnoreCase)
                || string.Equals(config.ChartKind, "line", StringComparison.OrdinalIgnoreCase));
    }

    private static void ApplyChartValueAxisFormat(
        ExcelChart chart,
        ReportComponent component,
        ChartConfigDefinition config,
        string? numberFormat)
    {
        if (string.IsNullOrWhiteSpace(numberFormat)
            || string.Equals(component.Type, "pieChart", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var isHorizontalBar = string.Equals(component.Type, "barChart", StringComparison.OrdinalIgnoreCase)
                && config.Display.Horizontal;
            var valueAxis = isHorizontalBar ? chart.XAxis : chart.YAxis;
            valueAxis.SourceLinked = false;
            valueAxis.Format = numberFormat;
        }
        catch
        {
            // Some EPPlus chart types expose axis formatting inconsistently. The worksheet
            // source data remains formatted even if the chart axis cannot be updated.
        }
    }

    private static void ApplyChartAxisLabels(
        ExcelChart chart,
        ReportComponent component,
        ChartConfigDefinition config,
        string categoryLabel,
        string valueLabel)
    {
        if (string.Equals(component.Type, "pieChart", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var isHorizontalBar = string.Equals(component.Type, "barChart", StringComparison.OrdinalIgnoreCase)
                && config.Display.Horizontal;

            chart.XAxis.Title.Text = isHorizontalBar ? valueLabel : categoryLabel;
            chart.YAxis.Title.Text = isHorizontalBar ? categoryLabel : valueLabel;
        }
        catch
        {
            // Axis title support depends on the concrete EPPlus chart axis type.
        }
    }

    private static void TryRemoveLegend(ExcelChart chart)
    {
        try
        {
            chart.Legend.Remove();
        }
        catch
        {
            // Excel can still open the chart if EPPlus cannot remove the legend node.
        }
    }

    private static string SanitizeChartName(string title)
    {
        var cleaned = new string((title ?? "Chart")
            .Where(char.IsLetterOrDigit)
            .Take(24)
            .ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "Chart" : $"{cleaned}Chart";
    }

    private static void TryApplySeriesColor(ExcelChartSerie serie, int index)
    {
        try
        {
            var fill = serie.GetType().GetProperty("Fill")?.GetValue(serie);
            var colorProperty = fill?.GetType().GetProperty("Color");
            colorProperty?.SetValue(fill, StandardChartColors[index % StandardChartColors.Length]);
        }
        catch
        {
            // EPPlus exposes series color APIs differently across versions. The chart still exports
            // correctly when a specific color cannot be applied.
        }
    }

    private static string GetConfigString(Dictionary<string, object> config, string key)
    {
        return config.TryGetValue(key, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
    }

    private sealed record ChartExportRow(string Category, string Series, double Value);
}
