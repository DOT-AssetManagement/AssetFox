using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.DataUnitTests.TestUtils;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using EFCore.BulkExtensions;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class SimulationOutputCreationContextTestSetup
    {
        public static SimulationOutputSetupContext SimpleContextWithObjectsInDatabase(UnitOfDataPersistenceWork unitOfWork, int numberOfYears = 1)
        {
            var assetPairs = AssetNameIdPairLists.Random(1);
            var numericAttributeName = RandomStrings.WithPrefix("NumericAttribute");
            var textAttributeName = RandomStrings.WithPrefix("TextAttrbute");
            var numericAttributeNames = new List<string> { numericAttributeName };
            var textAttributeNames = new List<string> { textAttributeName };
            var context = ContextWithObjectsInDatabase(unitOfWork, assetPairs, numericAttributeNames, textAttributeNames, numberOfYears);
            return context;
        }

        public static SimulationOutputSetupContext ContextWithObjectsInDatabase(UnitOfDataPersistenceWork unitOfWork, List<AssetNameIdPair> assetNameIdPairs, List<string> numericAttributeNames, List<string> textAttributeNames, int numberOfYears = 1)
        {
            var networkId = Guid.NewGuid();
            var maintainableAssets = new List<MaintainableAsset>();
            foreach (var assetPair in assetNameIdPairs)
            {
                var locationIdentifier = RandomStrings.WithPrefix("Location");
                var location = Locations.Section(locationIdentifier);
                var maintainableAsset = MaintainableAssets.InNetwork(networkId, CommonTestParameterValues.DefaultEquation, assetPair.Id);
                maintainableAssets.Add(maintainableAsset);
            }
            var network = NetworkTestSetup.ModelForEntityInDb(unitOfWork, maintainableAssets, networkId);
            // WjJake -- this manual setting of the asset name has to be wrong, but it is the only way I could see to get the Name into our asset.
            var assetIds = assetNameIdPairs.Select(pair => pair.Id).ToList();
            var assetEntities2 = new List<MaintainableAssetEntity>();
            foreach (var assetId in assetIds)
            {
                var assetToLoad = new MaintainableAssetEntity
                {
                    Id = assetId,
                };
                assetEntities2.Add(assetToLoad);
            }
            var assetReadBulkConfig = new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(MaintainableAssetEntity.Id)}
            };
            unitOfWork.Context.BulkRead(assetEntities2, assetReadBulkConfig);
            var assetDictionary = new Dictionary<Guid, string>();
            foreach (var assetPair in assetNameIdPairs)
            {
                assetDictionary[assetPair.Id] = assetPair.Name;
            }
            foreach (var entity in assetEntities2)
            {
                entity.AssetName = assetDictionary[entity.Id];
            }
            unitOfWork.Context.UpdateRange(assetEntities2);
            unitOfWork.Context.SaveChanges();
            var simulation = SimulationTestSetup.ModelForEntityInDb(unitOfWork, networkId: networkId);
            var attributes = new List<Attribute>();
            foreach (var numericAttributeName in numericAttributeNames)
            {
                var numericAttributeId = Guid.NewGuid();
                var numericAttribute = AttributeTestSetup.Numeric(numericAttributeId, numericAttributeName);
                attributes.Add(numericAttribute);
            }
            foreach (var textAttributeName in textAttributeNames)
            {
                var textAttributeId = Guid.NewGuid();
                var textAttribute = AttributeTestSetup.Text(textAttributeId, textAttributeName);
                attributes.Add(textAttribute);
            }
            unitOfWork.AttributeRepo.UpsertAttributesNonAtomic(attributes);
            var years = Enumerable.Range(2022, numberOfYears).ToList();
            var context = new SimulationOutputSetupContext
            {
                BudgetName = "Budget",
                AssetNameIdPairs = assetNameIdPairs,
                TreatmentName = "Treatment",
                Years = years,
                NumericAttributeNames = numericAttributeNames,
                TextAttributeNames = textAttributeNames,
                SimulationId = simulation.Id,
            };
            return context;
        }
    }
}
