using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories
{
    public interface ITreatmentPerformanceFactorRepository
    {
        void UpsertScenarioTreatmentPerformanceFactors(Dictionary<Guid, List<TreatmentPerformanceFactorDTO>> scenarioTreatmentPerformanceFactorPerTreatmentId,
            Guid SimulationId);

        void UpsertLibraryTreatmentPerformanceFactors(Dictionary<Guid, List<TreatmentPerformanceFactorDTO>> TreatmentPerformanceFactorPerTreatmentId,
           Guid LibraryId);
    }
}
