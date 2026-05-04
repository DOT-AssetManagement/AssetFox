using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using OfficeOpenXml;

namespace AssetFox.Core.UniversalReporting.Export;

public class UniversalReportExcelExporter : IUniversalReportExporter
{
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

            for (var c = 0; c < result.Columns.Count; c++)
            {
                var column = result.Columns[c];
                var excelColumn = c + 1;

                worksheet.Cells[1, excelColumn].Value = column.Header ?? column.Key;
                worksheet.Cells[1, excelColumn].Style.Font.Bold = true;

                var numberFormat = ExcelColumnFormatMap.Resolve(column.Format);
                if (!string.IsNullOrEmpty(numberFormat))
                {
                    worksheet.Column(excelColumn).Style.Numberformat.Format = numberFormat;
                }
            }

            var rowIndex = 2;
            await foreach (var row in result.Rows.WithCancellation(cancellationToken))
            {
                for (var c = 0; c < row.Count; c++)
                {
                    worksheet.Cells[rowIndex, c + 1].Value = row[c];
                }

                rowIndex++;
            }
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

    private static string GetConfigString(Dictionary<string, object> config, string key)
    {
        return config.TryGetValue(key, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
    }
}
