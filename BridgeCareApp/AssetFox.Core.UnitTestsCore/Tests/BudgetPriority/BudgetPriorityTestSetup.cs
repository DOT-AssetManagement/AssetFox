using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public static class BudgetPriorityTestSetup
    {
        public static BudgetPriorityDTO SetupSingleBudgetPriorityForSimulationInDb(Guid simulationId)
        {
            var budgetPriority = BudgetPriorityDtos.New();
            var budgetPriorities = new List<BudgetPriorityDTO> { budgetPriority };
            TestHelper.UnitOfWork.BudgetPriorityRepo.UpsertOrDeleteScenarioBudgetPriorities(budgetPriorities, simulationId);
            return budgetPriority;
        }

        public static BudgetPriorityDTO SetupSingleBudgetPriorityWithCriterionLibraryForSimulationInDb(Guid simulationId)
        {
            var budgetPriority = BudgetPriorityDtos.WithCriterionLibrary();
            var budgetPriorities = new List<BudgetPriorityDTO> { budgetPriority };
            TestHelper.UnitOfWork.BudgetPriorityRepo.UpsertOrDeleteScenarioBudgetPriorities(budgetPriorities, simulationId);
            return budgetPriority;
        }

        public static BudgetPriorityDTO ModelForLibraryInDb(IUnitOfWork unitOfWork, Guid libraryId, Guid? id = null)
        {
            var budgetPriority = BudgetPriorityDtos.New(id);
            var budgetPriorities = new List<BudgetPriorityDTO> { budgetPriority };
            unitOfWork.BudgetPriorityRepo.UpsertOrDeleteBudgetPriorities(budgetPriorities, libraryId);
            return budgetPriority;
        }
    }
}
