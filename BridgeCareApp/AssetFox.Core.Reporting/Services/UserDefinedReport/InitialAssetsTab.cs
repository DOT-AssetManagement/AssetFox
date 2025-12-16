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
    internal class InitialAssetsTab
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportHelper _reportHelper;

        public InitialAssetsTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        internal void Fill(ExcelWorksheet assetSummariesWorksheet, List<string> filterAttributes, bool isPrimaryKeyNumeric, string primaryKey, List<AssetSummaryDetail> initialAssetSummaries)
        {
            //set default width
            assetSummariesWorksheet.DefaultColWidth = 18;

            // Headers
            var currentCell = AddHeaders(assetSummariesWorksheet, filterAttributes, primaryKey);

            // Add row next to headers for filters
            using (var autoFilterCells = assetSummariesWorksheet.Cells[2, 1, currentCell.Row, currentCell.Column])
            {
                autoFilterCells.AutoFilter = true;
            }

            // Data
            AddDynamicData(assetSummariesWorksheet, filterAttributes, initialAssetSummaries, isPrimaryKeyNumeric, primaryKey);

            assetSummariesWorksheet.Cells.AutoFitColumns();
        }

        private void AddDynamicData(ExcelWorksheet assetSummariesWorksheet, List<string> filterAttributes, List<AssetSummaryDetail> initialAssetSummaries, bool isPrimaryKeyNumeric, string primaryKey)
        {
            var dataRow = 3;
            var startColumn = 1;
            var dataColumn = startColumn;

            foreach (var assetSummary in initialAssetSummaries)
            {
                var primaryKeyValue = isPrimaryKeyNumeric
                ? CheckGetValue(assetSummary.ValuePerNumericAttribute, primaryKey).ToString()
                : CheckGetTextValue(assetSummary.ValuePerTextAttribute, primaryKey);
                assetSummariesWorksheet.Cells[dataRow, dataColumn].Value = primaryKeyValue;
                ExcelHelper.ApplyBorder(assetSummariesWorksheet.Cells[dataRow, dataColumn++]);

                foreach (var attribute in filterAttributes)
                {
                    if (attribute.Equals(primaryKey))
                    {
                        continue;
                    }
                    assetSummariesWorksheet.Cells[dataRow, dataColumn].Value = GetAttributeValue(assetSummary, attribute);
                    ExcelHelper.ApplyBorder(assetSummariesWorksheet.Cells[dataRow, dataColumn++]);                    
                }

                dataRow++;
                dataColumn = startColumn;
            }
        }        

        private static CurrentCell AddHeaders(ExcelWorksheet assetSummariesWorksheet, List<string> filterAttributes, string primaryKey)
        {
            var startColumn = 1;
            var startRow = 1;
            var currentRow = startRow;
            var currentColumn = startColumn;
                        
            assetSummariesWorksheet.Cells[currentRow, currentColumn].Value = primaryKey;
            ExcelHelper.ApplyStyleWithBorder(assetSummariesWorksheet.Cells[currentRow, currentColumn++]);

            foreach (var attribute in filterAttributes)
            {
                if (attribute.Equals(primaryKey))
                {
                    continue;
                }
                assetSummariesWorksheet.Cells[currentRow, currentColumn].Value = attribute;
                ExcelHelper.ApplyStyleWithBorder(assetSummariesWorksheet.Cells[currentRow, currentColumn++]);
            }

            return new CurrentCell { Row = ++startRow, Column = currentColumn - 1 };
        }

        private object GetAttributeValue(AssetSummaryDetail assetSummary, string attribute) =>
            assetSummary.ValuePerNumericAttribute.Any(_ => _.Key == attribute)
                ? CheckGetValue(assetSummary.ValuePerNumericAttribute, attribute)
                : CheckGetTextValue(assetSummary.ValuePerTextAttribute, attribute);

        private double CheckGetValue(IDictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(IDictionary<string, string> valuePerTextAttribute, string attribute) => _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);
    }
}
