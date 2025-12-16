using System.Collections.Generic;

namespace AssetFox.Core.Analysis.Input.DataTransfer;

public sealed class Network
{
    public AttributeSystem AttributeSystem { get; init; } = new();

    public List<MaintainableAsset> MaintainableAssets { get; init; } = new();

    public string Name { get; set; }
}
