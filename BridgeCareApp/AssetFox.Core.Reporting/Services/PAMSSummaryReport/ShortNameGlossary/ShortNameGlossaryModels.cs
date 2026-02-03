using System.Collections.Generic;
using AssetFox.Core.ExcelHelpers;

namespace AssetFox.Core.Reporting.Services.PAMSSummaryReport.ShortNameGlossary
{
    public static class ShortNameGlossaryModels
    {
        public static List<IExcelWorksheetContentModel> Content
            => new()
            {   
                ColorKeyRegion,
                ExcelWorksheetContentModels.ColumnWidth(1, 22),
                ExcelWorksheetContentModels.ColumnWidth(2, 22),
            };        

        public static AnchoredExcelRegionModel ColorKeyRegion
            => new()
            {
                Region = ShortNameGlossaryColorKeyModels.ColorKeyRows(),
                StartRow = 1,
                StartColumn = 1
            };

    }
}
