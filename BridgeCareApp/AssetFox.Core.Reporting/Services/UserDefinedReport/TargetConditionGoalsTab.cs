using System.Collections.Generic;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.UserDefinedReport
{
    internal class TargetConditionGoalsTab
    {
        internal static void Fill(ExcelWorksheet targetConditionGoalsWorksheet, List<SimulationYearDetail> years)
        {
            //set default width
            targetConditionGoalsWorksheet.DefaultColWidth = 18;

            // Headers            
            var currentCell = AddHeaders(targetConditionGoalsWorksheet);

            // Add row next to headers for filters
            using (var autoFilterCells = targetConditionGoalsWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(targetConditionGoalsWorksheet, years);

            targetConditionGoalsWorksheet.Cells.AutoFitColumns();
            targetConditionGoalsWorksheet.Column(5).SetTrueWidth(14.50);
            targetConditionGoalsWorksheet.Column(6).SetTrueWidth(16.50);
        }

        private static void AddDynamicData(ExcelWorksheet targetConditionGoalsWorksheet, List<SimulationYearDetail> years)
        {
            var dataRow = 3;
            var startColumn = 1;
            var dataColumn = startColumn;

            foreach (var year in years)
            {
                foreach (var targetConditionGoal in year.TargetConditionGoals)
                {
                    targetConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                    ExcelHelper.ApplyBorder(targetConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    targetConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = targetConditionGoal.AttributeName;
                    ExcelHelper.ApplyBorder(targetConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    targetConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = targetConditionGoal.GoalName;
                    ExcelHelper.ApplyBorder(targetConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    targetConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = targetConditionGoal.GoalIsMet;
                    ExcelHelper.ApplyBorder(targetConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    targetConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = targetConditionGoal.ActualValue;
                    ExcelHelper.ApplyBorder(targetConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    targetConditionGoalsWorksheet.Cells[dataRow, dataColumn].Value = targetConditionGoal.TargetValue;
                    ExcelHelper.ApplyBorder(targetConditionGoalsWorksheet.Cells[dataRow, dataColumn++]);

                    dataRow++;
                    dataColumn = startColumn;
                }
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet targetConditionGoalsWorksheet)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            targetConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(targetConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            targetConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "AttributeName";
            ExcelHelper.ApplyStyleWithBorder(targetConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            targetConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "GoalName";
            ExcelHelper.ApplyStyleWithBorder(targetConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            targetConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "GoalIsMet";
            ExcelHelper.ApplyStyleWithBorder(targetConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);            

            targetConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "ActualValue";
            ExcelHelper.ApplyStyleWithBorder(targetConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            targetConditionGoalsWorksheet.Cells[currentRow, currentColumn].Value = "TargetValue";
            ExcelHelper.ApplyStyleWithBorder(targetConditionGoalsWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }
    }
}
