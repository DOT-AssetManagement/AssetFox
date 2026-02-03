using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using AssetFoxCoreTests.Helpers;
using DataAttribute = AssetFox.Core.Data.Attributes.Attribute;
using TNetwork = AssetFox.Core.Data.Networking.Network;

namespace AssetFoxCoreTests.Tests.Integration
{
    public static class NetworkIntegrationTestSetup
    {
        public static TNetwork ModelForEntityInDbViaFactory(IUnitOfWork unitOfWork, DataAttribute attribute, NetworkCreationParameters parameters, string networkName,
            ExcelRawDataDTO excelRawDataDto)
        {
            var network = NetworkTestSetupViaFactory.ModelViaFactory(unitOfWork, attribute, parameters, networkName, excelRawDataDto);

            // insert network domain data into the data source
            unitOfWork.NetworkRepo.CreateNetwork(network);
            return network;
        }
    }
}
