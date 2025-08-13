using System.Drawing;
using AppliedResearchAssociates.iAM.ExcelHelpers;

namespace AppliedResearchAssociates.iAM.Reporting.Services.PAMSSummaryReport.ShortNameGlossary
{
    internal class ShortNameGlossaryColorKeyModels
    {
        internal static RowBasedExcelRegionModel ColorKeyRows() => RowBasedExcelRegionModels.WithRows(
            CenteredHeader("Color Key"),
            TwoByOneRow(ExcelValueModels.Nothing),

            CenteredHeader("Work Done Columns"),
            TwoByOneRow(ColoredText("PMS Treatment is being cashed flowed.", Color.White, Color.FromArgb(7384391))),
            TwoByOneRow(ColoredText("MPMS Project selected for consecutive years.", Color.White, Color.Orange)),

            TwoByOneRow(ExcelValueModels.Nothing),
            CenteredHeader("Details Columns"),
            TwoByOneRow(ColoredText("Project is being cashed flowed.", Color.White, Color.FromArgb(7384391))),

            TwoByOneRow(ExcelValueModels.Nothing),
            CenteredLongHeader("This software is provided on “as is” basis, and by using the software you acknowledge and agree that you are doing so at your own risk. While reasonable efforts have been made to ensure the accuracy and reliability of the software, no guarantee is made regarding the correctness, completeness, or suitability of any models, predictions or results it generates. Under no circumstances shall the software developers or distributors be held liable for any loss, damage, or other consequences arising from the use of the software or its outputs. Users assume full responsibility for any decisions made based on software use.")
        );

        private static ExcelRowModel CenteredLongHeader(string text)
            => ExcelRowModels.WithCells(
                new RelativeExcelRangeModel
                {
                    Content = StackedExcelModels.Stacked(
                        ExcelValueModels.String(text),
                        ExcelStyleModels.Bold,
                        ExcelStyleModels.HorizontalCenter,
                        ExcelStyleModels.WrapText),
                    Size = new ExcelRangeSize(10, 6)
                });

        private static ExcelRowModel CenteredHeader(string text)
            => ExcelRowModels.WithCells(
                new RelativeExcelRangeModel
                {
                    Content = StackedExcelModels.Stacked(
                        ExcelValueModels.String(text),
                        ExcelStyleModels.Bold,
                        ExcelStyleModels.HorizontalCenter),
                    Size = new ExcelRangeSize(2, 1)
                });

        private static IExcelModel ColoredText(string text, Color textColor, Color fillColor)
            => StackedExcelModels.Stacked(
                ExcelValueModels.String(text),
                ExcelStyleModels.FontColor(textColor),
                ExcelStyleModels.BackgroundColor(fillColor),
                ExcelStyleModels.ThinBorder);


        private static ExcelRowModel TwoByOneRow(IExcelModel content)
            => ExcelRowModels.WithCells(TwoByOne(content));

        private static IExcelModel ItalicYear(int year)
            => StackedExcelModels.Stacked(
                ExcelValueModels.Integer(year),
                ExcelStyleModels.HorizontalCenter,
                ExcelStyleModels.Italic);

        private static RelativeExcelRangeModel TwoByOne(IExcelModel content)
            => new RelativeExcelRangeModel
            {
                Content = content,
                Size = new ExcelRangeSize(2, 1)
            };
    }
}
