using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.CommittedProjects
{
    public class CommittedProjectRepositoryRealDatabaseTests
    {
        [Fact(Skip = "Test is not dependent on the No Treatment Before Committed Project Flag")]
        public void NoTreatmentBeforeCommittedProjects_GetSimulationCommittedProjects_Expected()
        {
            // Arrange
            var repo = new CommittedProjectRepository(TestHelper.UnitOfWork);
            var inputSimulationEntity = TestEntitiesForCommittedProjects.Simulations.Single(_ => _.Name == "FourYearTest");
            //var simulationDomain = CreateSimulation(inputSimulationEntity.Id, _testUOW);
            //var simulationEntity = _testUOW.Context.Simulation.Single(s => s.Id == simulationDomain.Id);
            //simulationEntity.NoTreatmentBeforeCommittedProjects = true;
            //_testUOW.Context.Simulation.Update(simulationEntity);
            //_testUOW.Context.SaveChanges();

            //// Act
            //repo.GetSimulationCommittedProjects(simulationDomain);

            //// Assert
            //var committedProjectNames = simulationDomain.CommittedProjects.Select(cp => cp.Name).ToList();
            //Assert.Equal(4, simulationDomain.CommittedProjects.Count);
            //Assert.Equal(10000, simulationDomain.CommittedProjects.Sum(_ => _.Cost));
            //Assert.Equal(3, simulationDomain.CommittedProjects.Count(_ => _.Name != "Something"));
        }


    }
}
