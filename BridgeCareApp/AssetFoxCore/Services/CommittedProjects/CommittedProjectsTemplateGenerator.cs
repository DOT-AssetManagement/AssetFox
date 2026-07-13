using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;
using OfficeOpenXml;

namespace AssetFoxCore.Services.CommittedProjects
{
    public class CommittedProjectsTemplateGenerator
    {
        private readonly string _networkKeyField;
        private List<string> _keyFields;

        public CommittedProjectsTemplateGenerator(string networkKeyField, System.Collections.Generic.List<string> keyFields)
        {
            _networkKeyField = networkKeyField ?? throw new ArgumentNullException(nameof(networkKeyField));
            _keyFields = keyFields ?? throw new ArgumentNullException(nameof(keyFields));
        }


        public FileInfoDTO CreateCommittedProjectTemplate()
        {
            var fileName = $"CommittedProjectsTemplate.xlsx";
            using var excelPackage = new ExcelPackage();
            var worksheet = excelPackage.Workbook.Worksheets.Add("Committed Projects");            
            AddHeaderCells(worksheet);
            worksheet.Cells.AutoFitColumns();

            return new FileInfoDTO
            {
                FileName = fileName,
                FileData = Convert.ToBase64String(excelPackage.GetAsByteArray()),
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }
        
        private void AddHeaderCells(ExcelWorksheet worksheet)
        {
            var column = 1;            
            var columnHeaders = new List<string>();
            columnHeaders.AddRange(_keyFields);
            columnHeaders.AddRange(CommittedProjectsColumnHeaders.NoKeyHeaders);

            // Add the headers to the worksheet
            foreach (var header in columnHeaders)
            {
                worksheet.Cells[1, column++].Value = header;
            }
        }
    }
}
