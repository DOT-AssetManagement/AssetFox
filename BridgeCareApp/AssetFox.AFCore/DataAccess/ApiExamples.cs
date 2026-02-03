using System;
using System.Collections.Generic;
using System.IO;
using AssetFox.Core.DataMiner;
using AssetFox.Core.DataMiner.Attributes;
using Newtonsoft.Json;
using NetworkFromSegmentProject = AssetFox.Core.DataAssignment.Networking;
using DataMinerAttribute = AssetFox.Core.DataMiner.Attributes.Attribute;
using AssetFox.Core.DataAssignment.Aggregation;

namespace AssetFox.AFCore.DataAccess
{
    public sealed class ApiExamples
    {
        public NetworkFromSegmentProject.Network CreateNetwork()
        {
            var segmentationRulesJsonText = File.ReadAllText("segmentationMetaData.json");
            var attributeMetaDatum = JsonConvert.DeserializeAnonymousType(segmentationRulesJsonText,
                new { AttributeMetaDatum = default(AttributeMetaDatum) }).AttributeMetaDatum;

            var attribute = AttributeFactory.Create(attributeMetaDatum);
            var attributeData = AttributeDataBuilder.GetData(AttributeConnectionBuilder.Build(attribute));

            return NetworkFromSegmentProject.NetworkFactory.CreateNetworkFromAttributeDataRecords(attributeData);
        }

        public void AssignDataToSegments(Guid networkGuid)
        {
            var network = GetNetwork(networkGuid);

            var attributeJsonText = File.ReadAllText("attributeMetaData.json");
            var attributeMetaData = JsonConvert.DeserializeAnonymousType(attributeJsonText, new { AttributeMetaData = default(List<AttributeMetaDatum>) }).AttributeMetaData;

            var attributeData = new List<IAttributeDatum>();
            var attributes = new List<DataMinerAttribute>();

            // Create the list of attributes
            foreach (var attributeMetaDatum in attributeMetaData)
            {
                var attribute = AttributeFactory.Create(attributeMetaDatum);
                attributes.Add(attribute);
            }

            // Create the attribute data for each attribute
            foreach (var attribute in attributes)
            {
                attributeData.AddRange(AttributeDataBuilder.GetData(AttributeConnectionBuilder.Build(attribute)));
            }

            Aggregator.AssignAttributeDataToMaintainableAsset(attributeData, network.MaintainableAssets);
        }

        public void AggregateNetwork(Guid networkGuid)
        {
            var network = GetNetwork(networkGuid);

            var attributeJsonText = File.ReadAllText("attributeMetaData.json");
            var attributeMetaData = JsonConvert.DeserializeAnonymousType(attributeJsonText, new { AttributeMetaData = default(List<AttributeMetaDatum>) }).AttributeMetaData;

            var attributeData = new List<IAttributeDatum>();
            var attributes = new List<DataMinerAttribute>();

            // Create the list of attributes
            foreach (var attributeMetaDatum in attributeMetaData)
            {
                var attribute = AttributeFactory.Create(attributeMetaDatum);
                attributes.Add(attribute);
            }

            // Create the attribute data for each attribute
            foreach (var attribute in attributes)
            {
                attributeData.AddRange(AttributeDataBuilder.GetData(AttributeConnectionBuilder.Build(attribute)));
            }

            // Assign the attribute data to segments
            Aggregator.AssignAttributeDataToMaintainableAsset(attributeData, network.MaintainableAssets);

            //var aggregatedNumericResults = new DataMinerAttribute attribute (IEnumerable<(int year, double value);
            var aggregatedTextResults = new List<(DataMinerAttribute attribute, (int year, string value))>();
            foreach (var attribute in attributes)
            {
                foreach (var segment in network.MaintainableAssets)
                {
                    switch (attribute.DataType)
                    {
                    case "NUMERIC":

                        break;
                    case "TEXT":
                        //aggregatedTextResults.AddRange(segment.GetAggregatedValuesByYear(attribute, AggregationRuleFactory.CreateTextRule(attribute)));
                        break;
                    }


                }
            }

            // Save results to database, use them in the analysis.
        }

        public NetworkFromSegmentProject.Network GetNetwork(Guid networkGuid)
        {
            throw new NotImplementedException();
        }
    }
}
