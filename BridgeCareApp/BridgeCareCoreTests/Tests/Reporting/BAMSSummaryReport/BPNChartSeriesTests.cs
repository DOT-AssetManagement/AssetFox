using AssetFox.Core.Reporting.Services.BAMSSummaryReport.GraphTabs.BPN;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using Xunit;

namespace AssetFoxCoreTests.Tests.Reporting.BAMSSummaryReport;

public class BPNChartSeriesTests
{
    private static readonly string[] ExpectedSeriesOrder =
    {
        "BPN 1",
        "BPN 2",
        "BPN 3",
        "BPN 4",
        "BPN L",
        "BPN D",
        "BPN H",
        "BPN N",
        "BPN T"
    };

    [Theory]
    [InlineData("area")]
    [InlineData("count")]
    [InlineData("cash")]
    public void Fill_UsesTheSameBpnOrderAsTheSummaryTable(string chartType)
    {
        using var package = new ExcelPackage();
        var chartWorksheet = package.Workbook.Worksheets.Add("Chart");
        var dataWorksheet = package.Workbook.Worksheets.Add("Bridge Work Summary");
        const int headerRow = 2;
        const int simulationYearsCount = 1;
        const string chartName = "BPN Chart";

        dataWorksheet.Cells[headerRow, 2].Value = 2025;
        dataWorksheet.Cells[headerRow, 3].Value = 2026;
        for (var index = 0; index < ExpectedSeriesOrder.Length; index++)
        {
            dataWorksheet.Cells[headerRow + index + 1, 2].Value = index + 1;
            dataWorksheet.Cells[headerRow + index + 1, 3].Value = index + 11;
        }

        switch (chartType)
        {
            case "area":
                new BPNAreaChart().Fill(
                    chartWorksheet,
                    dataWorksheet,
                    headerRow,
                    simulationYearsCount,
                    chartName);
                break;
            case "count":
                new BPNCountChart().Fill(
                    chartWorksheet,
                    dataWorksheet,
                    headerRow,
                    simulationYearsCount,
                    chartName);
                break;
            case "cash":
                new CashNeededByBPN().Fill(
                    chartWorksheet,
                    dataWorksheet,
                    headerRow,
                    simulationYearsCount,
                    chartName);
                break;
        }

        var chart = Assert.IsAssignableFrom<ExcelChart>(chartWorksheet.Drawings[chartName]);
        Assert.Equal(ExpectedSeriesOrder.Length, chart.Series.Count);
        var actualSeriesOrder = Enumerable.Range(0, chart.Series.Count)
            .Select(index => chart.Series[index].Header);
        Assert.Equal(ExpectedSeriesOrder, actualSeriesOrder);
    }
}
