using System;
using System.Linq;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.ExcelDatabaseStorage.Serializers;
using AssetFox.Core.DataPersistenceCore.Migrations;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.Attributes
{
    public class ExcelAttributeConnectionTests
    {
        [Fact]
        public void GetData_StringAttributeInDatabase_Gets()
        {
            // This test really belongs in DataUnitTests. The problem is
            // that making that change would require resetting the database for
            // a run of DataUnitTests. That takes about 15 seconds.
            // Arrange
            var unitOfWork = TestHelper.UnitOfWork;
            var dataSource = DataSourceTestSetup.DtoForExcelDataSourceInDb(unitOfWork);
            var attribute = AttributeConnectionAttributes.ForExcelTestData(dataSource.Id);
            var importedSpreadsheet = ExcelRawDataSetup.RawData(dataSource.Id);
            var deserializationResult = ExcelRawDataSpreadsheetSerializer.Deserialize(importedSpreadsheet.SerializedWorksheetContent);
            var rawDataSpreadsheet = deserializationResult.Worksheet;

            unitOfWork.AttributeRepo.UpsertAttributes(attribute);
            var dataSourceMap = new DataSourceMappingDTO
            {
                Id = Guid.NewGuid(),
                DataField = attribute.Command,
                DataSourceId = dataSource.Id,
                AttributeId = attribute.Id
            };
            //var dataSourceMaps = unitOfWork.DataSourceMappingRepo.GetDataSourceMappings(dataSource.Id);
            //var dataSourceMap = dataSourceMaps.First(m => m.AttributeId == attribute.Id);

            var excelAttributeConnection = new ExcelAttributeConnection(attribute, dataSourceMap, dataSource, rawDataSpreadsheet);

            // Act
            var result = excelAttributeConnection.GetData<string>();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(4, resultList.Count);
            Assert.IsType<AttributeDatum<string>>(resultList[0]);
        }
    }
}
