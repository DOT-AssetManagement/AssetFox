using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.TestHelpers.Assertions;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using AppliedResearchAssociates.iAM.Common;

using Xunit;
using Xunit.Sdk;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public class SimulationOutputRepoTests
    {
        [Fact]
        public void SaveSimulationOutput_Does()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var context = SimulationOutputCreationContextTestSetup.SimpleContextWithObjectsInDatabase(TestHelper.UnitOfWork);
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaRelational(context.SimulationId, simulationOutput);
        }

        [Fact]
        public void SaveSimulationOutput_ThenLoad_Same()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var context = SimulationOutputCreationContextTestSetup.SimpleContextWithObjectsInDatabase(TestHelper.UnitOfWork);
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaRelational(context.SimulationId, simulationOutput);
            var loadedOutput = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(context.SimulationId);
            ObjectAssertions.Equivalent(simulationOutput.InitialAssetSummaries, loadedOutput.InitialAssetSummaries);
            ObjectAssertions.EquivalentExcluding(simulationOutput, loadedOutput, so => so.LastModifiedDate,
                so => so.Years[0].Assets[0].TreatmentSchedulingCollisions);
        }

        [Theory]
        
        [InlineData(2)]
        //[InlineData(12)] // commented out because this case takes about 20 seconds as of 5/14/25
        public void SaveMultiYearSimulationOutput_ThenLoad_Same(int numberOfYears)
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var context = SimulationOutputCreationContextTestSetup.SimpleContextWithObjectsInDatabase(TestHelper.UnitOfWork, numberOfYears);
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaRelational(context.SimulationId, simulationOutput);
            var loadedOutput = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(context.SimulationId);
            ObjectAssertions.Equivalent(simulationOutput.InitialAssetSummaries, loadedOutput.InitialAssetSummaries);
            SimulationOutputAssertions.SameSimulationOutput(simulationOutput, loadedOutput, false);
        }

        [Fact (Skip="Takes approximately 30 seconds as of 5/14/25")]
        public void SaveSimulationOutputWithMoreAssetsThanBatchSize_ThenLoad_Same()
        {
            var numberOfAssets = 25 + SimulationOutputRepository.AssetLoadBatchSize;
            var assetNameIdPairs = AssetNameIdPairLists.Random(numberOfAssets);
            var numericAttributeName = RandomStrings.WithPrefix("NumericAttribute");
            var textAttributeName = RandomStrings.WithPrefix("TextAttrbute");
            var numericAttributeNames = new List<string> { numericAttributeName };
            var textAttributeNames = new List<string> { textAttributeName };
            var context = SimulationOutputCreationContextTestSetup.ContextWithObjectsInDatabase(
                TestHelper.UnitOfWork,
                assetNameIdPairs,
                numericAttributeNames,
                textAttributeNames
                );
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaRelational(context.SimulationId, simulationOutput);
            var loadedOutput = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(context.SimulationId);
            simulationOutput.InitialAssetSummaries.Sort(a => a.AssetName);
            loadedOutput.InitialAssetSummaries.Sort(a => a.AssetName);
            ObjectAssertions.Equivalent(simulationOutput.InitialAssetSummaries, loadedOutput.InitialAssetSummaries);
            SimulationOutputAssertions.SameSimulationOutput(simulationOutput, loadedOutput, false);
        }

        [Fact (Skip="Took 1.7 minutes as of 5/14/25")]
        public void SaveSimulationOutput_ThenLoad_LastModifiedDate_Expected()
        {
            var numberOfYears = 1;
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var context = SimulationOutputCreationContextTestSetup.SimpleContextWithObjectsInDatabase(TestHelper.UnitOfWork, numberOfYears);
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            var startDate = DateTime.Now;
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaRelational(context.SimulationId, simulationOutput);
            var endDate = DateTime.Now;
            var loadedOutput = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(context.SimulationId);
            var lastModifiedDate = loadedOutput.LastModifiedDate;
            DateTimeAssertions.Between(startDate, endDate, lastModifiedDate, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void SaveSimulationOutput_UpdateSimulation_ThenLoad_LastModifiedDate_MatchesSimulationUpdateDate()
        {
            var numberOfYears = 1;
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var context = SimulationOutputCreationContextTestSetup.SimpleContextWithObjectsInDatabase(TestHelper.UnitOfWork, numberOfYears);
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaRelational(context.SimulationId, simulationOutput);
            var oldSimulation = TestHelper.UnitOfWork.SimulationRepo.GetSimulation(context.SimulationId);
            oldSimulation.Name = RandomStrings.WithPrefix("Modified simulation");
            var updateStartDate = DateTime.Now;
            TestHelper.UnitOfWork.SimulationRepo.UpdateSimulationAndPossiblyUsers(oldSimulation);
            var updateEndDate = DateTime.Now;
            var loadedOutput = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaRelation(context.SimulationId);
            var lastModifiedDate = loadedOutput.LastModifiedDate;
            DateTimeAssertions.Between(updateStartDate, updateEndDate, lastModifiedDate, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void CreateSimulationOutputViaJson_Does()
        {
            var numberOfYears = 1;
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var context = SimulationOutputCreationContextTestSetup.SimpleContextWithObjectsInDatabase(TestHelper.UnitOfWork, numberOfYears);
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            var before = DateTime.Now;

            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaJson(context.SimulationId, simulationOutput);

            var after = DateTime.Now;
            var simulationOutputAfter = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaJson(context.SimulationId);
            ObjectAssertions.EquivalentExcluding(simulationOutput, simulationOutputAfter, so => so.LastModifiedDate);
            DateTimeAssertions.Between(before, after, simulationOutputAfter.LastModifiedDate, TimeSpan.FromSeconds(1));
        }


        [Fact]
        public void ConvertOutputFromJsonToRelational_JsonOutputInDb_Converts()
        {
            var numberOfYears = 1;
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var context = SimulationOutputCreationContextTestSetup.SimpleContextWithObjectsInDatabase(TestHelper.UnitOfWork, numberOfYears);
            var simulationOutput = SimulationOutputModels.SimulationOutput(context);
            TestHelper.UnitOfWork.SimulationOutputRepo.CreateSimulationOutputViaJson(context.SimulationId, simulationOutput);

            TestHelper.UnitOfWork.SimulationOutputRepo.ConvertSimulationOutpuFromJsonTorelational(context.SimulationId);

            var simulationOutputAfter = TestHelper.UnitOfWork.SimulationOutputRepo.GetSimulationOutputViaJson(context.SimulationId);
            ObjectAssertions.EquivalentExcluding(simulationOutput, simulationOutputAfter, so => so.LastModifiedDate);
        }
    }
}
