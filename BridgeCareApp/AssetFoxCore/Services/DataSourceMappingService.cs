using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using OfficeOpenXml;

namespace AssetFoxCore.Services
{
    public class DataSourceMappingService : IDataSourceMappingService
    {
        private static IUnitOfWork _unitOfWork;

        public DataSourceMappingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public FileInfoDTO DownloadDataSourceMappings(Guid dataSourceId)
        {
            var mappingsDtos = _unitOfWork.DataSourceMappingRepo.GetDataSourceMappings(dataSourceId);
            var fileInfoDto = ExportDataSourceMappingsFile(dataSourceId, mappingsDtos);

            return fileInfoDto;
        }

        public FileInfoDTO ExportDataSourceMappingsFile(Guid dataSourceId, List<DataSourceMappingDTO> mappingsDtos)
        {
            var dataSource = _unitOfWork.DataSourceRepo.GetDataSource(dataSourceId);
            var dataSourceName = dataSource.Name.Replace(" ", "_").Trim();            

            var fileName = $"DataSourceMappings_{dataSourceName}.xlsx";

            using var excelPackage = new ExcelPackage();
            var worksheet = excelPackage.Workbook.Worksheets.Add("Mappings");

            var columns = new string[] { "Attribute", "Column" };
            AddHeaderCells(worksheet, columns);

            if (mappingsDtos.Any())
            {
                AddDataCells(worksheet, mappingsDtos, columns);
            }

            worksheet.Cells.AutoFitColumns();

            return new FileInfoDTO
            {
                FileName = fileName,
                FileData = Convert.ToBase64String(excelPackage.GetAsByteArray()),
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        private static void AddDataCells(ExcelWorksheet worksheet, List<DataSourceMappingDTO> mappingsDtos, string[] columns)
        {
            int row = 2;
            int col = 1;
            foreach (var mappingDto in mappingsDtos)
            {
                worksheet.Cells[row, col].Value = mappingDto.AttributeName;
                worksheet.Cells[row, col + 1].Value = mappingDto.DataField;

                row++;
            }
        }

        private static void AddHeaderCells(ExcelWorksheet worksheet, string[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = columns[i];
            }
        }
    }
}
