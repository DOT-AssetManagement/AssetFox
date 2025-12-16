using System.Threading.Channels;
using AssetFox.Core.Common;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Aggregation;
using AssetFox.Core.Data.Mappers;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Extensions;
using AssetFox.Core.UnitTestsCore.SampleData;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using AssetFox.Core.UnitTestsCore.Tests.DataSources;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Models;
using AssetFoxCore.Services.Aggregation;
using AssetFoxCoreTests.Helpers;
using AssetFoxCoreTests.Tests.AttributeDatum;
using Moq;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class ExcelAggregationTests
    {
        private static string SampleAttributeDataPath()
        {
            var filename = SampleAttributeDataFilenames.Sample;
            var folder = Directory.GetCurrentDirectory();
            var returnValue = Path.Combine(folder, SampleAttributeDataFolderNames.SampleData, filename);
            return returnValue;
        }

        [Fact]
        public async Task Aggregate_ExcelDataSourceReturnedFromRepo_AttributesReturnedFromRepo_Aggregates()
        {
            var mockUnitOfWork = UnitOfWorkMocks.New();
            var networkRepo = NetworkRepositoryMocks.New(mockUnitOfWork);
            var maintainableAssetRepo = MaintainableAssetRepositoryMocks.New(mockUnitOfWork);
            var dataSourceRepo = DataSourceRepositoryMocks.New(mockUnitOfWork);
            var excelWorksheetRepo = ExcelRawDataRepositoryMocks.New(mockUnitOfWork);
            var attributeDatumRepo = AttributeDatumRepositoryMocks.New(mockUnitOfWork);
            var aggregatedResultRepo = AggregatedResultRepositoryMocks.New(mockUnitOfWork);
            var unitOfWork = mockUnitOfWork.Object;
            var spreadsheetService = TestServices.CreateExcelSpreadsheetImportService(unitOfWork);
            var dataSourceDto = ExcelDataSourceDtos.WithColumnNames("Inspection_Date", "BRKEY");
            dataSourceRepo.Setup(d => d.GetDataSource(dataSourceDto.Id)).Returns(dataSourceDto);
            excelWorksheetRepo.Setup(e => e.AddExcelRawData(It.IsAny<ExcelRawDataDTO>())).Returns((ExcelRawDataDTO d) => d.Id);
            var excelRawDataId = Guid.NewGuid();
            var excelRawDataDto = ExcelRawDataDtos.Dto(excelRawDataId);
            excelWorksheetRepo.Setup(e => e.GetExcelRawDataByDataSourceId(dataSourceDto.Id)).Returns(excelRawDataDto);
            var districtAttribute = AttributeDtos.District(dataSourceDto);
            var districtAttributeDomain = AttributeDtoDomainMapper.ToDomain(districtAttribute, unitOfWork.EncryptionKey);
            var path = SampleAttributeDataPath();
            var stream = File.OpenRead(path);
            var excelPackage = new ExcelPackage(stream);
            var importResult = spreadsheetService.ImportRawData(dataSourceDto.Id, excelPackage.Workbook.Worksheets[0]);

            var networkName = RandomStrings.WithPrefix("Network");
            var attribute = AttributeTestSetup.NumericDto(dataSourceDto, connectionType: ConnectionType.EXCEL);
            var allDataSourceDto = AllDataSourceDtoFakeFrontEndFactory.ToAll(dataSourceDto);

            var networkDefinitionAttribute = AllAttributeDtos.BrKey(allDataSourceDto);
            var parameters = new NetworkCreationParameters
            {
                DefaultEquation = "[Deck_Area]",
                NetworkDefinitionAttribute = networkDefinitionAttribute
            };
            var network = NetworkTestSetupViaFactory.ModelViaFactory(
                mockUnitOfWork.Object, districtAttributeDomain, parameters, networkName, excelRawDataDto);
            var networkId = network.Id;
            var assetName = "100";
            var location = new SectionLocation(Guid.NewGuid(), assetName);
            var maintainableAssetId = Guid.NewGuid();
            var spatialWeightingValue = "[Deck_Area]";
            var newAsset = new MaintainableAsset(maintainableAssetId, networkId, location, spatialWeightingValue);
            var assetList = new List<MaintainableAsset> { newAsset };
            maintainableAssetRepo.Setup(a => a.GetAllInNetworkWithAssignedDataAndLocations(networkId)).Returns(assetList);
            var doNotLog = new DoNotLog();
            var aggregationService = new AggregationService(unitOfWork, doNotLog);
            var channel = Channel.CreateUnbounded<AggregationStatusMemo>();
            var aggregationState = new AggregationState();
            var dataSourceId = dataSourceDto.Id;

            var aggregationResult = await aggregationService.AggregateNetworkData(channel.Writer, networkId, aggregationState, dataSourceId);

            Assert.True(aggregationResult);
            var addCall = aggregatedResultRepo.SingleInvocationWithName(nameof(IAggregatedResultRepository.AddAggregatedResults));
            var results = addCall.Arguments[0] as List<IAggregatedResult>;
            var theResult = results.Single();
            var theStringResult = theResult as AggregatedResult<string>;
            var aggregatedDatum = theStringResult.AggregatedData.Single();
            Assert.Equal(districtAttribute.Id, aggregatedDatum.attribute.Id);
            Assert.Equal(2022, aggregatedDatum.yearValuePair.year);
            Assert.Equal("110", aggregatedDatum.yearValuePair.value);
        }
    }
}
