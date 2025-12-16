using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Common;
using AssetFox.Core.Common.PerformanceMeasurement;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Newtonsoft.Json;
using Xunit;

namespace AssetFox.Core.StressTesting
{
    public class SimulationOutputRepositoryStressTests
    {
        private void SaveSimulationOutput_ThenLoad_Same(string filename, Func<SimulationOutput, SimulationOutput> preTransform = null)
        {
            if (preTransform == null)
            {
                preTransform = (SimulationOutput so) => so;
            }
            var text = FileReader.ReadAllTextInGitIgnoredFile(filename);
            var rawOutput = JsonConvert.DeserializeObject<SimulationOutput>(text);
            var yearCount = rawOutput.Years.Count;
            var simulationOutput = preTransform(rawOutput);
            var assetNameIdPairs = simulationOutput.InitialAssetSummaries.Select(a => AssetNameIdPairs.ForAssetSummaryDetail(a)).ToList();
            var assetSummary = simulationOutput.InitialAssetSummaries[0];
            var attributeNamesToIgnore = new List<string> { "AREA" };
            var numericAttributeNames = assetSummary.ValuePerNumericAttribute.Keys.Except(attributeNamesToIgnore).ToList();
            var textAttributeNames = assetSummary.ValuePerTextAttribute.Keys.ToList();
            var context = SimulationOutputCreationContextTestSetup.ContextWithObjectsInDatabase(TestHelper.UnitOfWork, assetNameIdPairs, numericAttributeNames, textAttributeNames, yearCount);
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaRelational(context.SimulationId, simulationOutput);
            var loadedOutput = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(context.SimulationId);
            SimulationOutputAssertions.SameSimulationOutput(loadedOutput, simulationOutput, false);
        }

        //[Fact]
        [Fact(Skip = "Takes about 2-3 minutes to run. Needs the above file.")]
        public void SaveSimulationOutput221_ThenLoad_Same()
        {
            SaveSimulationOutput_ThenLoad_Same(CannedSimulationOutput.Filename221);
        }
    }
}
