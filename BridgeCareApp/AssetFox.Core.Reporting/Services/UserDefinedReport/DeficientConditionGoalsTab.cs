using System.Collections.Generic;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.UserDefinedReport
{
    internal class DeficientConditionGoalsTab
    {
        internal static void Fill(ExcelWorksheet deficientConditionGoalsWorksheet, List<SimulationYearDetail> years)
        {
            //set default width
            deficientConditionGoalsWorksheet.DefaultColWidth = 18;

            // Headers            
            var currentCell = AddHeaders(deficientConditionGoalsWorksheet);

            // Add row next to headers for filters
            using (var autoFilterCells = deficientConditionGoalsWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(deficientConditionGoalsWorksheet, years);

            deficientConditionGoalsWorksheet.Cells.AutoFitColumns();
        }

        private static void AddDynamicData(ExcelWorksheet deficientConditionGoalsWorksheet, List<SimulationYearDetail> years)
        {
            var dataRow = 3;
            var startColumn = 1;
            var dataColumn = startColumn;

            foreach (var year in years)
            {
                foreach (var deficientConditionGoal in year.DeficientConditionGoals)
                {
                    deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                    ExcelHelper.ApplyBorder(deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = deficientConditionGoal.AttributeName;
                    ExcelHelper.ApplyBorder(deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = deficientConditionGoal.GoalName;
                    ExcelHelper.ApplyBorder(deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = deficientConditionGoal.GoalIsMet;
                    ExcelHelper.ApplyBorder(deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = deficientConditionGoal.ActualDeficientPercentage;
                    ExcelHelper.ApplyBorder(deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = deficientConditionGoal.AllowedDeficientPercentage;
                    ExcelHelper.ApplyBorder(deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = deficientConditionGoal.DeficientLimit;
                    ExcelHelper.ApplyBorder(deficientConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    dataRow++;
                    dataColumn = startColumn;
                }
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet deficientConditionGoalsWorksheet)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "AttributeName";
            ExcelHelper.ApplyStyleWithBorder(deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "GoalName";
            ExcelHelper.ApplyStyleWithBorder(deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "GoalIsMet";
            ExcelHelper.ApplyStyleWithBorder(deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);            

            deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "ActualDeficientPercentage";
            ExcelHelper.ApplyStyleWithBorder(deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "AllowedDeficientPercentage";
            ExcelHelper.ApplyStyleWithBorder(deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "DeficientLimit";
            ExcelHelper.ApplyStyleWithBorder(deficientConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }
    }
}
