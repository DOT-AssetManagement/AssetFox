using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public static class InvestmentPlanTestSetup
    {
        public static InvestmentPlanDTO ModelForEntityInDb(
            UnitOfDataPersistenceWork unitOfWork,
            Guid simulationId,
            Guid? id,
            int firstYearOfAnalysisPeriod,
            int numberOfYearsInAnalysisPeriod = 1)
        {
            var dto = InvestmentPlanDtos.Dto(id, firstYearOfAnalysisPeriod, numberOfYearsInAnalysisPeriod);
            unitOfWork.InvestmentPlanRepo.UpsertInvestmentPlan(dto, simulationId);
            return dto;
        }
    }
}
