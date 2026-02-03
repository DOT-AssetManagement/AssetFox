using System.Collections.Generic;
using AssetFox.Core.DTOs;
using AssetFox.Core.ExcelHelpers;
using OfficeOpenXml;

namespace AssetFoxCore.Services.Treatment
{
    public static class TreatmentWorksheetGenerator
    {
        public static void Fill(ExcelWorkbook workbook, List<TreatmentDTO> treatments)
        {
            foreach (var treatment in treatments)
            {
                var worksheetModel = ExcelTreatmentModels.TreatmentWorksheet(treatment);
                ExcelWorksheetAdder.AddWorksheet(workbook, worksheetModel);
            }
        }
    }
}
