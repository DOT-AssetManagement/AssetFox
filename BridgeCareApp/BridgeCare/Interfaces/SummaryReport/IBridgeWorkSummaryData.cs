using System.Collections.Generic;
using BridgeCare.Models;

namespace BridgeCare.Interfaces.SummaryReport
{
    public interface IBridgeWorkSummaryData
    {
        List<InvestmentLibraryBudgetYearModel> GetYearlyBudgetModels(int simulationId, BridgeCareContext dbContext);

        Dictionary<int, List<double>> GetYearlyBudgetAmounts(int simulationId, List<int> simulationYears, BridgeCareContext dbContext);
    }
}
