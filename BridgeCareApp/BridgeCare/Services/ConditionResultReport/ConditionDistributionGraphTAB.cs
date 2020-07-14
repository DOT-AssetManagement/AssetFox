using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace BridgeCare.Services.ConditionResultReport
{
    public class ConditionDistributionGraphTAB
    {
        private readonly StackedColumnChartCommon stackedColumnChartCommon;

        public ConditionDistributionGraphTAB(StackedColumnChartCommon stackedColumnChartCommon)
        {
            this.stackedColumnChartCommon = stackedColumnChartCommon;
        }

        internal void Fill(ExcelWorksheet worksheet, ExcelWorksheet deckAreaSheetData, int totalDeckAreaRow,
            int totalBridgeCareBudgetPerYear, int simulationYearsCount)
        {
            stackedColumnChartCommon.SetWorksheetProperties(worksheet);
            var title = Properties.Resources.ConditionDistribution;
            var chart = worksheet.Drawings.AddChart(title, eChartType.ColumnStacked);
            stackedColumnChartCommon.SetChartProperties(chart, title, 950, 700, 6, 7);

            SetChartAxes(chart);
            AddSeries(deckAreaSheetData, totalDeckAreaRow, simulationYearsCount, chart);

            var secondaryChart = chart.PlotArea.ChartTypes.Add(eChartType.LineMarkers);
            CreatFundingLevelPerYearLine(deckAreaSheetData, totalBridgeCareBudgetPerYear, simulationYearsCount,
                secondaryChart, totalBridgeCareBudgetPerYear, "Funding", Color.Black);
            secondaryChart.UseSecondaryAxis = true;
            SetChartAxesForFunding(secondaryChart);

            chart.AdjustPositionAndSize();
            chart.Locked = true;
        }
        private void AddSeries(ExcelWorksheet worksheet, int totalDeckAreaRow, int count, ExcelChart chart)
        {
            CreateSeries(worksheet, totalDeckAreaRow, count, chart, totalDeckAreaRow + 3, Properties.Resources.Poor, Color.Red);

            CreateSeries(worksheet, totalDeckAreaRow, count, chart, totalDeckAreaRow + 2, Properties.Resources.Fair, Color.Yellow);

            CreateSeries(worksheet, totalDeckAreaRow, count, chart, totalDeckAreaRow + 1, Properties.Resources.Good, Color.FromArgb(0, 176, 80));
        }
        private void CreateSeries(ExcelWorksheet worksheet, int totalDeckAreaRow,  int count, ExcelChart chart, int fromRow, string header, Color color)
        {
            var serie = worksheet.Cells[fromRow, 2, fromRow, count + 2];
            var xSerie = worksheet.Cells[totalDeckAreaRow, 2, totalDeckAreaRow, count + 2];
            var excelChartSerie = chart.Series.Add(serie, xSerie);
            excelChartSerie.Header = header;
            excelChartSerie.Fill.Color = color;
        }
        private void CreatFundingLevelPerYearLine(ExcelWorksheet worksheet, int totalDeckAreaRow, int count, ExcelChart chart, int fromRow, string header, Color color)
        {
            var serie = worksheet.Cells[fromRow, 2, fromRow, count + 2];
            var xSerie = worksheet.Cells[totalDeckAreaRow, 2, totalDeckAreaRow, count + 2];
            var excelChartSerie = chart.Series.Add(serie, xSerie);
            excelChartSerie.Header = header;
            excelChartSerie.Fill.Color = color;
        }

        private void SetChartAxes(ExcelChart chart)
        {
            stackedColumnChartCommon.SetChartAxes(chart);
            var yAxis = chart.YAxis;
            yAxis.Format = "#0%";
            yAxis.MaxValue = 1;
            yAxis.Title.TextVertical = OfficeOpenXml.Drawing.eTextVerticalType.Vertical;
            yAxis.Title.Font.Size = 10;
            yAxis.Title.Text = "% in condition category";
        }

        private void SetChartAxesForFunding(ExcelChart chart)
        {
            //stackedColumnChartCommon.SetChartAxes(chart);
            var yAxis = chart.YAxis;
            yAxis.DisplayUnit = 1000000;
            yAxis.Format = "_-$* #,##0.00_-;-$* #,##0.00_-;_-$* \"-\"??_-;_-@_-";
            yAxis.Title.TextVertical = OfficeOpenXml.Drawing.eTextVerticalType.Vertical;
            yAxis.Title.Font.Size = 10;
            yAxis.Title.Text = "Funding level in Millions ($)";
        }
    }
}
