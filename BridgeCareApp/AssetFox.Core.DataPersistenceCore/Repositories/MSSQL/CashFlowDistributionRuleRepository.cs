using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CashFlow;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CashFlow;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class CashFlowDistributionRuleRepository : ICashFlowDistributionRuleRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public CashFlowDistributionRuleRepository(UnitOfDataPersistenceWork unitOfWork) =>
            _unitOfWork = unitOfWork ??
                                         throw new ArgumentNullException(nameof(unitOfWork));


        public void UpsertOrDeleteCashFlowDistributionRules(
            Dictionary<Guid, List<CashFlowDistributionRuleDTO>> distributionRulesPerCashFlowRuleId, Guid libraryId)
        {
            var cashFlowDistributionRuleEntities = distributionRulesPerCashFlowRuleId
                .SelectMany(_ => _.Value.Select(distributionRule => distributionRule.ToLibraryEntity(_.Key)))
                .ToList();

            var entityIds = cashFlowDistributionRuleEntities.Select(_ => _.Id).ToList();

            var existingEntityIds = _unitOfWork.Context.CashFlowDistributionRule.AsNoTracking()
                .Where(_ => _.CashFlowRule.CashFlowRuleLibraryId == libraryId && entityIds.Contains(_.Id))
                .Select(_ => _.Id).ToList();

            _unitOfWork.Context
                .DeleteAll<CashFlowDistributionRuleEntity>(_ => _.CashFlowRule.CashFlowRuleLibraryId == libraryId &&
                                                                !entityIds.Contains(_.Id));

            _unitOfWork.Context
                .UpdateAll(cashFlowDistributionRuleEntities.Where(_ => existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);

            _unitOfWork.Context
                .AddAll(cashFlowDistributionRuleEntities.Where(_ => !existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);
        }

        public void UpsertOrDeleteScenarioCashFlowDistributionRules(
            Dictionary<Guid, List<CashFlowDistributionRuleDTO>> distributionRulesPerCashFlowRuleId, Guid simulationId)
        {
            var cashFlowDistributionRuleEntities = distributionRulesPerCashFlowRuleId
                .SelectMany(_ => _.Value.Select(distributionRule => distributionRule.ToScenarioEntity(_.Key)))
                .ToList();

            var entityIds = cashFlowDistributionRuleEntities.Select(_ => _.Id).ToList();

            var existingEntityIds = _unitOfWork.Context.ScenarioCashFlowDistributionRule.AsNoTracking()
                .Where(_ => _.ScenarioCashFlowRule.SimulationId == simulationId && entityIds.Contains(_.Id))
                .Select(_ => _.Id).ToList();

            _unitOfWork.Context
                .DeleteAll<ScenarioCashFlowDistributionRuleEntity>(_ => _.ScenarioCashFlowRule.SimulationId == simulationId &&
                                                                !entityIds.Contains(_.Id));

            _unitOfWork.Context
                .UpdateAll(cashFlowDistributionRuleEntities.Where(_ => existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);

            _unitOfWork.Context
                .AddAll(cashFlowDistributionRuleEntities.Where(_ => !existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);
        }
    }
}
