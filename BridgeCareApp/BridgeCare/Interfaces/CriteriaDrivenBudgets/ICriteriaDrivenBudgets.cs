using System.Collections.Generic;
using System.Threading.Tasks;
using BridgeCare.Models;

namespace BridgeCare.Interfaces.CriteriaDrivenBudgets
{
    public interface ICriteriaDrivenBudgets
    {
        List<CriteriaDrivenBudgetModel> GetAnyCriteriaDrivenBudgets(int simulationId, BridgeCareContext db);

        List<CriteriaDrivenBudgetModel> GetPermittedCriteriaDrivenBudgets(int simulationId, BridgeCareContext db, string username);

        Task<string> SaveAnyCriteriaDrivenBudgets(int selectedScenarioId, List<CriteriaDrivenBudgetModel> data, BridgeCareContext db);

        Task<string> SavePermittedCriteriaDrivenBudgets(int selectedScenarioId, List<CriteriaDrivenBudgetModel> data, BridgeCareContext db, string username);
    }
}
