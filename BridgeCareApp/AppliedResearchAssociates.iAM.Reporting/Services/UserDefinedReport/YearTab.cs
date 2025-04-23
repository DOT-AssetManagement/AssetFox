using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.ExcelHelpers;
using AppliedResearchAssociates.iAM.Reporting.Models;
using OfficeOpenXml;

namespace AppliedResearchAssociates.iAM.Reporting.Services.UserDefinedReport
{
    internal class YearTab
    {
        internal void Fill(ExcelWorksheet yearWorksheet, UserDefinedReportRequestModel userDefinedReportRequestModel, SimulationYearDetail simulationYearDetail)
        {
            var startColumn = 1;
            var startRow = 1;
            CurrentCell currentCell = new CurrentCell { Row = startRow, Column = startColumn };
            yearWorksheet.Cells[currentCell.Row++, currentCell.Column].Value = "Year " + simulationYearDetail.Year + " details";

            // Display output details for simulation year
            currentCell.Row += 2;
            yearWorksheet.Cells[currentCell.Row++, currentCell.Column].Value = simulationYearDetail.ConditionOfNetwork;
            var currentRow = currentCell.Row;

            // Budgets            
            currentRow = FillBudgets(yearWorksheet, userDefinedReportRequestModel.DisplayBudgets, simulationYearDetail, startColumn, currentRow);

            // DeficientConditionGoals
            currentRow = FillDeficientConditionGoals(yearWorksheet, userDefinedReportRequestModel.DisplayDeficientConditionGoals, simulationYearDetail, startColumn, currentRow);

            // TargetConditionGoals
            currentRow = FillTargetConditionGoals(yearWorksheet, userDefinedReportRequestModel.DisplayTargetConditionGoals, simulationYearDetail, startColumn, currentRow);

            // Assets
            currentRow = FillAssets(yearWorksheet, userDefinedReportRequestModel.DisplayAssets, simulationYearDetail, startColumn, currentRow);
        }

        private static int FillAssets(ExcelWorksheet yearWorksheet, bool displayAssets, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayAssets)
            {
                currentRow++;
                var currentColumn = startColumn;

                // TODO
            }

            return currentRow;
        }

        private static int FillTargetConditionGoals(ExcelWorksheet yearWorksheet, bool displayTargetConditionGoals, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayTargetConditionGoals)
            {
                currentRow++;
                var currentColumn = startColumn;

                // headers
                yearWorksheet.Cells[currentRow++, currentColumn].Value = "TargetConditionGoals";

                yearWorksheet.Cells[currentRow, currentColumn++].Value = "AttributeName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalIsMet";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "ActualValue";
                yearWorksheet.Cells[currentRow, currentColumn].Value = "TargetValue";

                // data                
                foreach (var targetConditionGoal in simulationYearDetail.TargetConditionGoals)
                {
                    currentRow++;
                    currentColumn = startColumn;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = targetConditionGoal.AttributeName;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = targetConditionGoal.GoalName;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = targetConditionGoal.GoalIsMet;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = targetConditionGoal.ActualValue;
                    yearWorksheet.Cells[currentRow, currentColumn].Value = targetConditionGoal.TargetValue;
                }
            }

            return currentRow;
        }

        private static int FillDeficientConditionGoals(ExcelWorksheet yearWorksheet, bool displayDeficientConditionGoals, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayDeficientConditionGoals)
            {
                currentRow++;
                var currentColumn = startColumn;

                // headers
                yearWorksheet.Cells[currentRow++, currentColumn].Value = "DeficientConditionGoals";

                yearWorksheet.Cells[currentRow, currentColumn++].Value = "AttributeName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalIsMet";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "ActualDeficientPercentage";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "AllowedDeficientPercentage";
                yearWorksheet.Cells[currentRow, currentColumn].Value = "DeficientLimit";

                // data                
                foreach (var deficientConditionGoal in simulationYearDetail.DeficientConditionGoals)
                {
                    currentRow++;
                    currentColumn = startColumn;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = deficientConditionGoal.AttributeName;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = deficientConditionGoal.GoalName;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = deficientConditionGoal.GoalIsMet;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = deficientConditionGoal.ActualDeficientPercentage;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = deficientConditionGoal.AllowedDeficientPercentage;
                    yearWorksheet.Cells[currentRow, currentColumn].Value = deficientConditionGoal.DeficientLimit;

                }
            }

            return currentRow;
        }

        private static int FillBudgets(ExcelWorksheet yearWorksheet, bool displayBudgets, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayBudgets)
            {
                currentRow++;
                var currentColumn = startColumn;

                // headers
                yearWorksheet.Cells[currentRow++, currentColumn].Value = "Budgets";

                yearWorksheet.Cells[currentRow, currentColumn++].Value = "BudgetName";
                yearWorksheet.Cells[currentRow, currentColumn].Value = "AvailableFunding";
                ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);

                // data                
                foreach (var budget in simulationYearDetail.Budgets)
                {
                    currentRow++;
                    currentColumn = startColumn;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = budget.BudgetName;
                    yearWorksheet.Cells[currentRow, currentColumn].Value = budget.AvailableFunding;
                }
            }

            return currentRow;
        }
    }
}
