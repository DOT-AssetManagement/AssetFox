using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Export;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using Moq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Xunit;

namespace AssetFox.Core.UniversalReporting.Tests
{
    public class UniversalReportExcelExporterTests
    {
        [Fact]
        public async Task ExportAsync_DataWorksheet_AddsStyledHeaderAndFilterRange()
        {
            var simulationId = Guid.NewGuid();
            var reportFolder = Path.Combine("Reports", simulationId.ToString());
            var schema = new ReportSchemaDefinition
            {
                Pages = new List<ReportPage>
                {
                    new()
                    {
                        Components = new List<ReportComponent>
                        {
                            new()
                            {
                                Type = "assetDetail",
                                Title = "Asset Detail",
                                Order = 0,
                                Visible = true,
                            },
                        },
                    },
                },
            };

            var columns = new List<ViewerColumn>
            {
                new() { Key = "asset", Header = "Asset", Format = "text" },
                new() { Key = "condition", Header = "Very Long Condition Attribute Name", Format = "number" },
            };

            var queryEngine = new Mock<IUniversalQueryEngine>();
            queryEngine
                .Setup(engine => engine.StreamAllAsync(
                    It.IsAny<ReportComponent>(),
                    It.IsAny<ViewerDataRequest>(),
                    12,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ComponentResult(columns, Rows(
                    new List<object> { "Bridge 1", 82 },
                    new List<object> { "Bridge 2", 74 })));

            try
            {
                var exporter = new UniversalReportExcelExporter(queryEngine.Object);
                var filePath = await exporter.ExportAsync(schema, simulationId, 12, CancellationToken.None);

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets["Asset Detail"];

                Assert.NotNull(worksheet);
                Assert.Equal("Asset Detail", worksheet.Cells[1, 1].Value);
                Assert.Equal("Asset", worksheet.Cells[2, 1].Value);
                Assert.Equal("Very Long Condition Attribute Name", worksheet.Cells[2, 2].Value);
                Assert.Equal("Bridge 1", worksheet.Cells[3, 1].Value);
                Assert.Equal("Bridge 2", worksheet.Cells[4, 1].Value);
                Assert.True(worksheet.Cells[2, 1, 4, 2].AutoFilter);
                Assert.True(worksheet.Cells[2, 1].Style.Font.Bold);
                Assert.True(worksheet.Cells[2, 1].Style.WrapText);
                Assert.True(worksheet.Column(2).Width >= "Very Long Condition Attribute Name".Length);
                Assert.Equal(ExcelFillStyle.Solid, worksheet.Cells[4, 1].Style.Fill.PatternType);
            }
            finally
            {
                if (Directory.Exists(reportFolder))
                {
                    Directory.Delete(reportFolder, recursive: true);
                }
            }
        }

        [Fact]
        public async Task ExportAsync_TreatmentTimeline_PivotsYearsUnderGroupedHeaders()
        {
            var simulationId = Guid.NewGuid();
            var reportFolder = Path.Combine("Reports", simulationId.ToString());
            var component = new ReportComponent
            {
                Type = "treatmentTimeline",
                Title = "Treatment Timeline",
                Order = 0,
                Visible = true,
                Config = new Dictionary<string, object>
                {
                    ["showNoTreatment"] = false
                }
            };
            var schema = new ReportSchemaDefinition
            {
                Pages = new List<ReportPage>
                {
                    new() { Components = new List<ReportComponent> { component } }
                }
            };
            var columns = new List<ViewerColumn>
            {
                new() { Key = "asset", Header = "Asset ID", Format = "text" },
                new() { Key = "year", Header = "Year", Format = "year" },
                new() { Key = "staticAttribute_0", Header = "District", Format = "text" },
                new() { Key = "appliedTreatment", Header = "Treatment", Format = "text" },
                new() { Key = "cost", Header = "Cost", Format = "currency" },
                new() { Key = "yearAttribute_2", Header = "Condition", Format = "number" }
            };
            var queryEngine = new Mock<IUniversalQueryEngine>();
            queryEngine
                .Setup(engine => engine.StreamAllAsync(
                    It.IsAny<ReportComponent>(),
                    It.IsAny<ViewerDataRequest>(),
                    12,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ComponentResult(columns, Rows(
                    new List<object> { "Bridge 1", 2025, "North", "No Treatment", 0d, 80d },
                    new List<object> { "Bridge 1", 2026, "North", "Repair", 125000d, 92d },
                    new List<object> { "Bridge 2", 2025, "South", "Replace", 500000d, 95d },
                    new List<object> { "Bridge 2", 2026, "South", "No Treatment", 0d, 94d })));

            try
            {
                var exporter = new UniversalReportExcelExporter(queryEngine.Object);
                var filePath = await exporter.ExportAsync(schema, simulationId, 12, CancellationToken.None);

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets["Treatment Timeline"];

                Assert.NotNull(worksheet);
                Assert.Equal("Asset ID", worksheet.Cells[2, 1].Value);
                Assert.Equal("District", worksheet.Cells[2, 2].Value);
                Assert.Equal(2025, Convert.ToInt32(worksheet.Cells[2, 3].Value));
                Assert.Equal("Treatment", worksheet.Cells[3, 3].Value);
                Assert.Equal("Cost", worksheet.Cells[3, 4].Value);
                Assert.Equal("Condition", worksheet.Cells[3, 5].Value);
                Assert.Equal(2026, Convert.ToInt32(worksheet.Cells[2, 6].Value));
                Assert.True(worksheet.Cells[2, 1, 3, 1].Merge);
                Assert.True(worksheet.Cells[2, 3, 2, 5].Merge);
                Assert.Equal("Bridge 1", worksheet.Cells[4, 1].Value);
                Assert.Equal("North", worksheet.Cells[4, 2].Value);
                Assert.Equal("No Treatment", worksheet.Cells[4, 3].Value);
                Assert.Equal("Repair", worksheet.Cells[4, 6].Value);
                Assert.Equal(125000d, worksheet.Cells[4, 7].Value);
                Assert.Equal(92d, worksheet.Cells[4, 8].Value);

                queryEngine.Verify(engine => engine.StreamAllAsync(
                    It.Is<ReportComponent>(exportComponent =>
                        exportComponent.Config.ContainsKey("showNoTreatment")
                        && Equals(exportComponent.Config["showNoTreatment"], true)),
                    It.IsAny<ViewerDataRequest>(),
                    12,
                    It.IsAny<CancellationToken>()), Times.Once);
            }
            finally
            {
                if (Directory.Exists(reportFolder))
                {
                    Directory.Delete(reportFolder, recursive: true);
                }
            }
        }

        private static async IAsyncEnumerable<List<object>> Rows(params List<object>[] rows)
        {
            await Task.Yield();

            foreach (var row in rows)
            {
                yield return row;
            }
        }
    }
}
