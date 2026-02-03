using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Services.Paging.Generics;

namespace AssetFoxCore.Services
{
    public class RemainingLifeLimitPagingService : PagingService<RemainingLifeLimitDTO, RemainingLifeLimitLibraryDTO>, IRemainingLifeLimitPagingService
    {
        private static IUnitOfWork _unitOfWork;

        public RemainingLifeLimitPagingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        protected override List<RemainingLifeLimitDTO> GetScenarioRows(Guid scenarioId)
        {
            return _unitOfWork.RemainingLifeLimitRepo.GetScenarioRemainingLifeLimits(scenarioId);
        }
        protected override List<RemainingLifeLimitDTO> GetLibraryRows(Guid libraryId)
        {
            return _unitOfWork.RemainingLifeLimitRepo.GetRemainingLifeLimitsByLibraryId(libraryId);
        }
        protected override List<RemainingLifeLimitDTO> CreateAsNewDataset(List<RemainingLifeLimitDTO>  rows)
        {
            rows.ForEach(_ =>
            {
                _.Id = Guid.NewGuid();
                if (_.CriterionLibrary != null)
                {
                    _.CriterionLibrary.Id = Guid.NewGuid();
                }
            });

            return rows;
        }
    }
}
