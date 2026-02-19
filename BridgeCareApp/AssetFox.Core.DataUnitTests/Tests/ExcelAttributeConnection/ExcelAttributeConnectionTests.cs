using System;
using System.Linq;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.ExcelDatabaseStorage.Serializers;
using AssetFox.Core.DataPersistenceCore.Migrations;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs;
using Xunit;

namespace AssetFox.Core.DataUnitTests.Tests.Attributes
{
    public class ExcelAttributeConnectionTests
    {
        [Fact]
        public void GetData_StringAttributeInDatabase_Gets()
        {
            // If one is starting with this test, the Database needs to already
            // exist and have been reset by DatabaseResetter. NOT adding that call however
            // because it is slow.
            // Arrange
            var config = TestConfiguration.Get();
            var unitOfWork = UnitOfWorkSetup.New(config);
            var dataSource = DataSourceTestSetup.DtoForExcelDataSourceInDb(unitOfWork);
            var attribute = AttributeConnectionAttributes.ForExcelTestData(dataSource.Id);
            var importedSpreadsheet = ExcelRawDataSetup.RawData(dataSource.Id);
            var deserializationResult = ExcelRawDataSpreadsheetSerializer.Deserialize(importedSpreadsheet.SerializedWorksheetContent);
            var rawDataSpreadsheet = deserializationResult.Worksheet;

            unitOfWork.AttributeRepo.UpsertAttributes(attribute);
            var dataSourceMap = new DataSourceMappingDTO
            {
                Id = Guid.NewGuid(),
                DataField = attribute.Name,
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
