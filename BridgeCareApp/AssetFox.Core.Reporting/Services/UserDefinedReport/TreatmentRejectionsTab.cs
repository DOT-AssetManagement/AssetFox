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
    internal class TreatmentRejectionsTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public TreatmentRejectionsTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet treatmentRejectionsWorksheet, bool isPrimaryKeyNumeric, string primaryKey, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years)
        {
            //set default width
            treatmentRejectionsWorksheet.DefaultColWidth = 18;
              
            // Headers
            var currentCell = AddHeaders(treatmentRejectionsWorksheet, primaryKey);

            // Add row next to headers for filters
            using (var autoFilterCells = treatmentRejectionsWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(treatmentRejectionsWorksheet, initialAssetSummaries, years, isPrimaryKeyNumeric, primaryKey);

            treatmentRejectionsWorksheet.Cells.AutoFitColumns();
            treatmentRejectionsWorksheet.Column(3).SetTrueWidth(65);
        }

        private void AddDynamicData(ExcelWorksheet treatmentOptionsWorksheet, List<AssetSummaryDetail> initialAssetSummaries, List<SimulationYearDetail> years, bool isPrimaryKeyNumeric, string primaryKey)
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

                    foreach (var treatmentRejection in asset.TreatmentRejections)
                    {
                        treatmentOptionsWorksheet.Cells[dataRow, dataColumn].Value = primaryKeyValue;
                        ExcelHelper.ApplyBorder(treatmentOptionsWorksheet.Cells[dataRow, dataColumn++]);

                        treatmentOptionsWorksheet.Cells[dataRow, dataColumn].Value = year.Year;
                        ExcelHelper.ApplyBorder(treatmentOptionsWorksheet.Cells[dataRow, dataColumn++]);

                        treatmentOptionsWorksheet.Cells[dataRow, dataColumn].Value = treatmentRejection.TreatmentName;
                        treatmentOptionsWorksheet.Cells[dataRow, dataColumn].Style.WrapText = true;
                        ExcelHelper.ApplyBorder(treatmentOptionsWorksheet.Cells[dataRow, dataColumn++]);

                        treatmentOptionsWorksheet.Cells[dataRow, dataColumn].Value = treatmentRejection.TreatmentRejectionReason.ToString();
                        ExcelHelper.ApplyBorder(treatmentOptionsWorksheet.Cells[dataRow, dataColumn++]);

                        treatmentOptionsWorksheet.Cells[dataRow, dataColumn].Value = treatmentRejection.PotentialConditionChange;
                        ExcelHelper.ApplyBorder(treatmentOptionsWorksheet.Cells[dataRow, dataColumn++]);

                        dataRow++;
                        dataColumn = startColumn;
                    }
                }
            }
        }

        private static CurrentCell AddHeaders(ExcelWorksheet treatmentSchedulingCollisionsWorksheet, string primaryKey)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;

            treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
            ExcelHelper.ApplyStyleWithBorder(treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn].Value = "Year";
            ExcelHelper.ApplyStyleWithBorder(treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentName";
            ExcelHelper.ApplyStyleWithBorder(treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn].Value = "TreatmentRejectionReason";
            ExcelHelper.ApplyStyleWithBorder(treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn++]);

            treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn].Value = "PotentialConditionChange";
            ExcelHelper.ApplyStyleWithBorder(treatmentSchedulingCollisionsWorksheet.Cells[currentRow, currentColumn++]);

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }

        private double CheckGetValue(IDictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(IDictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
