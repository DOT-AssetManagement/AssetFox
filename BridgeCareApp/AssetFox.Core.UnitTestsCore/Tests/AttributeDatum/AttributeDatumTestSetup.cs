using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.Mappers;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class AttributeDatumTestSetup
    {
        private static Guid AssignAttributeDatum<T>(
            AttributeDTO attributeDto,
            MaintainableAsset asset,
            T datumValue,
            string locationName = "location"
            )
        {
            var assets = new List<MaintainableAsset> { asset };
            var datumId = Guid.NewGuid();
            var location = Locations.Section(locationName);
            var domainAttribute = AttributeDtoDomainMapper.ToDomain(attributeDto, "");
            var datum = new AttributeDatum<T>(datumId, domainAttribute, datumValue, location, DateTime.Now);
            var attributeDtos = new List<AttributeDTO> { attributeDto };
            asset.AssignedData.Add(datum);
            TestHelper.UnitOfWork.AttributeDatumRepo.AddAssignedData(assets, attributeDtos);
            return datumId;
        }

        public static Guid AssignStringAttributeDatum(
            AttributeDTO attributeDto,
            MaintainableAsset asset,
            string datumValue = "where",
            string locationName = "location"
            )
        {
            var guid = AssignAttributeDatum(
                attributeDto, asset, datumValue, locationName);
            return guid;
        }

        public static Guid AssignDoubleAttributeDatum(
            AttributeDTO attributeDto,
            MaintainableAsset asset,
            double datumValue,
            string locationName = "location"
            )
        {
            var guid = AssignAttributeDatum(
                attributeDto, asset, datumValue, locationName);
            return guid;
        }
    }
}
