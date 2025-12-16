using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class InvestmentPlanRepository : IInvestmentPlanRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public InvestmentPlanRepository(UnitOfDataPersistenceWork unitOfWork) =>
            _unitOfWork = unitOfWork ??
                                         throw new ArgumentNullException(nameof(unitOfWork));

        public void GetSimulationInvestmentPlan(Simulation simulation)
        {
            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulation.Id))
            {
                throw new RowNotInTableException("No simulation was found for the given scenario.");
            }

            if(!_unitOfWork.Context.InvestmentPlan.Any(_ => _.Simulation.Id == simulation.Id))
            {
                throw new RowNotInTableException("No budgets were found for the given scenario.");
            }

            _unitOfWork.Context.InvestmentPlan.AsNoTracking()
                .Include(_ => _.Simulation)
                .ThenInclude(_ => _.Budgets)
                .ThenInclude(_ => _.ScenarioBudgetAmounts)
                .Include(_ => _.Simulation)
                .ThenInclude(_ => _.Budgets)
                .ThenInclude(_ => _.CriterionLibraryScenarioBudgetJoin)
                .ThenInclude(_ => _.CriterionLibrary)
                .Include(_ => _.Simulation)
                .ThenInclude(_ => _.CashFlowRules)
                .ThenInclude(_ => _.CriterionLibraryScenarioCashFlowRuleJoin)
                .ThenInclude(_ => _.CriterionLibrary)
                .Include(_ => _.Simulation)
                .ThenInclude(_ => _.CashFlowRules)
                .ThenInclude(_ => _.ScenarioCashFlowDistributionRules)
                .Single(_ => _.Simulation.Id == simulation.Id)
                .FillSimulationInvestmentPlan(simulation);
        }

        public InvestmentPlanDTO GetInvestmentPlan(Guid simulationId)
        {
            if (simulationId == Guid.Empty)
            {
                return new InvestmentPlanDTO();
            }

            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException("No simulation was found for the given scenario.");
            }

            var investmentPlan = _unitOfWork.Context.InvestmentPlan.AsNoTracking()
                .SingleOrDefault(_ => _.SimulationId == simulationId);
            return investmentPlan != null ? investmentPlan.ToDto() : new InvestmentPlanDTO();
        }

        public int[] GetInvestmentStartAndEndYears(Guid simulationId)
        {
            var investmentPlan = _unitOfWork.Context.InvestmentPlan.AsNoTracking().FirstOrDefault(ip => ip.SimulationId == simulationId);
            var startYear = investmentPlan.FirstYearOfAnalysisPeriod;
            var numYearsInAnalysis = investmentPlan.NumberOfYearsInAnalysisPeriod;
            var endYear = startYear + numYearsInAnalysis - 1;

            return new int[] { startYear, endYear };
        }

        public void UpsertInvestmentPlan(InvestmentPlanDTO dto, Guid simulationId)
        {
            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException("No simulation was found for the given scenario.");
            }

            var investmentPlanEntity = dto.ToEntity(simulationId);

            _unitOfWork.Context.Upsert(investmentPlanEntity, dto.Id, _unitOfWork.UserEntity?.Id);

            // Update last modified date
            _unitOfWork.SimulationRepo
                .UpdateLastModifiedDate(_unitOfWork.Context.Simulation.FirstOrDefault(_ => _.Id == simulationId));
        }
    }
}
