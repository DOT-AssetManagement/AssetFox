using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.ExcelHelpers;
using AppliedResearchAssociates.iAM.Reporting.Models;
using OfficeOpenXml;

namespace AppliedResearchAssociates.iAM.Reporting.Services.UserDefinedReport
{
    internal class YearAssetsTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public YearAssetsTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet yearWorksheet, UserDefinedReportRequestModel userDefinedReportRequestModel, List<SimulationYearDetail> years)
        {
            //set default width
            yearWorksheet.DefaultColWidth = 18;
            
            var startColumn = 1;            
            var startRow = 1;
            
            // Display output details for simulation year
            var currentColumn = startColumn;
            var currentRow = startRow;
            yearWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);
            yearWorksheet.Cells[currentRow, currentColumn].Value = simulationYearDetail.Year;
            ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
            currentColumn = startColumn;
            yearWorksheet.Cells[currentRow, currentColumn].Value = "ConditionOfNetwork";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);
            yearWorksheet.Cells[currentRow, currentColumn].Value = simulationYearDetail.ConditionOfNetwork;
            ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow++, currentColumn]);

            // Budgets            
            currentRow = FillBudgets(yearWorksheet, userDefinedReportRequestModel.DisplayBudgets, simulationYearDetail, startColumn, currentRow);

            // DeficientConditionGoals
            currentRow = FillDeficientConditionGoals(yearWorksheet, userDefinedReportRequestModel.DisplayDeficientConditionGoals, simulationYearDetail, startColumn, currentRow);

            // TargetConditionGoals
            currentRow = FillTargetConditionGoals(yearWorksheet, userDefinedReportRequestModel.DisplayTargetConditionGoals, simulationYearDetail, startColumn, currentRow);

            // Assets
            FillAssets(yearWorksheet, userDefinedReportRequestModel.DisplayYearAssets, simulationYearDetail, startColumn, currentRow);

            yearWorksheet.Cells.AutoFitColumns();
        }

        private void FillAssets(ExcelWorksheet yearWorksheet, bool displayAssets, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        {
            if (displayAssets)
            {
                currentRow++;
                var currentColumn = startColumn;
                // headers
                yearWorksheet.Cells[currentRow, currentColumn].Value = "Assets";                
                ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow, currentColumn], System.Drawing.Color.Gray);
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
                ExcelHelper.ApplyBottomBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow++, startColumn + 10]);

                var primaryKeyFields = _unitOfWork.AdminSettingsRepo.GetKeyFields();
                var primaryKey = primaryKeyFields[0].ToString();
                var isPrimaryKeyNumeric = _reportHelper.IsPrimaryKeyNumberic(simulationYearDetail.Assets[0].ValuePerTextAttribute, simulationYearDetail.Assets[0].ValuePerNumericAttribute, primaryKey);
                currentRow++;

                foreach (var asset in simulationYearDetail.Assets)
                {
                    currentColumn = startColumn;
                    // headers and data
                    var primaryKeyValue = isPrimaryKeyNumeric
                        ? CheckGetValue(asset.ValuePerNumericAttribute, primaryKey).ToString()
                        : CheckGetTextValue(asset.ValuePerTextAttribute, primaryKey);                    

                    yearWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = primaryKeyValue;                    
                    ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow - 1, currentColumn, currentRow - 1, currentColumn + 1], System.Drawing.Color.LightBlue);
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow - 1, currentColumn, currentRow - 1, currentColumn + 1]);

                    var fromRow = currentRow;
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "AppliedTreatment";
                    yearWorksheet.Cells[currentRow, currentColumn + 1].Value = asset.AppliedTreatment;
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Style.WrapText = true;

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentCause";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.TreatmentCause.ToString();

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentStatus";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.TreatmentStatus.ToString();
                                        
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentFundingIgnoresSpendingLimit";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.TreatmentFundingIgnoresSpendingLimit;

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "SpatialWeightForOrderingOptions";
                    yearWorksheet.Cells[currentRow++, currentColumn + 1].Value = asset.SpatialWeightForOrderingOptions;

                    yearWorksheet.Cells[currentRow, currentColumn].Value = "ProjectSource";
                    yearWorksheet.Cells[currentRow, currentColumn + 1].Value = asset.ProjectSource;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, currentColumn, currentRow++, currentColumn + 1]);

                    // ValuePerNumericAttribute
                    currentRow++;
                    currentColumn = startColumn;
                    // header
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Numeric attributes";
                    ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow, currentColumn], System.Drawing.Color.LightGray);
                    ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
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
                    ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow, currentColumn], System.Drawing.Color.LightGray);
                    ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
                    // headers and data
                    foreach (var textAttribute in asset.ValuePerTextAttribute)
                    {                        
                        yearWorksheet.Cells[currentRow, currentColumn].Value = textAttribute.Key;
                        yearWorksheet.Cells[currentRow + 1, currentColumn++].Value = textAttribute.Value;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow + 1, currentColumn - 1]);

                    // TreatmentOptions                    
                    currentRow += 3;
                    currentColumn = startColumn;

                    // headers
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment options";
                    ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 4, true);
                    ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Cost";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Benefit";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "RemainingLife";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "ConditionChange";
                    ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                    // data
                    fromRow = currentRow + 1;
                    foreach (var treatmentOption in asset.TreatmentOptions)
                    {
                        currentRow++;
                        currentColumn = startColumn;
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentOption.TreatmentName;
                        yearWorksheet.Cells[currentRow, currentColumn++].Style.WrapText = true;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.Cost;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.Benefit;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.RemainingLife;
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentOption.ConditionChange;
                    }
                    if (asset.TreatmentOptions.Count == 0)
                    {
                        currentRow = fromRow;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow++, currentColumn]);

                    // TreatmentSchedulingCollisions                    
                    currentRow += 2;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment scheduling collisions";
                    ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 1, true);
                    ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "Year";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "NameOfUnscheduledTreatment";
                    ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
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
                    ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 2, true);
                    ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                    yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentRejectionReason";
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "PotentialConditionChange";
                    ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                    // data
                    fromRow = currentRow + 1;
                    foreach (var treatmentRejection in asset.TreatmentRejections)
                    {
                        currentRow++;
                        currentColumn = startColumn;
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentRejection.TreatmentName;
                        yearWorksheet.Cells[currentRow, currentColumn++].Style.WrapText = true;
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentRejection.TreatmentRejectionReason.ToString();
                        yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentRejection.PotentialConditionChange;
                    }
                    if (asset.TreatmentRejections.Count == 0)
                    {
                        currentRow = fromRow;
                    }
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

                    // TreatmentConsiderations
                    currentRow += 2;
                    currentColumn = startColumn;
                    // headers
                    yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment considerations";
                    ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow, currentColumn], System.Drawing.Color.LightGray);
                    ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
                    ExcelHelper.ApplyBottomBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, startColumn + 5]);
                    // headers and data
                    fromRow = currentRow + 1;
                    foreach (var treatmentConsideration in asset.TreatmentConsiderations)
                    {
                        currentRow += 2;
                        currentColumn = startColumn;                                                
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
                        yearWorksheet.Cells[currentRow, currentColumn + 1].Value = treatmentConsideration.TreatmentName;
                        yearWorksheet.Cells[currentRow, currentColumn + 1].Style.WrapText = true;
                        ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow, currentColumn, currentRow, currentColumn + 1]);

                        // CashFlowConsiderations
                        currentRow += 2;
                        currentColumn = startColumn;
                        // headers
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "Cash flow considerations";
                        ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 1, true);
                        ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);                        
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "CashFlowRuleName";
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "ReasonAgainstCashFlow";
                        ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                        // data
                        fromRow = currentRow + 1;
                        foreach (var cashFlowConsideration in treatmentConsideration.CashFlowConsiderations)
                        {
                            currentRow++;
                            currentColumn = startColumn;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = cashFlowConsideration.CashFlowRuleName;
                            yearWorksheet.Cells[currentRow, currentColumn].Value = cashFlowConsideration.ReasonAgainstCashFlow.ToString();
                        }
                        if (treatmentConsideration.CashFlowConsiderations.Count == 0)
                        {
                            currentRow = fromRow;
                        }
                        ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

                        // FundingCalculationInput
                        currentRow += 2;
                        currentColumn = startColumn;
                        // headers
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "Current budgets to spend";
                        ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 2, true);
                        ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "Name";
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "Amount";
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "Year";
                        ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                        // data
                        fromRow = currentRow + 1;
                        foreach (var budget in treatmentConsideration.FundingCalculationInput.CurrentBudgetsToSpend)
                        {
                            currentRow++;
                            currentColumn = startColumn;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = budget.Name;
                            yearWorksheet.Cells[currentRow, currentColumn].Value = budget.Amount;
                            ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn++], ExcelFormatStrings.Currency);
                            yearWorksheet.Cells[currentRow, currentColumn].Value = budget.Year;
                        }
                        if (treatmentConsideration.FundingCalculationInput.CurrentBudgetsToSpend.Count == 0)
                        {
                            currentRow = fromRow;
                        }
                        ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

                        // FundingCalculationOutput
                        currentRow += 2;
                        currentColumn = startColumn;
                        // headers
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "Allocation matrix";
                        ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 3, true);
                        ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);                        
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "Year";
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "BudgetName";
                        yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
                        yearWorksheet.Cells[currentRow, currentColumn].Value = "AllocatedAmount";
                        ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
                        // data
                        fromRow = currentRow + 1;
                        foreach (var allocation in treatmentConsideration.FundingCalculationOutput.AllocationMatrix)
                        {
                            currentRow++;
                            currentColumn = startColumn;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = allocation.Year;
                            yearWorksheet.Cells[currentRow, currentColumn++].Value = allocation.BudgetName;                            
                            yearWorksheet.Cells[currentRow, currentColumn].Value = allocation.TreatmentName;
                            yearWorksheet.Cells[currentRow, currentColumn++].Style.WrapText = true;
                            yearWorksheet.Cells[currentRow, currentColumn].Value = allocation.AllocatedAmount;
                            ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn], ExcelFormatStrings.Currency);
                        }
                        if (treatmentConsideration.FundingCalculationOutput.AllocationMatrix.Count == 0)
                        {
                            currentRow = fromRow;
                        }
                        ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow++, currentColumn]);
                        ExcelHelper.ApplyBottomBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, startColumn + 5]);
                    }                    
                    currentRow++;                    

                    ExcelHelper.ApplyBottomBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, startColumn + 10]);

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
                ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 4, true);
                ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.Gray);                
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
                ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 5, true);
                ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.Gray);                
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
                ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 1, true);
                ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.Gray);                
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

        private double CheckGetValue(Dictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(Dictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
