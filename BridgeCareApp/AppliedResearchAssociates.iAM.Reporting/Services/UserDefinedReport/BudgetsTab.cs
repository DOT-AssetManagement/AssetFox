using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.ExcelHelpers;
using AppliedResearchAssociates.iAM.Reporting.Models;
using OfficeOpenXml;

namespace AppliedResearchAssociates.iAM.Reporting.Services.UserDefinedReport
{
    internal class BudgetsTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public BudgetsTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet budgetsWorksheet, bool isPrimaryKeyNumeric, string primaryKey, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years)
        {
            //set default width
            budgetsWorksheet.DefaultColWidth = 18;

            // Headers            
            var currentCell = AddHeaders(budgetsWorksheet, primaryKey);

            // Add row next to headers for filters
            using (var autoFilterCells = budgetsWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(budgetsWorksheet, initialAssetSummaries, years, isPrimaryKeyNumeric, primaryKey);

            budgetsWorksheet.Cells.AutoFitColumns();
        }

        private void AddDynamicData(ExcelWorksheet budgetsWorksheet, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years, bool isPrimaryKeyNumeric, string primaryKey)
        {
            var dataRow = 3;
            var startColumn = 1;
            var dataColumn = startColumn;

            foreach (var assetSummary in initialAssetSummaries)
            {
                var primaryKeyValue = isPrimaryKeyNumeric
                ? CheckGetValue(assetSummary.ValuePerNumericAttribute, primaryKey).ToString()
                : CheckGetTextValue(assetSummary.ValuePerTextAttribute, primaryKey);

                foreach (var year in years)
                {                       
                    foreach (var budget in year.Budgets)
                    {
                        budgetsWorksheet.Cells[dataRow, dataColumn].Value = primaryKeyValue;
                        ExcelHelper.ApplyBorder(budgetsWorksheet.Cells[dataRow, dataColumn++]);

                        budgetsWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                        ExcelHelper.ApplyBorder(budgetsWorksheet.Cells[dataRow, dataColumn++]);                        

                        budgetsWorksheet.Cells[dataRow, dataColumn].Value = budget.BudgetName;
                        ExcelHelper.ApplyBorder(budgetsWorksheet.Cells[dataRow, dataColumn++]);

                        budgetsWorksheet.Cells[dataRow, dataColumn].Value = budget.AvailableFunding;
                        ExcelHelper.ApplyBorder(budgetsWorksheet.Cells[dataRow, dataColumn++]);

                        dataRow++;
                        dataColumn = startColumn;
                    }                    
                }
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet budgetsWorksheet, string primaryKey)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            budgetsWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
            ExcelHelper.ApplyStyleWithBorder(budgetsWorksheet.Cells[currentRow, currentColumn++]);

            budgetsWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(budgetsWorksheet.Cells[currentRow, currentColumn++]);

            budgetsWorksheet.Cells[currentRow, currentColumn].Value = "BudgetName";
            ExcelHelper.ApplyStyleWithBorder(budgetsWorksheet.Cells[currentRow, currentColumn++]);

            budgetsWorksheet.Cells[currentRow, currentColumn].Value = "AvailableFunding";
            ExcelHelper.ApplyStyleWithBorder(budgetsWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }
                
        private double CheckGetValue(Dictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(Dictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
