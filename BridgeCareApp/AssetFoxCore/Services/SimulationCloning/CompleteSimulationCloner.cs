using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DTOs;
using AssetFoxCore.Services.SimulationCloning;

namespace AssetFoxCore.Services
{
    public class CompleteSimulationCloner
    {
        public static CompleteSimulationDTO Clone(CompleteSimulationDTO completeSimulation, CloneSimulationDTO cloneRequest, Guid ownerId, string ownerName)
        {
            var isToSameNetwork = completeSimulation.NetworkId == cloneRequest.NetworkId;
            var cloneAnalysisMethod = AnalysisMethodCloner.Clone(completeSimulation.AnalysisMethod, ownerId, isToSameNetwork);            
            var cloneCashFlowFule = CashFlowRuleCloner.CloneList(completeSimulation.CashFlowRules, ownerId);
            var cloneInvestmentPlan = InvestmentPlanCloner.Clone(completeSimulation.InvestmentPlan);
            //var cloneReportIndex = new List<ReportIndexDTO>(); // ReportIndexCloner.CloneList(completeSimulation.ReportIndexes); // commenting for now, need to fix the report paths and handle actual coping of report files
            var clonePerformanceCurves = PerformanceCurvesCloner.CloneListNullPropagating(completeSimulation.PerformanceCurves, ownerId);
            var cloneCalculatedAttribute = CalculatedAttributeCloner.CloneList(completeSimulation.CalculatedAttributes, ownerId);
            var sourceBudgets = (completeSimulation.Budgets ?? new List<BudgetDTO>()).ToList();
            var cloneBudget = BudgetCloner.CloneList(sourceBudgets, ownerId);
            var sourceAndClonedBudgetPairs = sourceBudgets
                .Zip(cloneBudget, (source, clone) => new { Source = source, Clone = clone })
                .Where(pair => pair.Source.Id != Guid.Empty)
                .ToList();
            var budgetIdMap = sourceAndClonedBudgetPairs
                .GroupBy(pair => pair.Source.Id)
                .ToDictionary(group => group.Key, group => group.First().Clone.Id);
            var clonedBudgetsBySourceId = sourceAndClonedBudgetPairs
                .GroupBy(pair => pair.Source.Id)
                .ToDictionary(group => group.Key, group => group.First().Clone);
         
            var cloneRemainingLifeLimits = RemainingLifeLimitCloner.CloneList(completeSimulation.RemainingLifeLimits, ownerId);
            var cloneTreatment = TreatmentCloner.CloneList(completeSimulation.Treatments, ownerId, clonedBudgetsBySourceId);
            var cloneTreatmentRuleOverrides = TreatmentRuleOverrideCloner.CloneList(completeSimulation.TreatmentRuleOverrides, completeSimulation.Treatments, cloneTreatment);
            var cloneTargetConditionGoal = TargetConditionGoalCloner.CloneList(completeSimulation.TargetConditionGoals, ownerId);
            var cloneDeficientConditionGoal = DeficientConditionGoalCloner.CloneList(completeSimulation.DeficientConditionGoals, ownerId);
            var cloneBudgetPriorities = BudgetPriorityCloner.CloneList(completeSimulation.BudgetPriorities, budgetIdMap, ownerId);
            var cloneBaseCommittedProject = BaseCommittedProjectCloner.CloneList(completeSimulation.CommittedProjects, budgetIdMap);
            var user = new SimulationUserDTO
            {
                CanModify = true,
                UserId = ownerId,
                IsOwner = true,
                Username = ownerName,
            };
            var users = new List<SimulationUserDTO>
            {
                
            };
            if (ownerId != Guid.Empty&&! string.IsNullOrEmpty(ownerName))
            {
                users.Add(user);
            }

            //var cloneSimulationOutput = SimulationOutputCloner.Clone(completeSimulation.SimulationOutput);
            
            //var simulationAnalysisDetail = SimulationAnalysisDetailCloner.Clone(completeSimulation.SimulationAnalysisDetail);

            var clone = new CompleteSimulationDTO
            {
                NoTreatmentBeforeCommittedProjects = completeSimulation.NoTreatmentBeforeCommittedProjects,
                Name = cloneRequest.ScenarioName,
                NetworkId = cloneRequest.NetworkId,
                //figure out where the properties come from
                AnalysisMethod = cloneAnalysisMethod,
                //ReportIndexes = cloneReportIndex,
                PerformanceCurves = clonePerformanceCurves,
                CalculatedAttributes = cloneCalculatedAttribute,
                BudgetPriorities = cloneBudgetPriorities,
                CashFlowRules = cloneCashFlowFule,
                InvestmentPlan = cloneInvestmentPlan,
                RemainingLifeLimits = cloneRemainingLifeLimits,
                Treatments = cloneTreatment,
                TreatmentRuleOverrides = cloneTreatmentRuleOverrides,
                TargetConditionGoals = cloneTargetConditionGoal,
                DeficientConditionGoals = cloneDeficientConditionGoal,
                Budgets = cloneBudget,
                CommittedProjects = cloneBaseCommittedProject,
                Id = Guid.NewGuid(),
                Users = users,
                //SimulationOutput = cloneSimulationOutput,
                //SimulationAnalysisDetail = simulationAnalysisDetail
            };

            return clone;
        }        
    }
}
