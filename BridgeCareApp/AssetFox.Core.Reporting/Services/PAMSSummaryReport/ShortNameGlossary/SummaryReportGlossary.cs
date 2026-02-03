using AssetFox.Core.ExcelHelpers;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.PAMSSummaryReport.ShortNameGlossary
{
    public class SummaryReportGlossary
    {
        public void Fill(ExcelWorksheet worksheet)
        {
            var regions = ShortNameGlossaryModels.Content;
            ExcelWorksheetWriter.VisitList(worksheet, regions);
        }
    }
}
