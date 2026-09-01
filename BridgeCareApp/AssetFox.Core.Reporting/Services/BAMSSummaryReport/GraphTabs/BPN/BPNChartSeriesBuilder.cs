using System.Drawing;
using AssetFox.Core.Reporting.Services.BAMSSummaryReport.BridgeWorkSummary.StaticContent;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.GraphTabs.BPN
{
    internal static class BPNChartSeriesBuilder
    {
        public static void AddSeries(
            ExcelWorksheet bridgeWorkSummaryWorksheet,
            int headerRow,
            int simulationYearsCount,
            ExcelChart chart,
            int firstDataColumn)
        {
            var bpnNames = EnumExtensions.GetValues<BPNName>();
            for (var index = 0; index < bpnNames.Count; index++)
            {
                var bpnName = bpnNames[index];
                var row = headerRow + index + 1;
                var values = bridgeWorkSummaryWorksheet.Cells[
                    row,
                    firstDataColumn,
                    row,
                    simulationYearsCount + 2];
                var categories = bridgeWorkSummaryWorksheet.Cells[
                    headerRow,
                    firstDataColumn,
                    headerRow,
                    simulationYearsCount + 2];
                var series = chart.Series.Add(values, categories);
                series.Header = bpnName.ToReportLabel();
                series.Fill.Color = GetColor(bpnName);
            }
        }

        private static Color GetColor(BPNName bpnName) => bpnName switch
        {
            BPNName.BPN1 => Color.FromArgb(68, 114, 196),
            BPNName.BPN2 => Color.FromArgb(237, 125, 49),
            BPNName.BPN3 => Color.FromArgb(165, 165, 165),
            BPNName.BPN4 => Color.FromArgb(255, 192, 0),
            BPNName.BPNL => Color.FromArgb(38, 68, 120),
            BPNName.BPND => Color.FromArgb(91, 155, 21),
            BPNName.BPNH => Color.FromArgb(112, 173, 71),
            BPNName.BPNN => Color.FromArgb(158, 72, 14),
            BPNName.BPNT => Color.FromArgb(99, 99, 99),
            _ => Color.Black
        };
    }
}
