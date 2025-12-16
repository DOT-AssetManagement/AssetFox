using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Data;

namespace AssetFox.Core.DataUnitTests.Tests
{
    public static class Locations
    {
        public static SectionLocation Section(string locationIdentifier, Guid? id = null)
        {
            var resolvedId = id ?? Guid.NewGuid();
            var returnValue = new SectionLocation(resolvedId, locationIdentifier);
            return returnValue;
        }
    }
}
