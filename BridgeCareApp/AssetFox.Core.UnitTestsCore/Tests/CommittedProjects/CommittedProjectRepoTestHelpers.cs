using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests.CommittedProjects
{
    public static class CommittedProjectRepoTestHelpers
    {

        public static Simulation CreateSimulation(Guid simulationId, IUnitOfWork unitOfWork, bool populateInvestments = true)
        {
            var explorer = unitOfWork.AttributeRepo.GetExplorer();
            var attributeNameLookup = unitOfWork.AttributeRepo.GetIdNameCache();
            var testNetwork = explorer.AddNetwork();
            testNetwork.Id = TestDataForCommittedProjects.NetworkId;
            SectionMapper mapper = new(testNetwork);
            foreach (var asset in TestEntitiesForCommittedProjects.MaintainableAssetEntities)
            {
                mapper.CreateMaintainableAsset(asset, attributeNameLookup);
            }
            var simulation = testNetwork.AddSimulation();
            simulation.Id = simulationId;
            // This has to be ignored to create a bad scenario object
            if (populateInvestments) unitOfWork.InvestmentPlanRepo.GetSimulationInvestmentPlan(simulation);
            return simulation;
        }

    }
}
