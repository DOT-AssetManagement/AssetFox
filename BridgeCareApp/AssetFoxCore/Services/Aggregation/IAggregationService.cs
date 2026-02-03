using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFoxCore.Services.Aggregation
{
    public interface IAggregationService
    {
        /// <summary>Returns true if the aggregation succeeded</summary> 
        Task<bool> AggregateNetworkData(
            ChannelWriter<AggregationStatusMemo> writer,
            Guid networkId,
            AggregationState state,
            Guid dataSourceId, CancellationToken? cancellationToken = null);
    }
}
