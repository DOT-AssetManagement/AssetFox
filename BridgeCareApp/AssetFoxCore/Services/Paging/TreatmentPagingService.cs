using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models;
using AssetFoxCore.Services.Paging.Generics;

namespace AssetFoxCore.Services
{
    public class TreatmentPagingService : PagingService<TreatmentDTO, TreatmentLibraryDTO>, ITreatmentPagingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TreatmentPagingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private void FixNullLists(TreatmentDTO dto)
        {
            dto.Consequences ??= new List<TreatmentConsequenceDTO>();
            dto.Costs ??= new List<TreatmentCostDTO>();
        }

        public override List<TreatmentDTO> GetSyncedScenarioDataSet(Guid simulationId, PagingSyncModel<TreatmentDTO> request)
        {
            var rows = request.LibraryId == null ?
                    _unitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatments(simulationId) :
                    _unitOfWork.SelectableTreatmentRepo.GetSelectableTreatments(request.LibraryId.Value);
            rows = SyncDataset(rows, request);
            _unitOfWork.SelectableTreatmentRepo.AddDefaultPerformanceFactors(simulationId, rows);
            if (request.LibraryId != null)
            {
                var budgets = _unitOfWork.BudgetRepo.GetScenarioBudgets(simulationId);
                var budgetIds = budgets.Select(_ => _.Id).ToList();

                rows.ForEach(_ =>
                {
                    FixNullLists(_);
                    _.Id = Guid.NewGuid();
                    if (_.CriterionLibrary != null)
                    {
                        _.CriterionLibrary.Id = Guid.NewGuid();
                    }
                    _.Consequences.ForEach(__ =>
                    {
                        __.Id = Guid.NewGuid();
                        if (__.CriterionLibrary != null)
                        {
                            __.CriterionLibrary.Id = Guid.NewGuid();
                        }
                        if (__.Equation != null)
                        {
                            __.Equation.Id = Guid.NewGuid();
                        }
                    });
                    _.Costs.ForEach(__ =>
                    {
                        __.Id = Guid.NewGuid();
                        if (__.Equation != null)
                        {
                            __.Equation.Id = Guid.NewGuid();
                        }
                        if (__.CriterionLibrary != null)
                        {
                            __.CriterionLibrary.Id = Guid.NewGuid();
                        }
                    });
                    if (_.BudgetIds == null || !_.BudgetIds.Any())
                    {
                        _.BudgetIds = budgetIds;
                    }
                    _.SupersedeRules.ForEach(__ =>
                    {
                        __.Id = Guid.NewGuid();
                        __.treatment = rows.First(row => row.Name == __.treatment.Name);
                    });
                   
                });
            }           
            return rows;
        }

        protected override List<TreatmentDTO> GetScenarioRows(Guid scenarioId) => _unitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatments(scenarioId);

        protected override List<TreatmentDTO> GetLibraryRows(Guid libraryId) => _unitOfWork.SelectableTreatmentRepo.GetSelectableTreatments(libraryId);
        protected override List<TreatmentDTO> CreateAsNewDataset(List<TreatmentDTO> rows)
        {
            // Dictionary to map Names to the new Guids for O(1) lookup
            var nameIdMap = new Dictionary<string, Guid>();

            // PASS 1: Assign IDs and populate the Map
            foreach (var row in rows)
            {
                row.Id = Guid.NewGuid();

                if (!string.IsNullOrEmpty(row.Name) && !nameIdMap.ContainsKey(row.Name))
                {
                    nameIdMap.Add(row.Name, row.Id);
                }

                // Handle Children
                if (row.CriterionLibrary != null) row.CriterionLibrary.Id = Guid.NewGuid();

                row.Consequences?.ForEach(c =>
                {
                    c.Id = Guid.NewGuid();
                    if (c.CriterionLibrary != null) c.CriterionLibrary.Id = Guid.NewGuid();
                    if (c.Equation != null) c.Equation.Id = Guid.NewGuid();
                });

                row.Costs?.ForEach(c =>
                {
                    c.Id = Guid.NewGuid();
                    if (c.Equation != null) c.Equation.Id = Guid.NewGuid();
                    if (c.CriterionLibrary != null) c.CriterionLibrary.Id = Guid.NewGuid();
                });

                row.PerformanceFactors?.ForEach(p =>
                {
                    p.Id = Guid.NewGuid();
                });
            }

            // PASS 2: Resolve supersede references using the Map
            foreach (var row in rows)
            {
                if (row.SupersedeRules == null) continue;

                foreach (var rule in row.SupersedeRules)
                {
                    rule.Id = Guid.NewGuid();

                    if (rule.treatment != null && nameIdMap.TryGetValue(rule.treatment.Name, out Guid newId))
                    {
                        rule.treatment.Id = newId;
                    }
                }
            }

            return rows;
        }
    }
}
