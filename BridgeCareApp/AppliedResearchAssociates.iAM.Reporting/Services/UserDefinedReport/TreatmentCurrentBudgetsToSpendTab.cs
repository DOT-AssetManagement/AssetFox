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
    internal class TreatmentCurrentBudgetsToSpendTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public TreatmentCurrentBudgetsToSpendTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet treatmentCurrentBudgetsToSpendWorksheet, bool isPrimaryKeyNumeric, string primaryKey, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years)
        {
            //set default width
            treatmentCurrentBudgetsToSpendWorksheet.DefaultColWidth = 18;

            // Headers
            var currentCell = AddHeaders(treatmentCurrentBudgetsToSpendWorksheet, primaryKey);

            // Add row next to headers for filters
            using (var autoFilterCells = treatmentCurrentBudgetsToSpendWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(treatmentCurrentBudgetsToSpendWorksheet, initialAssetSummaries, years, isPrimaryKeyNumeric, primaryKey);

            treatmentCurrentBudgetsToSpendWorksheet.Cells.AutoFitColumns();
            treatmentCurrentBudgetsToSpendWorksheet.Column(3).SetTrueWidth(65);
        }

        private void AddDynamicData(ExcelWorksheet treatmentCashflowConsiderationsWorksheet, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years, bool isPrimaryKeyNumeric, string primaryKey)
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

                    foreach (var treatmentConsideration in asset.TreatmentConsiderations)
                    {
                        foreach (var currentBudgetsToSpend in treatmentConsideration.FundingCalculationInput.CurrentBudgetsToSpend)
                        {
                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = primaryKeyValue;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = treatmentConsideration.TreatmentName;
                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Style.WrapText = true;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = currentBudgetsToSpend.Name;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = currentBudgetsToSpend.Amount;
                            ExcelHelper.SetCurrencyFormat(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn], ExcelFormatStrings.Currency);
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            dataRow++;
                            dataColumn = startColumn;
                        }
                    }
                }
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet treatmentCurrentBudgetsToSpendWorksheet, string primaryKey)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
            ExcelHelper.ApplyStyleWithBorder(treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
            ExcelHelper.ApplyStyleWithBorder(treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn].Value = "Name";
            ExcelHelper.ApplyStyleWithBorder(treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn].Value = "Amount";
            ExcelHelper.ApplyStyleWithBorder(treatmentCurrentBudgetsToSpendWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }

        private double CheckGetValue(Dictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(Dictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
