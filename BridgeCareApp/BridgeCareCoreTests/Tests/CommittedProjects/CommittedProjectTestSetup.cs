using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFoxCoreTests.Tests
{
    public static class CommittedProjectTestSetup
    {
        public static SectionCommittedProjectDTO ModelForEntityInDb(IUnitOfWork unitOfWork, Guid scenarioBudgetId, Guid simulationId, string locationKey, string locationValue, string treatmentName, int year)
        {
            var dto = SectionCommittedProjectDtos.Dto(null, scenarioBudgetId, simulationId, locationKey: locationKey, locationValue: locationValue, treatment: treatmentName, year: year);
            var projects = new List<SectionCommittedProjectDTO> { dto };
            unitOfWork.CommittedProjectRepo.UpsertCommittedProjects(projects);
            return dto;
        }
    }
}
