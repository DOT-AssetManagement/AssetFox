using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Extensions;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DTOs;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using MoreLinq.Extensions;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Models;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL
{
    public class BudgetAmountRepository : IBudgetAmountRepository
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;

        public BudgetAmountRepository(UnitOfDataPersistenceWork unitOfWork) => _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        public void UpsertOrDeleteBudgetAmounts(Dictionary<Guid, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid libraryId)
        {
            var budgetAmountEntities = budgetAmountsPerBudgetId
                .SelectMany(_ => _.Value.Select(__ => __.ToLibraryEntity(_.Key))).ToList();

            var entityIds = budgetAmountEntities.Select(_ => _.Id).ToList();

            var existingEntityIds = _unitOfWork.Context.BudgetAmount.AsNoTracking()
                .Where(_ => _.Budget.BudgetLibraryId == libraryId && entityIds.Contains(_.Id)).Select(_ => _.Id)
                .ToList();

            _unitOfWork.Context.DeleteAll<BudgetAmountEntity>(_ =>
                _.Budget.BudgetLibraryId == libraryId && !entityIds.Contains(_.Id));

            _unitOfWork.Context.UpdateAll(
                budgetAmountEntities.Where(_ => existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);

            _unitOfWork.Context.AddAll(
                budgetAmountEntities.Where(_ => !existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);
        }
        public void SaveScenarioBudgetAmounts(InvestmentUpsertAndDeleteModel changes, Guid simulationId)
        {
            DeleteScenariobudgetamounts(changes.Deletionyears, simulationId);
            InsertScenarioBudgetAmounts(changes.AddedBudgetAmounts, simulationId, changes.FirstYearAnalysisBudgetShift);
            UpdateScenarioBudgetAmounts(changes.UpdatedBudgetAmounts, simulationId, changes.FirstYearAnalysisBudgetShift);           
        }
        private void InsertScenarioBudgetAmounts(Dictionary<string, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid simulationId, int shift = 0)
        {
            var entities = GetScenarioAmountEntities(budgetAmountsPerBudgetId, simulationId);
            _unitOfWork.Context.AddAll(entities, _unitOfWork.UserEntity?.Id);
        }
        private void UpdateScenarioBudgetAmounts(Dictionary<string, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid simulationId, int shift = 0)
        {
            var entities = GetScenarioAmountEntities(budgetAmountsPerBudgetId, simulationId);
            _unitOfWork.Context.UpdateAll(entities, _unitOfWork.UserEntity?.Id);
        }
        private List<ScenarioBudgetAmountEntity> GetScenarioAmountEntities(Dictionary<string, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid simulationId, int shift = 0)
        {
            var budgetNames = budgetAmountsPerBudgetId.Keys.ToList();
            var budgets = _unitOfWork.Context.ScenarioBudget.AsNoTracking().Where(_ => budgetNames.Contains(_.Name) && _.SimulationId == simulationId).ToList();
            var amountEntities = new List<ScenarioBudgetAmountEntity>();
            budgets.ForEach(_ =>
            {
                budgetAmountsPerBudgetId[_.Name].ForEach(__ => amountEntities.Add(__.ToScenarioEntity(_.Id)));
            });
            amountEntities.ForEach(_ => _.Year += shift);
            return amountEntities;
        }
        public void DeleteScenariobudgetamounts(List<int> years, Guid SimulationId)
        {
            _unitOfWork.Context.DeleteAll<ScenarioBudgetAmountEntity>(_ => years.Contains(_.Year) && _.ScenarioBudget.SimulationId == SimulationId);
        }

        public void SaveLibraryBudgetAmounts(InvestmentUpsertAndDeleteModel changes, Guid simulationId)
        {
            DeleteLibrarybudgetamounts(changes.Deletionyears, simulationId);
            InsertLibraryBudgetAmounts(changes.AddedBudgetAmounts, simulationId, changes.FirstYearAnalysisBudgetShift);
            UpdateLibraryBudgetAmounts(changes.UpdatedBudgetAmounts, simulationId, changes.FirstYearAnalysisBudgetShift);
        }
        private void InsertLibraryBudgetAmounts(Dictionary<string, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid libraryId, int shift = 0)
        {
            var entities = GetLibraryAmountEntities(budgetAmountsPerBudgetId, libraryId);
            _unitOfWork.Context.AddAll(entities, _unitOfWork.UserEntity?.Id);
        }
        private void UpdateLibraryBudgetAmounts(Dictionary<string, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid libraryId, int shift = 0)
        {
            var entities = GetLibraryAmountEntities(budgetAmountsPerBudgetId, libraryId);
            _unitOfWork.Context.UpdateAll(entities, _unitOfWork.UserEntity?.Id);
        }
        private List<BudgetAmountEntity> GetLibraryAmountEntities(Dictionary<string, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid libraryId, int shift = 0)
        {
            var budgetNames = budgetAmountsPerBudgetId.Keys.ToList();
            var budgets = _unitOfWork.Context.Budget.AsNoTracking().Where(_ => budgetNames.Contains(_.Name) && _.BudgetLibraryId == libraryId).ToList();
            var amountEntities = new List<BudgetAmountEntity>();
            budgets.ForEach(_ =>
            {
                budgetAmountsPerBudgetId[_.Name].ForEach(__ => amountEntities.Add(__.ToLibraryEntity(_.Id)));
            });
            amountEntities.ForEach(_ => _.Year += shift);
            return amountEntities;
        }
        public void DeleteLibrarybudgetamounts(List<int> years, Guid librayId)
        {
            _unitOfWork.Context.DeleteAll<BudgetAmountEntity>(_ => years.Contains(_.Year) && _.Budget.BudgetLibraryId == librayId);
        }

        public void UpsertOrDeleteScenarioBudgetAmounts(Dictionary<Guid, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid simulationId)
        {
            var budgetAmountEntities = budgetAmountsPerBudgetId
                .SelectMany(_ => _.Value.Select(amount => amount.ToScenarioEntity(_.Key))).ToList();

            var entityIds = budgetAmountEntities.Select(_ => _.Id).ToList();

            var existingEntityIds = _unitOfWork.Context.ScenarioBudgetAmount.AsNoTracking()
                .Where(_ => _.ScenarioBudget.SimulationId == simulationId && entityIds.Contains(_.Id)).Select(_ => _.Id)
                .ToList();

            _unitOfWork.Context.DeleteAll<ScenarioBudgetAmountEntity>(_ =>
                _.ScenarioBudget.SimulationId == simulationId && !entityIds.Contains(_.Id));

            _unitOfWork.Context.UpdateAll(
                budgetAmountEntities.Where(_ => existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);

            _unitOfWork.Context.AddAll(
                budgetAmountEntities.Where(_ => !existingEntityIds.Contains(_.Id)).ToList(), _unitOfWork.UserEntity?.Id);
        }

        public List<BudgetAmountDTO> GetLibraryBudgetAmounts(Guid libraryId)
        {
            if (!_unitOfWork.Context.BudgetLibrary.Any(_ => _.Id == libraryId))
            {
                throw new RowNotInTableException("The specified budget library was not found.");
            }

            var dtos = _unitOfWork.Context.BudgetAmount
                .Include(_ => _.Budget)
                .OrderBy(_ => _.Budget.BudgetOrder)
                .Where(_ => _.Budget.BudgetLibrary.Id == libraryId)
                .Select(budgetAmount => new BudgetAmountDTO
                {
                    Year = budgetAmount.Year,
                    Value = budgetAmount.Value,
                    BudgetName = budgetAmount.Budget.Name,
                }).ToList();
            return dtos;
        }

        public List<BudgetAmountDTO> GetScenarioBudgetAmounts(Guid simulationId)
        {
            if (!_unitOfWork.Context.Simulation.Any(_ => _.Id == simulationId))
            {
                throw new RowNotInTableException("The specified simulation was not found for the given scenario.");
            }

            return _unitOfWork.Context.ScenarioBudgetAmount.AsNoTracking()
                .Where(_ => _.ScenarioBudget.SimulationId == simulationId)
                .Include(sb => sb.ScenarioBudget)
                .Select(budgetAmount => new BudgetAmountDTO
                {
                    Year = budgetAmount.Year,
                    Value = budgetAmount.Value,
                    BudgetName = budgetAmount.ScenarioBudget.Name,
                }).AsNoTracking().ToList();
        }
    }
}
