using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.DataUnitTests.Tests
{
    public static class ExcelDataSourceDtos
    {

        public static ExcelDataSourceDTO WithColumnNames(string dateColumnName, string locationColumnName)
        {
            var randomName = RandomStrings.WithPrefix("TestExcelDataSource");
            var excelDataSource = new ExcelDataSourceDTO
            {
                DateColumn = dateColumnName,
                LocationColumn = locationColumnName,
                Id = Guid.NewGuid(),
                Name = randomName,
            };
            return excelDataSource;
        }
    }
}
