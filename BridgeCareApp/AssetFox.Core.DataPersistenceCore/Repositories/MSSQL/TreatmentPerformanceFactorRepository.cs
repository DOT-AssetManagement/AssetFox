using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.Analysis;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class TreatmentPerformanceFactorRepository : ITreatmentPerformanceFactorRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public TreatmentPerformanceFactorRepository(UnitOfDataPersistenceWork unitOfWork) =>
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        public void UpsertScenarioTreatmentPerformanceFactors(Dictionary<Guid, List<TreatmentPerformanceFactorDTO>> scenarioTreatmentPerformanceFactorsPerTreatmentId,
            Guid SimulationId)
        {
            var scenarioTreatmentPerformanceFactorEntities = scenarioTreatmentPerformanceFactorsPerTreatmentId
                .SelectMany(_ => _.Value.Select(factor => factor
                    .ToScenarioEntity(_.Key)))
                .ToList();

            var entityIds = scenarioTreatmentPerformanceFactorEntities.Select(_ => _.Id).ToList();

            var existingEntityIds = _unitOfWork.Context.ScenarioTreatmentPerformanceFactor.AsNoTracking()
                .Where(_ => _.ScenarioSelectableTreatment.SimulationId == SimulationId && entityIds.Contains(_.Id))
                .Select(_ => _.Id).ToList();

            _unitOfWork.Context.UpdateAll(scenarioTreatmentPerformanceFactorEntities.Where(_ => existingEntityIds.Contains(_.Id)).ToList());
            _unitOfWork.Context.AddAll(scenarioTreatmentPerformanceFactorEntities.Where(_ => !existingEntityIds.Contains(_.Id)).ToList());
        }
        
        public void UpsertLibraryTreatmentPerformanceFactors(Dictionary<Guid, List<TreatmentPerformanceFactorDTO>> TreatmentPerformanceFactorPerTreatmentId, Guid LibraryId)
        {
            var TreatmentPerformanceFactorEntities = TreatmentPerformanceFactorPerTreatmentId
                .SelectMany(_ => _.Value.Select(factor => factor
                    .ToLibraryEntity(_.Key)))
                .ToList();

            var entityIds = TreatmentPerformanceFactorEntities.Select(_ => _.Id).ToList();

            var existingEntityIds = _unitOfWork.Context.TreatmentPerformanceFactor.AsNoTracking()
                .Where(_ => _.SelectableTreatment.TreatmentLibraryId == LibraryId && entityIds.Contains(_.Id))
                .Select(_ => _.Id).ToList();

            _unitOfWork.Context.UpdateAll(TreatmentPerformanceFactorEntities.Where(_ => existingEntityIds.Contains(_.Id)).ToList());
            _unitOfWork.Context.AddAll(TreatmentPerformanceFactorEntities.Where(_ => !existingEntityIds.Contains(_.Id)).ToList());
        }
    }
}
