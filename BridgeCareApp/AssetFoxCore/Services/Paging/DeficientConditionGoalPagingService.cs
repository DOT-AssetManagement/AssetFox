using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using System.Linq;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models;
using AssetFoxCore.Services.Paging.Generics;

namespace AssetFoxCore.Services
{
    public class DeficientConditionGoalPagingService : PagingService<DeficientConditionGoalDTO, DeficientConditionGoalLibraryDTO>,  IDeficientConditionGoalPagingService
    {
        private static IUnitOfWork _unitOfWork;

        public DeficientConditionGoalPagingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        protected override List<DeficientConditionGoalDTO> GetScenarioRows(Guid scenarioId) => _unitOfWork.DeficientConditionGoalRepo.GetScenarioDeficientConditionGoals(scenarioId);

        protected override List<DeficientConditionGoalDTO> GetLibraryRows(Guid libraryId) => _unitOfWork.DeficientConditionGoalRepo.GetDeficientConditionGoalsByLibraryId(libraryId);
        protected override List<DeficientConditionGoalDTO> CreateAsNewDataset(List<DeficientConditionGoalDTO> rows)
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
