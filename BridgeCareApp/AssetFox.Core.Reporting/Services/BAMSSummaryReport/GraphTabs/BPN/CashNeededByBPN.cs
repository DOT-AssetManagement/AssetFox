using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.GraphTabs.BPN
{
    public class CashNeededByBPN
    {
        private StackedColumnChartCommon _stackedColumnChartCommon;

        public CashNeededByBPN()
        {
            _stackedColumnChartCommon = new StackedColumnChartCommon();
        }
        public void Fill(ExcelWorksheet worksheet, ExcelWorksheet bridgeWorkSummaryWorksheet, int totalCashNeededByBPNYearsRow, int simulationYearsCount, string title)
        {
            _stackedColumnChartCommon.SetWorksheetProperties(worksheet);
            var chart = worksheet.Drawings.AddChart(title, eChartType.ColumnStacked);
            _stackedColumnChartCommon.SetChartProperties(chart, title, 950, 700, 6, 7);

            SetChartAxes(chart);
            AddSeries(bridgeWorkSummaryWorksheet, totalCashNeededByBPNYearsRow, simulationYearsCount, chart);

            chart.AdjustPositionAndSize();
            chart.Locked = true;
        }
        private void AddSeries(ExcelWorksheet bridgeWorkSummaryWorkSheet, int totalCashNeededByBPNYearsRow, int count, ExcelChart chart)
        {
            BPNChartSeriesBuilder.AddSeries(bridgeWorkSummaryWorkSheet, totalCashNeededByBPNYearsRow, count, chart, 3);

            CreateLine(bridgeWorkSummaryWorkSheet, totalCashNeededByBPNYearsRow, count, chart, totalCashNeededByBPNYearsRow + 10, BAMSConstants.AnnualizedAmount, Color.Blue);
        }

        private void CreateLine(ExcelWorksheet bridgeWorkSummaryWorkSheet, int totalCashNeededByBPNYearsRow, int count, ExcelChart chart, int fromRow, string header, Color color)
        {
            var serie = bridgeWorkSummaryWorkSheet.Cells[fromRow, 3, fromRow, count + 2];
            var xSerie = bridgeWorkSummaryWorkSheet.Cells[totalCashNeededByBPNYearsRow, 3, totalCashNeededByBPNYearsRow, count + 2];

            var lineChart = chart.PlotArea.ChartTypes.Add(eChartType.Line);
            var excelChartSerie = lineChart.Series.Add(serie, xSerie);
            excelChartSerie.Header = header;
            excelChartSerie.Border.Fill.Color = color;
        }

        private void SetChartAxes(ExcelChart chart)
        {
            _stackedColumnChartCommon.SetChartAxes(chart);
            var yAxis = chart.YAxis;
            yAxis.DisplayUnit = 1000000;
            yAxis.Format = "_-$* #,##0_-;$* (#,##0)_-;_-$* \"-\"??_-;_-@_-";
            yAxis.Title.TextVertical = OfficeOpenXml.Drawing.eTextVerticalType.Vertical;
            yAxis.Title.Font.Size = 10;
            yAxis.Title.Text = "Millions ($)";
        }
    }
}
