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
    internal class TreatmentAllocationsTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public TreatmentAllocationsTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet treatmentAllocationsWorksheet, bool isPrimaryKeyNumeric, string primaryKey, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years)
        {
            //set default width
            treatmentAllocationsWorksheet.DefaultColWidth = 18;

            // Headers
            var currentCell = AddHeaders(treatmentAllocationsWorksheet, primaryKey);

            // Add row next to headers for filters
            using (var autoFilterCells = treatmentAllocationsWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(treatmentAllocationsWorksheet, initialAssetSummaries, years, isPrimaryKeyNumeric, primaryKey);

            treatmentAllocationsWorksheet.Cells.AutoFitColumns();
            treatmentAllocationsWorksheet.Column(3).SetTrueWidth(65);
            treatmentAllocationsWorksheet.Column(6).SetTrueWidth(65);
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
                        foreach (var allocation in treatmentConsideration.FundingCalculationOutput.AllocationMatrix)
                        {
                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = primaryKeyValue;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = treatmentConsideration.TreatmentName;
                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Style.WrapText = true;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = allocation.Year;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = allocation.BudgetName;                            
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = allocation.TreatmentName;
                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Style.WrapText = true;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = allocation.AllocatedAmount;
                            ExcelHelper.SetCurrencyFormat(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn], ExcelFormatStrings.Currency);
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);                            

                            dataRow++;
                            dataColumn = startColumn;
                        }
                    }
                }
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet treatmentAllocationsWorksheet, string primaryKey)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            treatmentAllocationsWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
            ExcelHelper.ApplyStyleWithBorder(treatmentAllocationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentAllocationsWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(treatmentAllocationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentAllocationsWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
            ExcelHelper.ApplyStyleWithBorder(treatmentAllocationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentAllocationsWorksheet.Cells[currentRow, currentColumn].Value = "AllocationYear";
            ExcelHelper.ApplyStyleWithBorder(treatmentAllocationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentAllocationsWorksheet.Cells[currentRow, currentColumn].Value = "BudgetName";
            ExcelHelper.ApplyStyleWithBorder(treatmentAllocationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentAllocationsWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
            ExcelHelper.ApplyStyleWithBorder(treatmentAllocationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentAllocationsWorksheet.Cells[currentRow, currentColumn].Value = "AllocatedAmount";
            ExcelHelper.ApplyStyleWithBorder(treatmentAllocationsWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }

        private double CheckGetValue(Dictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(Dictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
