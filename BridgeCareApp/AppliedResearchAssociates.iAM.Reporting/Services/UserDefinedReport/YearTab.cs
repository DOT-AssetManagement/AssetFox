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
            //set default width
            yearWorksheet.DefaultColWidth = 13;

            var startColumn = 1;
            var startRow = 1;
            var currentCell = new CurrentCell { Row = startRow, Column = startColumn };
            yearWorksheet.Cells[currentCell.Row, currentCell.Column].Value = "Year " + simulationYearDetail.Year + " details";
            ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentCell.Row++, currentCell.Column]);

            // Display output details for simulation year
            currentCell.Row++;
            yearWorksheet.Cells[currentCell.Row, currentCell.Column++].Value = "ConditionOfNetwork";
            yearWorksheet.Cells[currentCell.Row, currentCell.Column].Value = simulationYearDetail.ConditionOfNetwork;
            ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentCell.Row++, currentCell.Column]);
            var currentRow = currentCell.Row;            

            // Budgets            
            currentRow = FillBudgets(yearWorksheet, userDefinedReportRequestModel.DisplayBudgets, simulationYearDetail, startColumn, currentRow);

            // DeficientConditionGoals
            currentRow = FillDeficientConditionGoals(yearWorksheet, userDefinedReportRequestModel.DisplayDeficientConditionGoals, simulationYearDetail, startColumn, currentRow);

            // TargetConditionGoals
            currentRow = FillTargetConditionGoals(yearWorksheet, userDefinedReportRequestModel.DisplayTargetConditionGoals, simulationYearDetail, startColumn, currentRow);

            // Assets
            FillAssets(yearWorksheet, userDefinedReportRequestModel.DisplayAssets, simulationYearDetail, startColumn, currentRow);

            // TODO borders to tables
            //ExcelHelper.ApplyBorder(worksheet.Cells[headerRow1, 1, headerRow2, worksheet.Dimension.Columns]);
            //ExcelHelper.ApplyStyleNoWrap(worksheet.Cells[headerRow2, 3, headerRow2, 3 + currentAttributesCount - 1]);
            //ExcelHelper.ApplyStyle(worksheet.Cells[headerRow2, 1, headerRow2, 2]);

            yearWorksheet.Cells.AutoFitColumns();
        }

        private static void FillAssets(ExcelWorksheet yearWorksheet, bool displayAssets, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayAssets)
            {
                currentRow++;
                var currentColumn = startColumn;
                // headers
                yearWorksheet.Cells[currentRow, currentColumn].Value = "Assets";
                ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow++, currentColumn]);
                foreach (var asset in simulationYearDetail.Assets)
                {
                    currentColumn = startColumn;
                    // headers and data
                    // TODO generic
                    // yearWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
                    //yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = primaryKeyValue;

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "AppliedTreatment";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.AppliedTreatment;

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentCause";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.TreatmentCause.ToString();

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentStatus";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.TreatmentStatus.ToString();
                                        
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentFundingIgnoresSpendingLimit";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.TreatmentFundingIgnoresSpendingLimit;

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "SpatialWeightForOrderingOptions";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.SpatialWeightForOrderingOptions;

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "ProjectSource";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.ProjectSource;

                    // ValuePerNumericAttribute
                    currentRow++;
                    currentColumn = startColumn;
                    // header
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Numeric attributes";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow++, currentColumn]);
                    // headers and data
                    foreach (var numericAttribute in asset.ValuePerNumericAttribute)
                    {                        
                        yearWorksheet.Cells[currentRow, currentColumn].Value = numericAttribute.Key;
                        yearWorksheet.Cells[currentRow + 1, currentColumn++].Value = numericAttribute.Value;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow + 1, currentColumn - 1]);

                    // ValuePerTextAttribute
                    currentRow += 3;
                    currentColumn = startColumn;
                    // header
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Text attributes";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow++, currentColumn]);
                    // headers and data
                    foreach (var textAttribute in asset.ValuePerTextAttribute)
                    {                        
                        yearWorksheet.Cells[currentRow, currentColumn].Value = textAttribute.Key;
                        yearWorksheet.Cells[currentRow + 1, currentColumn++].Value = textAttribute.Value;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow + 1, currentColumn - 1]);

                    // TreatmentConsiderations
                    currentRow += 3;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment considerations";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow, currentColumn]);
                    // headers and data
                    var fromRow = currentRow + 1;
                    foreach (var treatmentConsideration in asset.TreatmentConsiderations)
                    {
                        currentRow += 2;
                        currentColumn = startColumn;                                                
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
                        yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = treatmentConsideration.TreatmentName;

                        // CashFlowConsiderations
                        currentRow += 2;
                        currentColumn = startColumn;
                        // headers
                        yearWorksheet.Cells[currentRow++, currentColumn].Value = "Cash flow considerations";

                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "CashFlowRuleName";
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "ReasonAgainstCashFlow";
                        // data
                        foreach (var cashFlowConsideration in treatmentConsideration.CashFlowConsiderations)
                        {
                            currentRow++;
                            currentColumn = startColumn;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = cashFlowConsideration.CashFlowRuleName;
                            yearWorksheet.Cells[currentRow, currentColumn].Value = cashFlowConsideration.ReasonAgainstCashFlow.ToString();
                        }

                        // FundingCalculationInput
                        currentRow += 2;
                        currentColumn = startColumn;
                        // headers
                        yearWorksheet.Cells[currentRow++, currentColumn].Value = "Current budgets to spend";

                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "Name";
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "Amount";
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "Year";
                        // data
                        foreach (var budget in treatmentConsideration.FundingCalculationInput.CurrentBudgetsToSpend)
                        {
                            currentRow++;
                            currentColumn = startColumn;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = budget.Name;
                            yearWorksheet.Cells[currentRow, currentColumn].Value = budget.Amount;
                            ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn++], ExcelFormatStrings.Currency);
                            yearWorksheet.Cells[currentRow, currentColumn].Value = budget.Year;
                        }

                        // FundingCalculationOutput
                        currentRow += 2;
                        currentColumn = startColumn;
                        // headers
                        yearWorksheet.Cells[currentRow++, currentColumn].Value = "Allocation matrix";

                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "Year";
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "BudgetName";
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "AllocatedAmount";
                        // data
                        foreach (var allocation in treatmentConsideration.FundingCalculationOutput.AllocationMatrix)
                        {
                            currentRow++;
                            currentColumn = startColumn;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = allocation.Year;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = allocation.BudgetName;                            
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = allocation.TreatmentName;
                            yearWorksheet.Cells[currentRow, currentColumn].Value = allocation.AllocatedAmount;
                            ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn], ExcelFormatStrings.Currency);
                        }
                    }
                    if (asset.TreatmentConsiderations.Count == 0)
                    {
                        currentRow = fromRow;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

                    // TreatmentSchedulingCollisions                    
                    currentRow += 2;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment scheduling collisions";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow++, currentColumn]);
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Year";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "NameOfUnscheduledTreatment";
                    // data
                    fromRow = currentRow + 1;
                    foreach (var treatmentSchedulingCollision in asset.TreatmentSchedulingCollisions)
                    {                        
                        currentRow++;
                        currentColumn = startColumn;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentSchedulingCollision.Year;
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentSchedulingCollision.NameOfUnscheduledTreatment;
                    }
                    if (asset.TreatmentSchedulingCollisions.Count == 0)
                    {
                        currentRow = fromRow;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

                    // TreatmentRejections                    
                    currentRow += 2;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment rejections";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow++, currentColumn]);
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentRejectionReason";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "PotentialConditionChange";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                    // data
                    fromRow = currentRow + 1;
                    foreach (var treatmentRejection in asset.TreatmentRejections)
                    {
                        currentRow++;
                        currentColumn = startColumn;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentRejection.TreatmentName;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentRejection.TreatmentRejectionReason.ToString();
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentRejection.PotentialConditionChange;
                    }
                    if (asset.TreatmentRejections.Count == 0)
                    {
                        currentRow = fromRow;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

                    // TreatmentOptions                    
                    currentRow += 2;
                    currentColumn = startColumn;

                    // headers
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment options";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow++, currentColumn]);
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Cost";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Benefit";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "RemainingLife";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "ConditionChange";
                    ExcelHelper.ApplyStyle(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                    // data
                    fromRow = currentRow + 1;
                    foreach (var treatmentOption in asset.TreatmentOptions)
                    {
                        currentRow++;
                        currentColumn = startColumn;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.TreatmentName;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.Cost;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.Benefit;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.RemainingLife;
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentOption.ConditionChange;
                    }
                    if (asset.TreatmentOptions.Count == 0)
                    {
                        currentRow = fromRow;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

                    currentColumn = startColumn;
                    currentRow += 2;
                }
            }
        }

        private static int FillTargetConditionGoals(ExcelWorksheet yearWorksheet, bool displayTargetConditionGoals, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayTargetConditionGoals)
            {
                currentRow++;
                var currentColumn = startColumn;
                // headers
                yearWorksheet.Cells[currentRow, currentColumn].Value = "Target condition goals";
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "AttributeName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalIsMet";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "ActualValue";
                yearWorksheet.Cells[currentRow, currentColumn].Value = "TargetValue";
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                // data
                var fromRow = currentRow + 1;
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
                if (simulationYearDetail.TargetConditionGoals.Count == 0)
                {
                    currentRow = fromRow;
                }
                ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);
            }

            return ++currentRow;
        }

        private static int FillDeficientConditionGoals(ExcelWorksheet yearWorksheet, bool displayDeficientConditionGoals, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayDeficientConditionGoals)
            {
                currentRow++;
                var currentColumn = startColumn;
                // headers
                yearWorksheet.Cells[currentRow, currentColumn].Value = "Deficient condition goals";
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "AttributeName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalName";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "GoalIsMet";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "ActualDeficientPercentage";
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "AllowedDeficientPercentage";
                yearWorksheet.Cells[currentRow, currentColumn].Value = "DeficientLimit";
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                // data
                var fromRow = currentRow + 1;
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
                if (simulationYearDetail.DeficientConditionGoals.Count == 0)
                {
                    currentRow = fromRow;
                }
                ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);
            }

            return ++currentRow;
        }

        private static int FillBudgets(ExcelWorksheet yearWorksheet, bool displayBudgets, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayBudgets)
            {
                currentRow++;
                var currentColumn = startColumn;
                // headers
                yearWorksheet.Cells[currentRow, currentColumn].Value = "Budgets";
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
                yearWorksheet.Cells[currentRow, currentColumn++].Value = "BudgetName";
                yearWorksheet.Cells[currentRow, currentColumn].Value = "AvailableFunding";
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                // data
                var fromRow = currentRow + 1;
                foreach (var budget in simulationYearDetail.Budgets)
                {
                    currentRow++;
                    currentColumn = startColumn;
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = budget.BudgetName;
                    yearWorksheet.Cells[currentRow, currentColumn].Value = budget.AvailableFunding;
                    ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn], ExcelFormatStrings.CurrencyWithoutCents);
                }
                if (simulationYearDetail.Budgets.Count == 0)
                {
                    currentRow = fromRow;
                }
                ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);
            }

            return ++currentRow;
        }
    }
}
