using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Components.ScenarioParameters;
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
                    It.IsAny<ReportExecutionContext>(),
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
                    It.IsAny<ReportExecutionContext>(),
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
                    It.Is<ReportExecutionContext>(context =>
                        context.Component.Config.ContainsKey("showNoTreatment")
                        && Equals(context.Component.Config["showNoTreatment"], true)),
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

        [Fact]
        public async Task ExportAsync_ScenarioParameters_WritesStructuredSections()
        {
            var simulationId = Guid.NewGuid();
            var reportFolder = Path.Combine("Reports", simulationId.ToString());
            var component = new ReportComponent
            {
                Type = "scenarioParameters",
                Title = "Scenario Parameters",
                Order = 0,
                Visible = true
            };
            var schema = new ReportSchemaDefinition
            {
                Pages = new List<ReportPage>
                {
                    new() { Components = new List<ReportComponent> { component } }
                }
            };
            var document = new ScenarioParametersDocument();
            var scenarioSection = new ScenarioParameterSection
            {
                Key = ScenarioParameterSectionKeys.Scenario,
                Title = "Scenario",
                Kind = ScenarioParameterSectionKinds.KeyValue,
                Order = 10
            };
            scenarioSection.Columns.Add(new ScenarioParameterColumn { Key = "value", Label = "Value", Order = 0 });
            var scenarioRow = new ScenarioParameterRow { Key = "name", Label = "Scenario Name", Order = 0 };
            scenarioRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "value", Value = "Capital Plan", Format = "text" });
            scenarioSection.Rows.Add(scenarioRow);
            document.Sections.Add(scenarioSection);

            var fundingSection = new ScenarioParameterSection
            {
                Key = ScenarioParameterSectionKeys.Funding,
                Title = "Funding by Year",
                Kind = ScenarioParameterSectionKinds.Matrix,
                Order = 70,
                RowHeader = "Year"
            };
            fundingSection.Columns.Add(new ScenarioParameterColumn { Key = "capital", Label = "Capital", Order = 0 });
            var fundingRow = new ScenarioParameterRow { Key = "year-2027", Label = "2027", Order = 0 };
            fundingRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "capital", Value = 2_000_000m, Format = "currency" });
            fundingSection.Rows.Add(fundingRow);
            document.Sections.Add(fundingSection);

            var queryEngine = new Mock<IUniversalQueryEngine>();
            queryEngine
                .Setup(engine => engine.StreamAllAsync(
                    It.IsAny<ReportExecutionContext>(),
                    It.IsAny<ViewerDataRequest>(),
                    12,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ComponentResult(
                    ScenarioParametersDataContract.Columns,
                    Rows(ScenarioParametersDataContract.Encode(document).ToArray())));

            try
            {
                var exporter = new UniversalReportExcelExporter(queryEngine.Object);
                var filePath = await exporter.ExportAsync(schema, simulationId, 12, CancellationToken.None);

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets["Scenario Parameters"];

                Assert.NotNull(worksheet);
                Assert.Equal("Scenario Parameters", worksheet.Cells[1, 1].Value);
                Assert.Equal("Scenario", worksheet.Cells[3, 1].Value);
                Assert.Equal("Scenario Name", worksheet.Cells[4, 1].Value);
                Assert.Equal("Capital Plan", worksheet.Cells[4, 4].Value);
                Assert.True(worksheet.Cells[3, 1, 3, 8].Merge);
                Assert.True(worksheet.Cells[4, 1, 4, 3].Merge);
                Assert.True(worksheet.Cells[4, 4, 4, 8].Merge);
                Assert.Equal("Funding by Year", worksheet.Cells[6, 1].Value);
                Assert.Equal("Year", worksheet.Cells[7, 1].Value);
                Assert.Equal("Capital", worksheet.Cells[7, 2].Value);
                Assert.Equal("2027", worksheet.Cells[8, 1].Value);
                Assert.Equal(2_000_000d, Convert.ToDouble(worksheet.Cells[8, 2].Value));
                Assert.Equal("$#,##0.##", worksheet.Cells[8, 2].Style.Numberformat.Format);
                Assert.Equal(14d, worksheet.Column(1).Width, 2);
                Assert.Equal(16d, worksheet.Column(2).Width, 2);
                Assert.False(worksheet.Cells[8, 2].Style.WrapText);
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
        public async Task ExportAsync_ScenarioParameters_UsesReadableSpansForNarrativeTables()
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
                                Type = "scenarioParameters",
                                Title = "Scenario Parameters",
                                Order = 0,
                                Visible = true
                            }
                        }
                    }
                }
            };
            var document = new ScenarioParametersDocument();
            var prioritiesSection = new ScenarioParameterSection
            {
                Key = ScenarioParameterSectionKeys.Priorities,
                Title = "Analysis Priorities",
                Kind = ScenarioParameterSectionKinds.Table,
                Order = 50
            };
            prioritiesSection.Columns.Add(new ScenarioParameterColumn { Key = "priority", Label = "Priority", Order = 0 });
            prioritiesSection.Columns.Add(new ScenarioParameterColumn { Key = "year", Label = "Year", Order = 1 });
            prioritiesSection.Columns.Add(new ScenarioParameterColumn { Key = "criteria", Label = "Criteria", Order = 2 });
            prioritiesSection.Columns.Add(new ScenarioParameterColumn { Key = "allocations", Label = "Budget Allocations", Order = 3 });
            var priorityRow = new ScenarioParameterRow { Key = "priority-1", Order = 0 };
            priorityRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "priority", Value = 1, Format = "number" });
            priorityRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "year", Value = "All years", Format = "text" });
            priorityRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "criteria", Value = "[Condition] <= 3", Format = "text" });
            priorityRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "allocations", Value = "Capital: 100%; Local: 50%", Format = "text" });
            prioritiesSection.Rows.Add(priorityRow);
            document.Sections.Add(prioritiesSection);

            var treatmentsSection = new ScenarioParameterSection
            {
                Key = ScenarioParameterSectionKeys.Treatments,
                Title = "Treatments",
                Kind = ScenarioParameterSectionKinds.Table,
                Order = 90
            };
            treatmentsSection.Columns.Add(new ScenarioParameterColumn { Key = "source", Label = "Source", Order = 0 });
            treatmentsSection.Columns.Add(new ScenarioParameterColumn { Key = "name", Label = "Treatment", Order = 1 });
            treatmentsSection.Columns.Add(new ScenarioParameterColumn { Key = "category", Label = "Category", Order = 2 });
            treatmentsSection.Columns.Add(new ScenarioParameterColumn { Key = "criteria", Label = "Criteria", Order = 3 });
            var treatmentRow = new ScenarioParameterRow { Key = "treatment-1", Order = 0 };
            treatmentRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "source", Value = "Selectable", Format = "text" });
            treatmentRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "name", Value = "Rehabilitation", Format = "text" });
            treatmentRow.Cells.Add(new ScenarioParameterCell { ColumnKey = "category", Value = "Rehabilitation", Format = "text" });
            treatmentRow.Cells.Add(new ScenarioParameterCell
            {
                ColumnKey = "criteria",
                Value = string.Join(" AND ", Enumerable.Repeat("[Condition] <= 3", 40)),
                Format = "text"
            });
            treatmentsSection.Rows.Add(treatmentRow);
            document.Sections.Add(treatmentsSection);

            var queryEngine = new Mock<IUniversalQueryEngine>();
            queryEngine
                .Setup(engine => engine.StreamAllAsync(
                    It.IsAny<ReportExecutionContext>(),
                    It.IsAny<ViewerDataRequest>(),
                    12,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ComponentResult(
                    ScenarioParametersDataContract.Columns,
                    Rows(ScenarioParametersDataContract.Encode(document).ToArray())));

            try
            {
                var exporter = new UniversalReportExcelExporter(queryEngine.Object);
                var filePath = await exporter.ExportAsync(schema, simulationId, 12, CancellationToken.None);

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets["Scenario Parameters"];

                Assert.True(worksheet.Cells[4, 2, 4, 3].Merge);
                Assert.True(worksheet.Cells[4, 4, 4, 10].Merge);
                Assert.True(worksheet.Cells[4, 11, 4, 20].Merge);
                Assert.Equal("[Condition] <= 3", worksheet.Cells[5, 4].Value);
                Assert.Equal(60d, worksheet.Row(5).Height, 2);
                Assert.True(worksheet.Cells[8, 1, 8, 3].Merge);
                Assert.True(worksheet.Cells[8, 10, 8, 20].Merge);
                Assert.Equal(72d, worksheet.Row(9).Height, 2);
                Assert.True(worksheet.Cells[9, 10].Style.WrapText);
                Assert.False(worksheet.View.ShowGridLines);
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
