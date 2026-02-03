using System.Collections.Generic;

namespace AssetFox.Core.Analysis.Input.DataTransfer;

public sealed class AttributeValueHistory<T>
{
    public string AttributeName { get; set; }

    public List<HistoricalValue<T>> History { get; init; } = new();
}
