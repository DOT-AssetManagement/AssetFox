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
            FillAssets(yearWorksheet, userDefinedReportRequestModel.DisplayAssets, simulationYearDetail, startColumn, currentRow);
        }

        private static void FillAssets(ExcelWorksheet yearWorksheet, bool displayAssets, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayAssets)
            {
                currentRow++;
                var currentColumn = startColumn;
                // headers
                yearWorksheet.Cells[currentRow++, currentColumn].Value = "Assets";
                foreach (var asset in simulationYearDetail.Assets)
                {
                    currentColumn = startColumn;
                    // headers and data
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "AssetName";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.AssetName;

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
                    // headers and data
                    foreach (var numericAttribute in asset.ValuePerNumericAttribute)
                    {                        
                        yearWorksheet.Cells[currentRow, currentColumn].Value = numericAttribute.Key;
                        yearWorksheet.Cells[currentRow + 1, currentColumn++].Value = numericAttribute.Value;
                    }

                    // ValuePerTextAttribute
                    currentRow += 3;
                    currentColumn = startColumn;
                    // header
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Text attributes";
                    // headers and data
                    foreach (var textAttribute in asset.ValuePerTextAttribute)
                    {                        
                        yearWorksheet.Cells[currentRow, currentColumn].Value = textAttribute.Key;
                        yearWorksheet.Cells[currentRow + 1, currentColumn++].Value = textAttribute.Value;
                    }

                    // TreatmentConsiderations
                    currentRow += 3;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow++, currentColumn].Value = "Treatment considerations";
                    // headers and data
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

                    // TreatmentSchedulingCollisions                    
                    currentRow += 2;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow++, currentColumn].Value = "Treatment scheduling collisions";

                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Year";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "NameOfUnscheduledTreatment";
                    // data
                    foreach (var treatmentSchedulingCollision in asset.TreatmentSchedulingCollisions)
                    {                        
                        currentRow++;
                        currentColumn = startColumn;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentSchedulingCollision.Year;
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentSchedulingCollision.NameOfUnscheduledTreatment;
                    }

                    // TreatmentRejections                    
                    currentRow += 2;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow++, currentColumn].Value = "Treatment rejections";

                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentRejectionReason";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "PotentialConditionChange";
                    // data
                    foreach (var treatmentRejection in asset.TreatmentRejections)
                    {
                        currentRow++;
                        currentColumn = startColumn;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentRejection.TreatmentName;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentRejection.TreatmentRejectionReason.ToString();
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentRejection.PotentialConditionChange;
                    }

                    // TreatmentOptions                    
                    currentRow += 2;
                    currentColumn = startColumn;

                    // headers
                    yearWorksheet.Cells[currentRow++, currentColumn].Value = "Treatment options";

                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Cost";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Benefit";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "RemainingLife";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "conditionChange";
                    // data
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
                yearWorksheet.Cells[currentRow++, currentColumn].Value = "Target condition goals";

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

            return ++currentRow;
        }

        private static int FillDeficientConditionGoals(ExcelWorksheet yearWorksheet, bool displayDeficientConditionGoals, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayDeficientConditionGoals)
            {
                currentRow++;
                var currentColumn = startColumn;
                // headers
                yearWorksheet.Cells[currentRow++, currentColumn].Value = "Deficient condition goals";

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

            return ++currentRow;
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
                    ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn], ExcelFormatStrings.Currency);
                }
            }

            return ++currentRow;
        }
    }
}
