using System;
using System.Collections.Generic;
using System.Linq;
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

        internal void Fill(ExcelWorksheet yearWorksheet, List<string> filterAttributes, bool isPrimaryKeyNumeric, string primaryKey, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years)
        {
            //set default width
            yearWorksheet.DefaultColWidth = 18;            
                        
            // Headers
            var currentCell = AddHeaders(yearWorksheet, filterAttributes, primaryKey);

            // Add row next to headers for filters
            using (var autoFilterCells = yearWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(yearWorksheet, filterAttributes, initialAssetSummaries, years, isPrimaryKeyNumeric, primaryKey);

            yearWorksheet.Cells.AutoFitColumns();
            yearWorksheet.Column(3).SetTrueWidth(65);
        }

        private static CurrentCell AddHeaders(ExcelWorksheet yearWorksheet, List<string> filterAttributes, string primaryKey)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            yearWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            yearWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            yearWorksheet.Cells[currentRow, currentColumn].Value = "AppliedTreatment";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentCause";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentStatus";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentFundingIgnoresSpendingLimit";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            yearWorksheet.Cells[currentRow, currentColumn].Value = "SpatialWeightForOrderingOptions";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            yearWorksheet.Cells[currentRow, currentColumn].Value = "ProjectSource";
            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);

            foreach (var attribute in filterAttributes)
            {
                if (attribute.Equals(primaryKey))
                {
                    continue;
                }
                yearWorksheet.Cells[currentRow, currentColumn].Value = attribute;
                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, currentColumn++]);
            }

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }

        private void AddDynamicData(ExcelWorksheet yearWorksheet, List<string> filterAttributes, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years, bool isPrimaryKeyNumeric, string primaryKey)
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
                    var yearAssets = year.Assets;
                    var asset = isPrimaryKeyNumeric
                         ? yearAssets.FirstOrDefault(_ => CheckGetValue(_.ValuePerNumericAttribute, primaryKey).ToString() == primaryKeyValue)
                         : yearAssets.FirstOrDefault(_ => CheckGetTextValue(_.ValuePerTextAttribute, primaryKey) == primaryKeyValue);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = primaryKeyValue;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = asset.AppliedTreatment;
                    yearWorksheet.Cells[dataRow, dataColumn].Style.WrapText = true;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = asset.TreatmentCause;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = asset.TreatmentStatus;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = asset.TreatmentFundingIgnoresSpendingLimit;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = asset.SpatialWeightForOrderingOptions;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);

                    yearWorksheet.Cells[dataRow, dataColumn].Value = asset.ProjectSource;
                    ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);
                    
                    foreach (var attribute in filterAttributes)
                    {
                        if (attribute.Equals(primaryKey))
                        {
                            continue;
                        }
                        
                        yearWorksheet.Cells[dataRow, dataColumn].Value = GetAttributeValue(asset, attribute);
                        ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);
                    }

                    dataRow++;
                    dataColumn = startColumn;
                }
            }
        }

        private object GetAttributeValue(AssetSummaryDetail assetSummary, string attribute) =>
            assetSummary.ValuePerNumericAttribute.Any(_ => _.Key == attribute)
                ? CheckGetValue(assetSummary.ValuePerNumericAttribute, attribute)
                : CheckGetTextValue(assetSummary.ValuePerTextAttribute, attribute);        
                
        private double CheckGetValue(Dictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(Dictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);

        //private void FillAssets(ExcelWorksheet yearWorksheet, bool displayAssets, SimulationYearDetail simulationYearDetail, int startColumn, int currentRow)
        //{
        //    if (displayAssets)
        //    {
        //        foreach (var asset in simulationYearDetail.Assets)
        //        {
        //            currentColumn = startColumn;
        //            // headers and data
        //            var primaryKeyValue = isPrimaryKeyNumeric
        //                ? CheckGetValue(asset.ValuePerNumericAttribute, primaryKey).ToString()
        //                : CheckGetTextValue(asset.ValuePerTextAttribute, primaryKey);        //        

        //            // TreatmentOptions                    
        //            currentRow += 3;
        //            currentColumn = startColumn;

        //            // headers
        //            yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment options";
        //            ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 4, true);
        //            ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
        //            yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
        //            yearWorksheet.Cells[currentRow, currentColumn++].Value = "Cost";
        //            yearWorksheet.Cells[currentRow, currentColumn++].Value = "Benefit";
        //            yearWorksheet.Cells[currentRow, currentColumn++].Value = "RemainingLife";
        //            yearWorksheet.Cells[currentRow, currentColumn].Value = "ConditionChange";
        //            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
        //            // data
        //            fromRow = currentRow + 1;
        //            foreach (var treatmentOption in asset.TreatmentOptions)
        //            {
        //                currentRow++;
        //                currentColumn = startColumn;
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentOption.TreatmentName;
        //                yearWorksheet.Cells[currentRow, currentColumn++].Style.WrapText = true;
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.Cost;
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.Benefit;
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentOption.RemainingLife;
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentOption.ConditionChange;
        //            }
        //            if (asset.TreatmentOptions.Count == 0)
        //            {
        //                currentRow = fromRow;
        //            }
        //            ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow++, currentColumn]);

        //            // TreatmentSchedulingCollisions                    
        //            currentRow += 2;
        //            currentColumn = startColumn;
        //            // headers
        //            yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment scheduling collisions";
        //            ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 1, true);
        //            ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
        //            yearWorksheet.Cells[currentRow, currentColumn++].Value = "Year";
        //            yearWorksheet.Cells[currentRow, currentColumn].Value = "NameOfUnscheduledTreatment";
        //            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
        //            // data
        //            fromRow = currentRow + 1;
        //            foreach (var treatmentSchedulingCollision in asset.TreatmentSchedulingCollisions)
        //            {
        //                currentRow++;
        //                currentColumn = startColumn;
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentSchedulingCollision.Year;
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentSchedulingCollision.NameOfUnscheduledTreatment;
        //            }
        //            if (asset.TreatmentSchedulingCollisions.Count == 0)
        //            {
        //                currentRow = fromRow;
        //            }
        //            ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

        //            // TreatmentRejections                    
        //            currentRow += 2;
        //            currentColumn = startColumn;
        //            // headers
        //            yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment rejections";
        //            ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 2, true);
        //            ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
        //            yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
        //            yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentRejectionReason";
        //            yearWorksheet.Cells[currentRow, currentColumn].Value = "PotentialConditionChange";
        //            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
        //            // data
        //            fromRow = currentRow + 1;
        //            foreach (var treatmentRejection in asset.TreatmentRejections)
        //            {
        //                currentRow++;
        //                currentColumn = startColumn;
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentRejection.TreatmentName;
        //                yearWorksheet.Cells[currentRow, currentColumn++].Style.WrapText = true;
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = treatmentRejection.TreatmentRejectionReason.ToString();
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = treatmentRejection.PotentialConditionChange;
        //            }
        //            if (asset.TreatmentRejections.Count == 0)
        //            {
        //                currentRow = fromRow;
        //            }
        //            ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

        //            // TreatmentConsiderations
        //            currentRow += 2;
        //            currentColumn = startColumn;
        //            // headers
        //            yearWorksheet.Cells[currentRow, currentColumn].Value = "Treatment considerations";
        //            ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow, currentColumn], System.Drawing.Color.LightGray);
        //            ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow++, currentColumn]);
        //            ExcelHelper.ApplyBottomBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, startColumn + 5]);
        //            // headers and data
        //            fromRow = currentRow + 1;
        //            foreach (var treatmentConsideration in asset.TreatmentConsiderations)
        //            {
        //                currentRow += 2;
        //                currentColumn = startColumn;
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
        //                yearWorksheet.Cells[currentRow, currentColumn + 1].Value = treatmentConsideration.TreatmentName;
        //                yearWorksheet.Cells[currentRow, currentColumn + 1].Style.WrapText = true;
        //                ExcelHelper.ApplyBorder(yearWorksheet.Cells[currentRow, currentColumn, currentRow, currentColumn + 1]);

        //                // CashFlowConsiderations
        //                currentRow += 2;
        //                currentColumn = startColumn;
        //                // headers
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = "Cash flow considerations";
        //                ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 1, true);
        //                ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = "CashFlowRuleName";
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = "ReasonAgainstCashFlow";
        //                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
        //                // data
        //                fromRow = currentRow + 1;
        //                foreach (var cashFlowConsideration in treatmentConsideration.CashFlowConsiderations)
        //                {
        //                    currentRow++;
        //                    currentColumn = startColumn;
        //                    yearWorksheet.Cells[currentRow, currentColumn++].Value = cashFlowConsideration.CashFlowRuleName;
        //                    yearWorksheet.Cells[currentRow, currentColumn].Value = cashFlowConsideration.ReasonAgainstCashFlow.ToString();
        //                }
        //                if (treatmentConsideration.CashFlowConsiderations.Count == 0)
        //                {
        //                    currentRow = fromRow;
        //                }
        //                ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

        //                // FundingCalculationInput
        //                currentRow += 2;
        //                currentColumn = startColumn;
        //                // headers
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = "Current budgets to spend";
        //                ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 2, true);
        //                ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = "Name";
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = "Amount";
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = "Year";
        //                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
        //                // data
        //                fromRow = currentRow + 1;
        //                foreach (var budget in treatmentConsideration.FundingCalculationInput.CurrentBudgetsToSpend)
        //                {
        //                    currentRow++;
        //                    currentColumn = startColumn;
        //                    yearWorksheet.Cells[currentRow, currentColumn++].Value = budget.Name;
        //                    yearWorksheet.Cells[currentRow, currentColumn].Value = budget.Amount;
        //                    ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn++], ExcelFormatStrings.Currency);
        //                    yearWorksheet.Cells[currentRow, currentColumn].Value = budget.Year;
        //                }
        //                if (treatmentConsideration.FundingCalculationInput.CurrentBudgetsToSpend.Count == 0)
        //                {
        //                    currentRow = fromRow;
        //                }
        //                ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow, currentColumn]);

        //                // FundingCalculationOutput
        //                currentRow += 2;
        //                currentColumn = startColumn;
        //                // headers
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = "Allocation matrix";
        //                ExcelHelper.MergeCells(yearWorksheet, currentRow, currentColumn, currentRow, currentColumn + 3, true);
        //                ExcelHelper.ApplyColor(yearWorksheet.Cells[currentRow++, currentColumn], System.Drawing.Color.LightGray);
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = "Year";
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = "BudgetName";
        //                yearWorksheet.Cells[currentRow, currentColumn++].Value = "TreatmentName";
        //                yearWorksheet.Cells[currentRow, currentColumn].Value = "AllocatedAmount";
        //                ExcelHelper.ApplyStyleWithBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, currentColumn]);
        //                // data
        //                fromRow = currentRow + 1;
        //                foreach (var allocation in treatmentConsideration.FundingCalculationOutput.AllocationMatrix)
        //                {
        //                    currentRow++;
        //                    currentColumn = startColumn;
        //                    yearWorksheet.Cells[currentRow, currentColumn++].Value = allocation.Year;
        //                    yearWorksheet.Cells[currentRow, currentColumn++].Value = allocation.BudgetName;
        //                    yearWorksheet.Cells[currentRow, currentColumn].Value = allocation.TreatmentName;
        //                    yearWorksheet.Cells[currentRow, currentColumn++].Style.WrapText = true;
        //                    yearWorksheet.Cells[currentRow, currentColumn].Value = allocation.AllocatedAmount;
        //                    ExcelHelper.SetCurrencyFormat(yearWorksheet.Cells[currentRow, currentColumn], ExcelFormatStrings.Currency);
        //                }
        //                if (treatmentConsideration.FundingCalculationOutput.AllocationMatrix.Count == 0)
        //                {
        //                    currentRow = fromRow;
        //                }
        //                ExcelHelper.ApplyBorder(yearWorksheet.Cells[fromRow, startColumn, currentRow++, currentColumn]);
        //                ExcelHelper.ApplyBottomBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, startColumn + 5]);
        //            }
        //            currentRow++;

        //            ExcelHelper.ApplyBottomBorder(yearWorksheet.Cells[currentRow, startColumn, currentRow, startColumn + 10]);

        //            currentColumn = startColumn;
        //            currentRow += 2;
        //        }
        //    }
        //}        
    }
}
