using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.UserDefinedReport
{
    internal class TreatmentCashflowConsiderationsTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public TreatmentCashflowConsiderationsTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet treatmentCashflowConsiderationsWorksheet, bool isPrimaryKeyNumeric, string primaryKey, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years)
        {
            //set default width
            treatmentCashflowConsiderationsWorksheet.DefaultColWidth = 18;

            // Headers
            var currentCell = AddHeaders(treatmentCashflowConsiderationsWorksheet, primaryKey);

            // Add row next to headers for filters
            using (var autoFilterCells = treatmentCashflowConsiderationsWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(treatmentCashflowConsiderationsWorksheet, initialAssetSummaries, years, isPrimaryKeyNumeric, primaryKey);

            treatmentCashflowConsiderationsWorksheet.Cells.AutoFitColumns();
            treatmentCashflowConsiderationsWorksheet.Column(3).SetTrueWidth(65);
            treatmentCashflowConsiderationsWorksheet.Column(4).SetTrueWidth(18);
            treatmentCashflowConsiderationsWorksheet.Column(5).SetTrueWidth(22);
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
                        foreach (var cashFlowConsideration in treatmentConsideration.CashFlowConsiderations)
                        {
                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = primaryKeyValue;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = treatmentConsideration.TreatmentName;
                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Style.WrapText = true;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = cashFlowConsideration.CashFlowRuleName;
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn].Value = cashFlowConsideration.ReasonAgainstCashFlow.ToString();
                            ExcelHelper.ApplyBorder(treatmentCashflowConsiderationsWorksheet.Cells[dataRow, dataColumn++]);

                            dataRow++;
                            dataColumn = startColumn;
                        }
                    }
                }
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet treatmentCashflowConsiderationsWorksheet, string primaryKey)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
            ExcelHelper.ApplyStyleWithBorder(treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
            ExcelHelper.ApplyStyleWithBorder(treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn].Value = "CashFlowRuleName";
            ExcelHelper.ApplyStyleWithBorder(treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn].Value = "ReasonAgainstCashFlow";
            ExcelHelper.ApplyStyleWithBorder(treatmentCashflowConsiderationsWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }

        private double CheckGetValue(IDictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(IDictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
