using Xunit;
using System;
using AssetFox.Core.Data.Aggregation;
using System.Collections.Generic;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.Data;
using AssetFox.Core.DataUnitTests.TestUtils;

namespace AssetFox.Core.DataUnitTests.Tests.Aggregation
{
    public class AggregatorTests
    {
        List<IAttributeDatum> attributeData;
        List<MaintainableAsset> maintainableAssets = new List<MaintainableAsset>();
        private readonly Guid guId = Guid.Empty;
        private readonly SectionLocation sectionLocation1;
        private readonly SectionLocation sectionLocation2;

        public AggregatorTests()
        {
            attributeData = new List<IAttributeDatum>();
            sectionLocation1 = new SectionLocation(guId, CommonTestParameterValues.LocationIdentifier1);
            sectionLocation2 = new SectionLocation(guId, CommonTestParameterValues.LocationIdentifier2);
        }
    }
}
