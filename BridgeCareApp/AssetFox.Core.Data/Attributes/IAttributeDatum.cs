using System;

namespace AssetFox.Core.Data.Attributes
{
    public interface IAttributeDatum
    {
        Location Location { get; }

        Attribute Attribute { get; }

        DateTime TimeStamp { get; }
    }
}
