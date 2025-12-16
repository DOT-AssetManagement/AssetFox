using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.Analysis;
using System.Linq;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class TreatmentSchedulingMapper
    {
        public static TreatmentSchedulingEntity ToLibraryEntity(this TreatmentScheduling domain, Guid treatmentId) =>
            new TreatmentSchedulingEntity
            {
                Id = domain.Id,
                TreatmentId = treatmentId,
                OffsetToFutureYear = domain.OffsetToFutureYear
            };
        public static ScenarioTreatmentSchedulingEntity ToScenarioEntity(this TreatmentScheduling domain, Guid treatmentId) =>
            new ScenarioTreatmentSchedulingEntity
            {
                Id = domain.Id,
                TreatmentId = treatmentId,
                OffsetToFutureYear = domain.OffsetToFutureYear
            };

        public static void CreateTreatmentScheduling(this ScenarioTreatmentSchedulingEntity entity,
            SelectableTreatment selectableTreatment, Simulation simulation)
        {
            var scheduling = selectableTreatment.Schedulings.GetAdd(new TreatmentScheduling());
            scheduling.OffsetToFutureYear = entity.OffsetToFutureYear;
            scheduling.TreatmentToSchedule = simulation.Treatments.FirstOrDefault(_ => _.Id == selectableTreatment.Id);
        }
    }
}
