using System.Drawing;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Components.ScenarioParameters;
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
    private const int TimelineYearHeaderRow = 2;
    private const int TimelineFieldHeaderRow = 3;
    private const int TimelineDataStartRow = 4;
    private const string TimelineStaticColumnPrefix = "staticAttribute_";
    private const int ScenarioParameterStandardLayoutColumns = 20;
    private const int ScenarioParameterKeyValueColumns = 8;
    private const double ScenarioParameterColumnWidth = 16;
    private const double ScenarioParameterFirstColumnWidth = 14;
    private const double MaxColumnWidth = 45;
    private const double MinColumnWidth = 10;
    private const string CompactCurrencyFormat = "[>=1000000000]$0.0,,,\"B\";[>=1000000]$0.0,,\"M\";$#,##0.00";
    private static readonly Color TitleFillColor = Color.FromArgb(217, 225, 242);
    private static readonly Color HeaderFillColor = Color.FromArgb(180, 198, 231);
    private static readonly Color AlternatingRowFillColor = Color.LightGray;
    private static readonly Color TotalFillColor = Color.FromArgb(217, 217, 217);
    private static readonly Color BorderColor = Color.FromArgb(166, 166, 166);
    private static readonly Color ScenarioParameterSectionFillColor = Color.FromArgb(69, 90, 100);
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
            .Where(component => component.Visible
                && !string.Equals(component.Type, "textBlock", StringComparison.OrdinalIgnoreCase))
            .OrderBy(component => component.Order)
            .ToList();

        if (components.Count == 0)
        {
            throw new InvalidOperationException("The report template has no components selected for Excel export.");
        }

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
            if (string.Equals(component.Type, "scenarioParameters", StringComparison.OrdinalIgnoreCase))
            {
                await WriteScenarioParametersWorksheetAsync(
                    package,
                    worksheetName,
                    title,
                    component,
                    schema.CalculatedFields,
                    simulationId,
                    runId,
                    cancellationToken);
                continue;
            }

            if (string.Equals(component.Type, "treatmentTimeline", StringComparison.OrdinalIgnoreCase))
            {
                await WriteTreatmentTimelineWorksheetAsync(
                    package,
                    worksheetName,
                    title,
                    component,
                    schema.CalculatedFields,
                    simulationId,
                    runId,
                    cancellationToken);
                continue;
            }

            if (ChartTemplateValidator.IsChartComponent(component.Type))
            {
                await WriteChartWorksheetAsync(
                    package,
                    worksheetName,
                    title,
                    component,
                    schema.CalculatedFields,
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

            var executionContext = ReportExecutionContext.Create(component, schema.CalculatedFields);
            var result = await _queryEngine.StreamAllAsync(executionContext, request, runId, cancellationToken);
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

    private async Task WriteScenarioParametersWorksheetAsync(
        ExcelPackage package,
        string worksheetName,
        string title,
        ReportComponent component,
        IReadOnlyList<ReportCalculatedField> calculatedFields,
        Guid simulationId,
        int runId,
        CancellationToken cancellationToken)
    {
        var request = new ViewerDataRequest
        {
            SimulationId = simulationId,
            Page = 1,
            PageSize = int.MaxValue,
            SortDirection = "asc",
            ColumnFilters = new Dictionary<string, string>()
        };
        var executionContext = ReportExecutionContext.Create(component, calculatedFields);
        var result = await _queryEngine.StreamAllAsync(executionContext, request, runId, cancellationToken);
        var encodedRows = new List<List<object>>();
        await foreach (var row in result.Rows.WithCancellation(cancellationToken))
        {
            encodedRows.Add(row);
        }

        var document = ScenarioParametersDataContract.Decode(result.Columns, encodedRows);
        var worksheet = package.Workbook.Worksheets.Add(worksheetName);
        var maximumColumns = Math.Max(ScenarioParameterKeyValueColumns, document.Sections
            .Select(GetScenarioParameterSectionLastColumn)
            .DefaultIfEmpty(ScenarioParameterKeyValueColumns)
            .Max());
        WriteWorksheetTitle(worksheet, title, maximumColumns);
        worksheet.View.ShowGridLines = false;

        var currentRow = DataStartRow;
        if (document.Sections.Count == 0)
        {
            worksheet.Cells[currentRow, 1].Value = "No scenario parameters are available for the selected sections.";
            worksheet.Cells[currentRow, 1, currentRow, maximumColumns].Merge = maximumColumns > 1;
            worksheet.Cells[currentRow, 1].Style.Font.Italic = true;
            worksheet.Cells[currentRow, 1].Style.Font.Color.SetColor(Color.DimGray);
            ApplyBorder(worksheet.Cells[currentRow, 1, currentRow, maximumColumns]);
            currentRow++;
        }

        foreach (var section in document.Sections.OrderBy(section => section.Order))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hasRowHeader = !string.IsNullOrWhiteSpace(section.RowHeader);
            var columnSpans = GetScenarioParameterColumnSpans(section);
            var sectionLastColumn = GetScenarioParameterSectionLastColumn(section);
            var sectionHeader = worksheet.Cells[currentRow, 1, currentRow, sectionLastColumn];
            sectionHeader.Merge = sectionLastColumn > 1;
            sectionHeader.Value = section.Title;
            sectionHeader.Style.Font.Bold = true;
            sectionHeader.Style.Font.Color.SetColor(Color.White);
            sectionHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
            sectionHeader.Style.Fill.BackgroundColor.SetColor(ScenarioParameterSectionFillColor);
            ApplyBorder(sectionHeader);
            worksheet.Row(currentRow).Height = 22;
            currentRow++;

            if (section.Kind == ScenarioParameterSectionKinds.KeyValue)
            {
                var labelSpan = columnSpans[0];
                var valueSpan = columnSpans[1];
                foreach (var row in section.Rows.OrderBy(row => row.Order))
                {
                    var labelRange = MergeScenarioParameterRange(worksheet, currentRow, labelSpan);
                    labelRange.Value = row.Label;
                    labelRange.Style.Font.Bold = true;
                    labelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    labelRange.Style.Fill.BackgroundColor.SetColor(TitleFillColor);
                    var cell = row.Cells.OrderBy(item => item.ColumnKey, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
                    var valueRange = MergeScenarioParameterRange(worksheet, currentRow, valueSpan);
                    WriteScenarioParameterValue(valueRange, cell?.Value, cell?.Format);
                    valueRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    var rowRange = worksheet.Cells[currentRow, labelSpan.StartColumn, currentRow, valueSpan.EndColumn];
                    rowRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rowRange.Style.WrapText = true;
                    ApplyBorder(rowRange);
                    worksheet.Row(currentRow).Height = string.Equals(
                        section.Key,
                        ScenarioParameterSectionKeys.ScenarioCriteria,
                        StringComparison.OrdinalIgnoreCase)
                        ? 54
                        : 24;
                    currentRow++;
                }
            }
            else
            {
                var tableStartRow = currentRow;
                var spanIndex = 0;
                if (hasRowHeader)
                {
                    var rowHeaderRange = MergeScenarioParameterRange(worksheet, currentRow, columnSpans[spanIndex++]);
                    rowHeaderRange.Value = section.RowHeader;
                }

                foreach (var column in section.Columns.OrderBy(column => column.Order))
                {
                    var headerCell = MergeScenarioParameterRange(worksheet, currentRow, columnSpans[spanIndex++]);
                    headerCell.Value = column.Label;
                }

                var tableLastColumn = columnSpans[^1].EndColumn;
                var tableHeader = worksheet.Cells[currentRow, 1, currentRow, tableLastColumn];
                tableHeader.Style.Font.Bold = true;
                tableHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tableHeader.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                tableHeader.Style.WrapText = true;
                tableHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
                tableHeader.Style.Fill.BackgroundColor.SetColor(TitleFillColor);
                ApplyBorder(tableHeader);
                worksheet.Row(currentRow).Height = 34;
                currentRow++;

                var dataRowIndex = 0;
                foreach (var row in section.Rows.OrderBy(row => row.Order))
                {
                    spanIndex = 0;
                    if (hasRowHeader)
                    {
                        var rowHeaderRange = MergeScenarioParameterRange(worksheet, currentRow, columnSpans[spanIndex++]);
                        rowHeaderRange.Value = row.Label;
                    }

                    var cells = row.Cells.ToDictionary(cell => cell.ColumnKey, StringComparer.OrdinalIgnoreCase);
                    foreach (var column in section.Columns.OrderBy(column => column.Order))
                    {
                        cells.TryGetValue(column.Key, out var cell);
                        var valueRange = MergeScenarioParameterRange(worksheet, currentRow, columnSpans[spanIndex++]);
                        WriteScenarioParameterValue(valueRange, cell?.Value, cell?.Format);
                    }

                    var dataRow = worksheet.Cells[currentRow, 1, currentRow, tableLastColumn];
                    dataRow.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    dataRow.Style.WrapText = !string.Equals(
                        section.Key,
                        ScenarioParameterSectionKeys.Funding,
                        StringComparison.OrdinalIgnoreCase);
                    if (dataRowIndex % 2 == 1)
                    {
                        dataRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        dataRow.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 250, 252));
                    }

                    worksheet.Row(currentRow).Height = GetScenarioParameterDataRowHeight(section.Key);
                    currentRow++;
                    dataRowIndex++;
                }

                if (currentRow > tableStartRow)
                {
                    ApplyBorder(worksheet.Cells[tableStartRow, 1, currentRow - 1, tableLastColumn]);
                }
            }

            currentRow++;
        }

        var lastRow = Math.Max(TitleRow, currentRow - 1);
        worksheet.Cells[TitleRow, 1, lastRow, maximumColumns].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        worksheet.Row(TitleRow).Height = 24;
        for (var columnIndex = 1; columnIndex <= maximumColumns; columnIndex++)
        {
            worksheet.Column(columnIndex).Width = columnIndex == 1
                ? ScenarioParameterFirstColumnWidth
                : ScenarioParameterColumnWidth;
        }

        worksheet.View.FreezePanes(DataStartRow, 1);
    }

    private static IReadOnlyList<ScenarioParameterColumnSpan> GetScenarioParameterColumnSpans(
        ScenarioParameterSection section)
    {
        if (section.Kind == ScenarioParameterSectionKinds.KeyValue)
        {
            return string.Equals(
                section.Key,
                ScenarioParameterSectionKeys.ScenarioCriteria,
                StringComparison.OrdinalIgnoreCase)
                ? new[]
                {
                    new ScenarioParameterColumnSpan(1, 3),
                    new ScenarioParameterColumnSpan(4, ScenarioParameterStandardLayoutColumns)
                }
                : new[]
                {
                    new ScenarioParameterColumnSpan(1, 3),
                    new ScenarioParameterColumnSpan(4, ScenarioParameterKeyValueColumns)
                };
        }

        if (section.Columns.Count == 4
            && string.Equals(section.Key, ScenarioParameterSectionKeys.Priorities, StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                new ScenarioParameterColumnSpan(1, 1),
                new ScenarioParameterColumnSpan(2, 3),
                new ScenarioParameterColumnSpan(4, 10),
                new ScenarioParameterColumnSpan(11, ScenarioParameterStandardLayoutColumns)
            };
        }

        if (section.Columns.Count == 5
            && string.Equals(section.Key, ScenarioParameterSectionKeys.CashFlow, StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                new ScenarioParameterColumnSpan(1, 3),
                new ScenarioParameterColumnSpan(4, 10),
                new ScenarioParameterColumnSpan(11, 12),
                new ScenarioParameterColumnSpan(13, 16),
                new ScenarioParameterColumnSpan(17, ScenarioParameterStandardLayoutColumns)
            };
        }

        if (section.Columns.Count == 3
            && string.Equals(section.Key, ScenarioParameterSectionKeys.BudgetCriteria, StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                new ScenarioParameterColumnSpan(1, 2),
                new ScenarioParameterColumnSpan(3, 5),
                new ScenarioParameterColumnSpan(6, ScenarioParameterStandardLayoutColumns)
            };
        }

        if (section.Columns.Count == 4
            && string.Equals(section.Key, ScenarioParameterSectionKeys.Treatments, StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                new ScenarioParameterColumnSpan(1, 3),
                new ScenarioParameterColumnSpan(4, 6),
                new ScenarioParameterColumnSpan(7, 9),
                new ScenarioParameterColumnSpan(10, ScenarioParameterStandardLayoutColumns)
            };
        }

        var columnCount = section.Columns.Count + (string.IsNullOrWhiteSpace(section.RowHeader) ? 0 : 1);
        return Enumerable.Range(1, Math.Max(1, columnCount))
            .Select(column => new ScenarioParameterColumnSpan(column, column))
            .ToList();
    }

    private static int GetScenarioParameterSectionLastColumn(ScenarioParameterSection section) =>
        GetScenarioParameterColumnSpans(section)[^1].EndColumn;

    private static ExcelRange MergeScenarioParameterRange(
        ExcelWorksheet worksheet,
        int row,
        ScenarioParameterColumnSpan span)
    {
        var range = worksheet.Cells[row, span.StartColumn, row, span.EndColumn];
        range.Merge = span.EndColumn > span.StartColumn;
        return range;
    }

    private static double GetScenarioParameterDataRowHeight(string sectionKey)
    {
        if (string.Equals(sectionKey, ScenarioParameterSectionKeys.Priorities, StringComparison.OrdinalIgnoreCase))
        {
            return 60;
        }

        if (string.Equals(sectionKey, ScenarioParameterSectionKeys.BudgetCriteria, StringComparison.OrdinalIgnoreCase)
            || string.Equals(sectionKey, ScenarioParameterSectionKeys.Treatments, StringComparison.OrdinalIgnoreCase))
        {
            return 72;
        }

        return string.Equals(sectionKey, ScenarioParameterSectionKeys.Funding, StringComparison.OrdinalIgnoreCase)
            ? 22
            : 28;
    }

    private static void WriteScenarioParameterValue(ExcelRange cell, object? value, string? format)
    {
        if (string.Equals(format, "boolean", StringComparison.OrdinalIgnoreCase))
        {
            cell.Value = value is bool boolean
                ? boolean ? "Yes" : "No"
                : value;
            return;
        }

        if (string.Equals(format, "date", StringComparison.OrdinalIgnoreCase))
        {
            if (value is DateTime date)
            {
                cell.Value = date;
                cell.Style.Numberformat.Format = "m/d/yyyy h:mm AM/PM";
                return;
            }

            if (DateTime.TryParse(Convert.ToString(value), out var parsedDate))
            {
                cell.Value = parsedDate;
                cell.Style.Numberformat.Format = "m/d/yyyy h:mm AM/PM";
                return;
            }
        }

        cell.Value = value;
        cell.Style.Numberformat.Format = format?.ToLowerInvariant() switch
        {
            "currency" => "$#,##0.##",
            "number" => "#,##0.##",
            "percent" => "0.0\"%\"",
            "year" => "0",
            _ => cell.Style.Numberformat.Format
        };
    }

    private sealed record ScenarioParameterColumnSpan(int StartColumn, int EndColumn);

    private async Task WriteTreatmentTimelineWorksheetAsync(
        ExcelPackage package,
        string worksheetName,
        string title,
        ReportComponent component,
        IReadOnlyList<ReportCalculatedField> calculatedFields,
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
        var exportComponent = CreateTreatmentTimelineExportComponent(component);
        var executionContext = ReportExecutionContext.Create(exportComponent, calculatedFields);
        var result = await _queryEngine.StreamAllAsync(executionContext, request, runId, cancellationToken);

        var yearIndex = ColumnIndex(result.Columns, "year");
        if (yearIndex < 0 || result.Columns.Count == 0)
        {
            throw new InvalidOperationException($"Treatment Timeline component '{title}' did not return key/year columns.");
        }

        var keyIndex = 0;
        var staticColumnIndexes = result.Columns
            .Select((column, index) => (column, index))
            .Where(item => item.column.Key?.StartsWith(TimelineStaticColumnPrefix, StringComparison.OrdinalIgnoreCase) == true)
            .Select(item => item.index)
            .ToList();
        var perYearColumnIndexes = result.Columns
            .Select((column, index) => (column, index))
            .Where(item => item.index != keyIndex
                && item.index != yearIndex
                && item.column.Key?.StartsWith(TimelineStaticColumnPrefix, StringComparison.OrdinalIgnoreCase) != true)
            .Select(item => item.index)
            .ToList();

        var years = new SortedSet<int>();
        var assets = new List<TimelinePivotAsset>();
        var assetsByKey = new Dictionary<string, TimelinePivotAsset>(StringComparer.OrdinalIgnoreCase);
        await foreach (var row in result.Rows.WithCancellation(cancellationToken))
        {
            if (keyIndex >= row.Count || yearIndex >= row.Count || !TryGetYear(row[yearIndex], out var year))
            {
                continue;
            }

            years.Add(year);
            var assetValue = row[keyIndex];
            var assetKey = assetValue?.ToString() ?? string.Empty;
            if (!assetsByKey.TryGetValue(assetKey, out var asset))
            {
                asset = new TimelinePivotAsset(
                    assetValue,
                    staticColumnIndexes.Select(index => index < row.Count ? row[index] : null).ToList());
                assetsByKey[assetKey] = asset;
                assets.Add(asset);
            }

            asset.ValuesByYear[year] = perYearColumnIndexes
                .Select(index => index < row.Count ? row[index] : null)
                .ToList();
        }

        var fixedColumnCount = 1 + staticColumnIndexes.Count;
        var columnCount = fixedColumnCount + (years.Count * perYearColumnIndexes.Count);
        if (columnCount > ExcelPackage.MaxColumns)
        {
            throw new InvalidOperationException(
                $"Treatment Timeline component '{title}' requires {columnCount} Excel columns, exceeding the {ExcelPackage.MaxColumns} column limit.");
        }

        var worksheet = package.Workbook.Worksheets.Add(worksheetName);
        WriteWorksheetTitle(worksheet, title, Math.Max(1, columnCount));

        WriteTimelineFixedHeader(worksheet, 1, result.Columns[keyIndex].Header ?? result.Columns[keyIndex].Key ?? string.Empty);
        for (var staticIndex = 0; staticIndex < staticColumnIndexes.Count; staticIndex++)
        {
            var column = result.Columns[staticColumnIndexes[staticIndex]];
            WriteTimelineFixedHeader(worksheet, staticIndex + 2, column.Header ?? column.Key ?? string.Empty);
            ApplyColumnNumberFormat(worksheet, staticIndex + 2, column.Format);
        }

        var excelColumn = fixedColumnCount + 1;
        if (perYearColumnIndexes.Count > 0)
        {
            foreach (var year in years)
            {
                var yearStartColumn = excelColumn;
                foreach (var perYearColumnIndex in perYearColumnIndexes)
                {
                    var column = result.Columns[perYearColumnIndex];
                    worksheet.Cells[TimelineFieldHeaderRow, excelColumn].Value = column.Header ?? column.Key;
                    ApplyColumnNumberFormat(worksheet, excelColumn, column.Format);
                    excelColumn++;
                }

                var yearEndColumn = excelColumn - 1;
                var yearRange = worksheet.Cells[TimelineYearHeaderRow, yearStartColumn, TimelineYearHeaderRow, yearEndColumn];
                yearRange.Merge = yearEndColumn > yearStartColumn;
                yearRange.Value = year;
            }
        }

        var rowIndex = TimelineDataStartRow;
        foreach (var asset in assets)
        {
            worksheet.Cells[rowIndex, 1].Value = asset.Key;
            for (var staticIndex = 0; staticIndex < asset.StaticValues.Count; staticIndex++)
            {
                worksheet.Cells[rowIndex, staticIndex + 2].Value = asset.StaticValues[staticIndex];
            }

            excelColumn = fixedColumnCount + 1;
            foreach (var year in years)
            {
                asset.ValuesByYear.TryGetValue(year, out var values);
                for (var fieldIndex = 0; fieldIndex < perYearColumnIndexes.Count; fieldIndex++)
                {
                    worksheet.Cells[rowIndex, excelColumn].Value = values != null && fieldIndex < values.Count
                        ? values[fieldIndex]
                        : null;
                    excelColumn++;
                }
            }

            rowIndex++;
        }

        ApplyTreatmentTimelineStyle(worksheet, Math.Max(1, columnCount), fixedColumnCount, rowIndex - 1);
    }

    private static ReportComponent CreateTreatmentTimelineExportComponent(ReportComponent component)
    {
        var config = component.Config == null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>(component.Config);

        // The viewer-only option must not remove years from the pivoted Excel timeline.
        config["showNoTreatment"] = true;
        return new ReportComponent
        {
            Id = component.Id,
            Type = component.Type,
            Title = component.Title,
            Order = component.Order,
            Visible = component.Visible,
            CriteriaExpression = component.CriteriaExpression,
            PageSize = component.PageSize,
            StripedRows = component.StripedRows,
            GridX = component.GridX,
            GridY = component.GridY,
            GridW = component.GridW,
            GridH = component.GridH,
            Config = config
        };
    }

    private static void WriteTimelineFixedHeader(ExcelWorksheet worksheet, int column, string header)
    {
        var range = worksheet.Cells[TimelineYearHeaderRow, column, TimelineFieldHeaderRow, column];
        range.Merge = true;
        range.Value = header;
    }

    private static void ApplyColumnNumberFormat(ExcelWorksheet worksheet, int column, string? format)
    {
        var numberFormat = ExcelColumnFormatMap.Resolve(format);
        if (!string.IsNullOrEmpty(numberFormat))
        {
            worksheet.Column(column).Style.Numberformat.Format = numberFormat;
        }
    }

    private static bool TryGetYear(object? value, out int year)
    {
        if (value is int intYear)
        {
            year = intYear;
            return true;
        }

        return int.TryParse(value?.ToString(), out year);
    }

    private static void ApplyTreatmentTimelineStyle(
        ExcelWorksheet worksheet,
        int columnCount,
        int fixedColumnCount,
        int lastDataRow)
    {
        var headerRange = worksheet.Cells[TimelineYearHeaderRow, 1, TimelineFieldHeaderRow, columnCount];
        headerRange.Style.Font.Bold = true;
        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        headerRange.Style.WrapText = true;
        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(HeaderFillColor);
        ApplyBorder(headerRange);

        worksheet.View.FreezePanes(TimelineDataStartRow, fixedColumnCount + 1);
        worksheet.Row(TimelineYearHeaderRow).Height = 24;
        worksheet.Row(TimelineFieldHeaderRow).Height = 30;

        if (lastDataRow >= TimelineDataStartRow)
        {
            var dataRange = worksheet.Cells[TimelineDataStartRow, 1, lastDataRow, columnCount];
            dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
            ApplyBorder(dataRange);
            for (var row = TimelineDataStartRow + 1; row <= lastDataRow; row += 2)
            {
                var rowRange = worksheet.Cells[row, 1, row, columnCount];
                rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                rowRange.Style.Fill.BackgroundColor.SetColor(AlternatingRowFillColor);
            }
        }

        var lastRow = Math.Max(TimelineFieldHeaderRow, lastDataRow);
        worksheet.Cells[TitleRow, 1, lastRow, columnCount].AutoFitColumns();
        for (var column = 1; column <= columnCount; column++)
        {
            if (worksheet.Column(column).Width > MaxColumnWidth)
            {
                worksheet.Column(column).Width = MaxColumnWidth;
            }

            if (worksheet.Column(column).Width < MinColumnWidth)
            {
                worksheet.Column(column).Width = MinColumnWidth;
            }
        }
    }

    private async Task WriteChartWorksheetAsync(
        ExcelPackage package,
        string worksheetName,
        string title,
        ReportComponent component,
        IReadOnlyList<ReportCalculatedField> calculatedFields,
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

        var executionContext = ReportExecutionContext.Create(component, calculatedFields);
        var result = await _queryEngine.StreamAllAsync(executionContext, request, runId, cancellationToken);
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

    private sealed class TimelinePivotAsset
    {
        public TimelinePivotAsset(object? key, List<object?> staticValues)
        {
            Key = key;
            StaticValues = staticValues;
        }

        public object? Key { get; }
        public List<object?> StaticValues { get; }
        public Dictionary<int, List<object?>> ValuesByYear { get; } = new();
    }

    private sealed record ChartExportRow(string Category, string Series, double Value);
}
