using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.GraphTabs.BPN
{
    public class BPNCountChart
    {
        private StackedColumnChartCommon _stackedColumnChartCommon;

        public BPNCountChart()
        {
            _stackedColumnChartCommon = new StackedColumnChartCommon();
        }

        public void Fill(ExcelWorksheet worksheet, ExcelWorksheet bridgeWorkSummaryWorksheet, int dataColumnRow, int simulationYearsCount, string title)
        {
            _stackedColumnChartCommon.SetWorksheetProperties(worksheet);
            var chart = worksheet.Drawings.AddChart(title, eChartType.ColumnStacked);
            _stackedColumnChartCommon.SetChartProperties(chart, title, 950, 700, 6, 7);

            _stackedColumnChartCommon.SetChartAxes(chart);
            AddSeries(bridgeWorkSummaryWorksheet, dataColumnRow, simulationYearsCount, chart);

            chart.AdjustPositionAndSize();
            chart.Locked = true;
        }

        private void AddSeries(ExcelWorksheet bridgeWorkSummaryWorkSheet, int dataColumnRow, int count, ExcelChart chart)
        {
            BPNChartSeriesBuilder.AddSeries(bridgeWorkSummaryWorkSheet, dataColumnRow, count, chart, 2);
        }
    }
}
