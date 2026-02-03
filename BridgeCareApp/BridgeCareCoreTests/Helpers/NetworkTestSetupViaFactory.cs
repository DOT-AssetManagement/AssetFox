using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using AssetFoxCore.Utils;
using DataAttribute = AssetFox.Core.Data.Attributes.Attribute;
using TNetwork = AssetFox.Core.Data.Networking.Network;


namespace AssetFoxCoreTests.Helpers
{
    public static class NetworkTestSetupViaFactory
    {

        public static TNetwork ModelViaFactory(IUnitOfWork unitOfWork, DataAttribute attribute, NetworkCreationParameters parameters, string networkName, ExcelRawDataDTO excelRawDataDto)
        {
            var allDataSource = parameters.NetworkDefinitionAttribute.DataSource;
            var mappedDataSource = AllDataSourceMapper.ToSpecificDto(allDataSource);
            var attributeConnection = AttributeConnectionBuilder.Build(attribute, mappedDataSource, unitOfWork, excelRawDataDto);
            var data = AttributeDataBuilder.GetData(attributeConnection);
            var network = NetworkFactory.CreateNetworkFromAttributeDataRecords(
                  data, parameters.DefaultEquation);
            network.Name = networkName;
            return network;
        }
    }
}
