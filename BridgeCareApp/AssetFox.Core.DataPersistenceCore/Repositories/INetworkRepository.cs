using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using Network = AssetFox.Core.Data.Networking.Network;
using System.Threading;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Common.Logging;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface INetworkRepository
    {
        void CreateNetwork(Network network);

        Task<List<NetworkDTO>> Networks();

        NetworkEntity GetMainNetwork();

        NetworkEntity GetRawNetwork();

        Analysis.Network GetSimulationAnalysisNetwork(Guid networkId, Explorer explorer, bool areFacilitiesRequired = true, Guid? simulationId = null);

        void DeleteNetwork(Guid networkId, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        void UpsertNetworkRollupDetail(Guid networkId, string status);

        string GetNetworkName(Guid networkId);

        string GetNetworkKeyAttribute(Guid networkId);
    }
}
