using System.Drawing;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace AssetFox.Core.UniversalReporting.Export;

public class UniversalReportExcelExporter : IUniversalReportExporter
{
    private const int TitleRow = 1;
    private const int HeaderRow = 2;
    private const int DataStartRow = 3;
    private const double MaxColumnWidth = 45;
    private const double MinColumnWidth = 10;
    private static readonly Color TitleFillColor = Color.FromArgb(217, 225, 242);
    private static readonly Color HeaderFillColor = Color.FromArgb(180, 198, 231);
    private static readonly Color AlternatingRowFillColor = Color.LightGray;
    private static readonly Color TotalFillColor = Color.FromArgb(217, 217, 217);
    private static readonly Color BorderColor = Color.FromArgb(166, 166, 166);

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

    private static string GetConfigString(Dictionary<string, object> config, string key)
    {
        return config.TryGetValue(key, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
    }
}
