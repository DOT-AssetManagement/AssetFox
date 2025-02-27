using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;

namespace BridgeCareCoreTests.Tests
{
    public static class CommittedProjectTestSetup
    {
        public static SectionCommittedProjectDTO ModelForEntityInDb(Guid scenarioBudgetId, Guid simulationId, string locationKey, string locationValue, string treatmentName)
        {
            var dto = SectionCommittedProjectDtos.Dto(null, scenarioBudgetId, simulationId, locationKey: locationKey, locationValue: locationValue, treatment: treatmentName);
            var projects = new List<SectionCommittedProjectDTO> { dto };
            TestHelper.UnitOfWork.CommittedProjectRepo.UpsertCommittedProjects(projects);
            return dto;
        }
    }
}
