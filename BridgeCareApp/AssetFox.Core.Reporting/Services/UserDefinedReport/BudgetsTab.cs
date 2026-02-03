using System.Collections.Generic;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.UserDefinedReport
{
    internal class BudgetsTab
    {
        internal static void Fill(ExcelWorksheet budgetsWorksheet, List<SimulationYearDetail> years)
        {
            //set default width
            budgetsWorksheet.DefaultColWidth = 18;

            // Headers            
            var currentCell = AddHeaders(budgetsWorksheet);

            // Add row next to headers for filters
            using (var autoFilterCells = budgetsWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(budgetsWorksheet, years);

            budgetsWorksheet.Cells.AutoFitColumns();
        }

        private static void AddDynamicData(ExcelWorksheet budgetsWorksheet, List<SimulationYearDetail> years)
        {
            var dataRow = 3;
            var startColumn = 1;
            var dataColumn = startColumn;

            foreach (var year in years)
            {
                foreach (var budget in year.Budgets)
                {
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

        private static CurrentCell AddHeaders(ExcelWorksheet budgetsWorksheet)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            budgetsWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(budgetsWorksheet.Cells[currentRow, currentColumn++]);

            budgetsWorksheet.Cells[currentRow, currentColumn].Value = "BudgetName";
            ExcelHelper.ApplyStyleWithBorder(budgetsWorksheet.Cells[currentRow, currentColumn++]);

            budgetsWorksheet.Cells[currentRow, currentColumn].Value = "RemainingFunding";
            ExcelHelper.ApplyStyleWithBorder(budgetsWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }
    }
}
