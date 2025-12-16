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
    public class CashFlowPagingService : PagingService<CashFlowRuleDTO, CashFlowRuleLibraryDTO>,  ICashFlowPagingService
    {
        private static IUnitOfWork _unitOfWork;

        public CashFlowPagingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        protected override List<CashFlowRuleDTO> GetScenarioRows(Guid scenarioId) => _unitOfWork.CashFlowRuleRepo.GetScenarioCashFlowRules(scenarioId);

        protected override List<CashFlowRuleDTO> GetLibraryRows(Guid libraryId) => _unitOfWork.CashFlowRuleRepo.GetCashFlowRulesByLibraryId(libraryId);
        protected override List<CashFlowRuleDTO> CreateAsNewDataset(List<CashFlowRuleDTO> rows)
        {
            rows.ForEach(_ =>
            {
                _.Id = Guid.NewGuid();
                if (_.CriterionLibrary != null)
                {
                    _.CriterionLibrary.Id = Guid.NewGuid();
                }
                _.CashFlowDistributionRules.ForEach(__ => __.Id = Guid.NewGuid());
            });

            return rows;
        }
    }
}
