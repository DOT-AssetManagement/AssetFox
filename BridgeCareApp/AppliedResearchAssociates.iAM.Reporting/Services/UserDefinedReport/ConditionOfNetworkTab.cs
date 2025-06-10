using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.ExcelHelpers;
using AppliedResearchAssociates.iAM.Reporting.Models;
using OfficeOpenXml;

namespace AppliedResearchAssociates.iAM.Reporting.Services.UserDefinedReport
{
    internal class ConditionOfNetworkTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public ConditionOfNetworkTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet conditionOfNetwokWorksheet, List<SimulationYearDetail> years)
        {
            var currentCell = AddHeaders(conditionOfNetwokWorksheet);

            AddDynamicData(conditionOfNetwokWorksheet, currentCell, years);

            conditionOfNetwokWorksheet.Cells.AutoFitColumns();
            conditionOfNetwokWorksheet.Column(2).Width = 20;
        }

        private static void AddDynamicData(ExcelWorksheet conditionOfNetwokWorksheet, CurrentCell currentCell, List<SimulationYearDetail> years)
        {
            var currentRow = currentCell.Row;            

            foreach (var year in years)
            {
                var currentColumn = 1;

                conditionOfNetwokWorksheet.Cells[++currentRow, currentColumn].Value = year.Year;
                ExcelHelper.ApplyBorder(conditionOfNetwokWorksheet.Cells[currentRow, currentColumn++]);

                conditionOfNetwokWorksheet.Cells[currentRow, currentColumn].Value = year.ConditionOfNetwork;                
                ExcelHelper.ApplyBorder(conditionOfNetwokWorksheet.Cells[currentRow, currentColumn]);
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet conditionOfNetwokWorksheet)
        {            
            var startRow = 1;
            var startColumn = 1;

            conditionOfNetwokWorksheet.Cells[startRow, startColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(conditionOfNetwokWorksheet.Cells[startRow, startColumn++]);
            conditionOfNetwokWorksheet.Cells[startRow, startColumn].Value = "ConditionOfNetwork";
            ExcelHelper.ApplyStyleWithBorder(conditionOfNetwokWorksheet.Cells[startRow, startColumn]);

            return new CurrentCell { Row = startRow, Column = startColumn };
        }
    }
}
