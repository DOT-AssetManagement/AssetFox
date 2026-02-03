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
            yearWorksheet.Column(6).SetTrueWidth(23.50);
            yearWorksheet.Column(7).SetTrueWidth(16.50);
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

                        yearWorksheet.Cells[dataRow, dataColumn].Value = GetAttributeValue(asset, assetSummary, attribute);
                        ExcelHelper.ApplyBorder(yearWorksheet.Cells[dataRow, dataColumn++]);
                    }

                    dataRow++;
                    dataColumn = startColumn;
                }
            }
        }

        private object GetAttributeValue(AssetDetail asset, AssetSummaryDetail assetSummary, string attribute) =>            
            asset.ValuePerNumericAttribute.ContainsKey(attribute) || asset.ValuePerTextAttribute.ContainsKey(attribute)
                ? asset.ValuePerNumericAttribute.Any(_ => _.Key == attribute)
                    ? CheckGetValue(asset.ValuePerNumericAttribute, attribute)
                    : CheckGetTextValue(asset.ValuePerTextAttribute, attribute)
                : assetSummary.ValuePerNumericAttribute.Any(_ => _.Key == attribute)
                    ? CheckGetValue(assetSummary.ValuePerNumericAttribute, attribute)
                    : CheckGetTextValue(assetSummary.ValuePerTextAttribute, attribute);

        private double CheckGetValue(IDictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(IDictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
