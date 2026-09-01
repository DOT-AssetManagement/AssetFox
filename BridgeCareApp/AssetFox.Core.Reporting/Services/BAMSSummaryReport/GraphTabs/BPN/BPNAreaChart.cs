using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.GraphTabs.BPN
{
    public class BPNAreaChart
    {
        private StackedColumnChartCommon _stackedColumnChartCommon;

        public BPNAreaChart()
        {
            _stackedColumnChartCommon = new StackedColumnChartCommon();
        }

        public void Fill(ExcelWorksheet worksheet, ExcelWorksheet bridgeWorkSummaryWorksheet, int dataColumnRow, int simulationYearsCount, string title)
        {
            _stackedColumnChartCommon.SetWorksheetProperties(worksheet);
            var chart = worksheet.Drawings.AddChart(title, eChartType.ColumnStacked);
            _stackedColumnChartCommon.SetChartProperties(chart, title, 950, 700, 6, 7);

            SetChartAxes(chart);
            AddSeries(bridgeWorkSummaryWorksheet, dataColumnRow, simulationYearsCount, chart);

            chart.AdjustPositionAndSize();
            chart.Locked = true;
        }

        private void AddSeries(ExcelWorksheet bridgeWorkSummaryWorkSheet, int dataColumnRow, int count, ExcelChart chart)
        {
            BPNChartSeriesBuilder.AddSeries(bridgeWorkSummaryWorkSheet, dataColumnRow, count, chart, 2);
        }

        private void SetChartAxes(ExcelChart chart)
        {
            _stackedColumnChartCommon.SetChartAxes(chart);
            var yAxis = chart.YAxis;
            yAxis.DisplayUnit = 1000000;
            yAxis.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* -??_);_(@_)";
            yAxis.Title.TextVertical = OfficeOpenXml.Drawing.eTextVerticalType.Vertical;
            yAxis.Title.Font.Size = 10;
            yAxis.Title.Text = "Millions sqft";
        }
    }
}
