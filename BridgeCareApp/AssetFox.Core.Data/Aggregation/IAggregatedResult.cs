using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.Data.Networking;

namespace AssetFox.Core.Data.Aggregation
{
    public interface IAggregatedResult
    {
        Guid Id { get; }
        MaintainableAsset MaintainableAsset { get; }
    }
}
