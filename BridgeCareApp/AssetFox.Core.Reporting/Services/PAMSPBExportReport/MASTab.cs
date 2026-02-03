using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DTOs;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models;
using AssetFox.Core.Reporting.Models.PAMSPBExport;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.PAMSPBExport
{
    public class MASTab
    {
        public void Fill(ExcelWorksheet masWorksheet, Guid networkId, List<MaintainableAsset> networkMaintainableAssets, List<AggregatedResultDTO> aggregatedResultDTOs, List<AttributeDTO> attributeDTOs)
        {
            var currentCell = AddHeadersCells(masWorksheet);

            FillDynamicDataInWorkSheet(masWorksheet, currentCell, networkId, networkMaintainableAssets, aggregatedResultDTOs, attributeDTOs);

            masWorksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;
            masWorksheet.Cells.AutoFitColumns();
        }

        private void FillDynamicDataInWorkSheet(ExcelWorksheet masWorksheet, CurrentCell currentCell, Guid networkId, List<MaintainableAsset> networkMaintainableAssets, List<AggregatedResultDTO> aggregatedResultDatumDTOs, List<AttributeDTO> attributeDTOs)
        {
            foreach (var networkMaintainableAsset in networkMaintainableAssets)
            {
                var assetId = networkMaintainableAsset.Id;

                // Generate data model
                var aggregatedResultDTOsForAsset = aggregatedResultDatumDTOs.Where(_ => _.MaintainableAssetId == assetId).ToList();
                var masDataModel = GenerateMASDataModel(assetId, networkId, networkMaintainableAsset, aggregatedResultDTOsForAsset, attributeDTOs);

                // Fill in excel
                currentCell = FillDataInWorksheet(masWorksheet, masDataModel, currentCell);

            }
            ExcelHelper.ApplyBorder(masWorksheet.Cells[1, 1, currentCell.Row - 1, currentCell.Column]);
        }

        private static CurrentCell FillDataInWorksheet(ExcelWorksheet masWorksheet, MASDataModel masDataModel, CurrentCell currentCell)
        {
            var row = currentCell.Row;
            int column = 1;
                        
            masWorksheet.Cells[row, column++].Value = masDataModel.NetworkId;
            masWorksheet.Cells[row, column++].Value = masDataModel.MaintainableAssetId;
            masWorksheet.Cells[row, column++].Value = masDataModel.District;
            masWorksheet.Cells[row, column++].Value = masDataModel.Cnty;
            masWorksheet.Cells[row, column++].Value = masDataModel.Route;            
            masWorksheet.Cells[row, column++].Value = masDataModel.AssetName;
            masWorksheet.Cells[row, column++].Value = masDataModel.Direction;
            masWorksheet.Cells[row, column++].Value = masDataModel.FromSection;
            masWorksheet.Cells[row, column++].Value = masDataModel.ToSection;
            SetDecimalFormat(masWorksheet.Cells[row, column]);
            masWorksheet.Cells[row, column++].Value = masDataModel.Area;            
            masWorksheet.Cells[row, column++].Value = masDataModel.Interstate;
            masWorksheet.Cells[row, column++].Value = masDataModel.Lanes;
            SetDecimalFormat(masWorksheet.Cells[row, column]);
            masWorksheet.Cells[row, column++].Value = masDataModel.Width;
            SetDecimalFormat(masWorksheet.Cells[row, column]);
            masWorksheet.Cells[row, column++].Value = masDataModel.Length;            
            masWorksheet.Cells[row, column++].Value = masDataModel.surfaceName;
            SetDecimalFormat(masWorksheet.Cells[row, column]);
            masWorksheet.Cells[row, column++].Value = masDataModel.RiskScore;

            return new CurrentCell { Row = ++row, Column = column - 1 };
        }

        private static void SetDecimalFormat(ExcelRange cell) => ExcelHelper.SetCustomFormat(cell, ExcelHelperCellFormat.DecimalPrecision3);

        private MASDataModel GenerateMASDataModel(Guid assetId, Guid networkId, MaintainableAsset networkMaintainableAsset, List<AggregatedResultDTO> aggregatedResultDTOsForAsset, List<AttributeDTO> attributeDTOs)
        {
            MASDataModel masDataModel = new MASDataModel
            {
                NetworkId = networkId,
                MaintainableAssetId = assetId
            };

            var test = GetTextValue(aggregatedResultDTOsForAsset, attributeDTOs, "DISTRICT");

            var locationIdentifier = networkMaintainableAsset.Location?.LocationIdentifier;
            masDataModel.AssetName = locationIdentifier;
            var fromSection = string.Empty;
            var toSection = string.Empty;
            var direction = string.Empty;
            if (!string.IsNullOrEmpty(locationIdentifier))
            {
                var parts = locationIdentifier.Split(new char[] { '_' });
                direction = parts[2];
                var fromTo = parts.Last()?.Split('-');
                fromSection = fromTo?.First();
                toSection = fromTo?.Last();
            }
            masDataModel.FromSection = fromSection;
            masDataModel.ToSection = toSection;                        
                        
            double pavementLength = GetNumericValue(aggregatedResultDTOsForAsset, attributeDTOs, "SEGMENT_LENGTH");
            double pavementWidth = GetNumericValue(aggregatedResultDTOsForAsset, attributeDTOs, "WIDTH");
            masDataModel.Length = pavementLength;
            masDataModel.Width = pavementWidth;
            masDataModel.Area = pavementLength * pavementWidth;
            masDataModel.District = GetTextValue(aggregatedResultDTOsForAsset, attributeDTOs, "DISTRICT");
            masDataModel.Cnty = GetTextValue(aggregatedResultDTOsForAsset, attributeDTOs, "CNTY");
            masDataModel.Route = GetTextValue(aggregatedResultDTOsForAsset, attributeDTOs, "SR");
            masDataModel.Direction = direction; // GetTextValue(aggregatedResultDTOsForAsset, attributeDTOs, "DIRECTION");            
            masDataModel.Interstate = GetTextValue(aggregatedResultDTOsForAsset, attributeDTOs, "INTERSTATE");
            masDataModel.Lanes = GetNumericValue(aggregatedResultDTOsForAsset, attributeDTOs, "LANES");
            masDataModel.surfaceName = GetTextValue(aggregatedResultDTOsForAsset, attributeDTOs, "SURFACE_NAME");
            masDataModel.RiskScore = GetNumericValue(aggregatedResultDTOsForAsset, attributeDTOs, "RISKSCORE");

            return masDataModel;
        }

        private double GetNumericValue(List<AggregatedResultDTO> aggregatedResultDTOsForAsset, List<AttributeDTO> attributeDTOs, string attribute)
        {
            var aggregatedResultDTOForAsset = aggregatedResultDTOsForAsset.FirstOrDefault(_ => _.Attribute.Name == attribute);             
            return aggregatedResultDTOForAsset != null ? (double)aggregatedResultDTOForAsset.NumericValue : Convert.ToDouble(attributeDTOs.FirstOrDefault(_ => _.Name == attribute).DefaultValue);
        }

        private string GetTextValue(List<AggregatedResultDTO> aggregatedResultDTOsForAsset, List<AttributeDTO> attributeDTOs, string attribute)
        {
            var aggregatedResultDTOForAsset = aggregatedResultDTOsForAsset.FirstOrDefault(_ => _.Attribute.Name == attribute);            
            return aggregatedResultDTOForAsset != null ? aggregatedResultDTOForAsset.TextValue : attributeDTOs.FirstOrDefault(_ => _.Name == attribute).DefaultValue;
        }

        private static CurrentCell AddHeadersCells(ExcelWorksheet worksheet)
        {
            int headerRow = 1;
            int column = 1;

            worksheet.Cells.Style.WrapText = false;
            worksheet.Cells[headerRow, column++].Value = "NetworkID";
            worksheet.Cells[headerRow, column++].Value = "AssetId";
            worksheet.Cells[headerRow, column++].Value = "District";
            worksheet.Cells[headerRow, column++].Value = "Cnty";
            worksheet.Cells[headerRow, column++].Value = "Route";            
            worksheet.Cells[headerRow, column++].Value = "Asset";
            worksheet.Cells[headerRow, column++].Value = "Direction";
            worksheet.Cells[headerRow, column++].Value = "FromSection";
            worksheet.Cells[headerRow, column++].Value = "ToSection";
            worksheet.Cells[headerRow, column++].Value = "Area";
            worksheet.Cells[headerRow, column++].Value = "Interstate";
            worksheet.Cells[headerRow, column++].Value = "Lanes";
            worksheet.Cells[headerRow, column++].Value = "Width";
            worksheet.Cells[headerRow, column++].Value = "Length";            
            worksheet.Cells[headerRow, column++].Value = "SurfaceName";
            worksheet.Cells[headerRow, column++].Value = "Risk";

            return new CurrentCell { Row = ++headerRow, Column = column };
        }
    }
}
