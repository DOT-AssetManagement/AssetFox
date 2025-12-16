using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssetFox.Core.Data.Aggregation;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.Helpers;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;

namespace AssetFox.Core.Data.Networking
{
    public class MaintainableAsset
    {
        public MaintainableAsset(Guid id, Guid networkId, Location location, string spatialWeighting)
        {
            Id = id;
            NetworkId = networkId;
            Location = location;
            SpatialWeighting = spatialWeighting;
        }

        public AggregatedResult<T> GetAggregatedValuesByYear<T>(Attribute attribute, AggregationRule<T> aggregationRule)
        {
            var specifiedData = AssignedData.Where(_ => _.Attribute.Id == attribute.Id);
            return new AggregatedResult<T>(Guid.NewGuid(), this, aggregationRule.Apply(specifiedData, attribute).ToList());
        }

        public void AssignAttributeData(IEnumerable<IAttributeDatum> attributeData)
        {
            //List<DatumLog> datumLog = new List<DatumLog>();
            foreach (var datum in attributeData)
            {
                if (datum.Location.MatchOn(Location))
                {
                    AssignedData.Add(datum);
                }
                //else
                //{
                //    // return the unmatched datum to be logged and reported
                //        var currentDatumLog = new DatumLog(datum.Attribute.Id, Location.Id, datum.Attribute.Name);
                //        if (datumLog.Find(x => (x.Equals(currentDatumLog))) == null)
                //            datumLog.Add(currentDatumLog);
                //}
            }

            //return datumLog;
        }

        public void AssignAttributeDataFromDataSource(IEnumerable<IAttributeDatum> attributeData) => AssignedData.AddRange(attributeData);

        public List<IAttributeDatum> AssignedData { get; } = new List<IAttributeDatum>();

        public Guid Id { get; }

        public Guid NetworkId { get; }

        public string SpatialWeighting { get; }

        public Location Location { get; }
    }
}
